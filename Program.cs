using AutoMapper;
using Mango.Services.CouponAPI;
using Mango.Services.CouponAPI.Data;
using Mango.Services.CouponAPI.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using DotNetEnv;
using Microsoft.Extensions.Options;

bool isRunningInContainer =
    Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

if (!isRunningInContainer)
{
    Env.Load(".env");
}

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();


var databaseProvider =
    builder.Configuration["Database:Provider"] ?? "SqlServer";

var connectionStringKey =
    databaseProvider.Trim().Equals(
        "PostgreSQL",
        StringComparison.OrdinalIgnoreCase)
        || databaseProvider.Trim().Equals(
        "Postgres",
        StringComparison.OrdinalIgnoreCase)
        ? "PostgreSqlConnection"
        : "DefaultConnection";

var mangoOptions = new MangoOptions
{
    Secret = builder.Configuration["ApiSettings:Secret"] ?? string.Empty,
    Issuer = builder.Configuration["ApiSettings:Issuer"] ?? string.Empty,
    Audience = builder.Configuration["ApiSettings:Audience"] ?? string.Empty,

    DefaultConnection =
        (isRunningInContainer
            ? builder.Configuration[$"Docker:ConnectionStrings:{connectionStringKey}"]
            : builder.Configuration[$"Http:ConnectionStrings:{connectionStringKey}"])
        ?? string.Empty
};

builder.Services.AddSingleton(
    Microsoft.Extensions.Options.Options.Create(mangoOptions));
// Add services to the container.

var connectionString = mangoOptions.DefaultConnection;

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "The database connection string is missing.");
}


var normalizedProvider = databaseProvider.Trim().ToLowerInvariant();

// SQL Server context: retains the existing SQL Server migrations.
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

// PostgreSQL context: will use its own migrations.
builder.Services.AddDbContext<PostgreSqlAppDbContext>(options =>
{
    options.UseNpgsql(
        connectionString,
        npgsqlOptions =>
        {
            npgsqlOptions.MigrationsHistoryTable(
                "__EFMigrationsHistory_PostgreSql");
        });
});

// The controller and startup migration code use this shared base context.
// Resolve the concrete context selected by configuration.
if (normalizedProvider == "sqlserver")
{
    builder.Services.AddScoped<CouponDbContext>(
        serviceProvider =>
            serviceProvider.GetRequiredService<AppDbContext>());
}
else if (normalizedProvider == "postgresql" ||
         normalizedProvider == "postgres")
{
    builder.Services.AddScoped<CouponDbContext>(
        serviceProvider =>
            serviceProvider.GetRequiredService<PostgreSqlAppDbContext>());
}
else
{
    throw new InvalidOperationException(
        $"Unsupported database provider: {databaseProvider}");
}


IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition(name: JwtBearerDefaults.AuthenticationScheme, securityScheme: new OpenApiSecurityScheme()
    {
        //This name is shown in Authorize pop up of CouponAPI Swagger
        Name = "Authorization",
        Description = "Enter the Bearer Authorization string as following: `Bearer Generated-JWT-Token`",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {Type= ReferenceType.SecurityScheme,
                Id=JwtBearerDefaults.AuthenticationScheme}
            }, new string[] {} //TODO: Why this is empty here
        }
    }); 
});

//Adding Authentication
var key = Encoding.ASCII.GetBytes(mangoOptions.Secret);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    x.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey =
            new SymmetricSecurityKey(key),

        ValidateIssuer = true,
        ValidIssuer = mangoOptions.Issuer,

        ValidateAudience = true,
        ValidAudience = mangoOptions.Audience
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

ApplyMigration();

app.Run();



void ApplyMigration()
{
    using (var scope = app.Services.CreateScope())
    {
        var _db = scope.ServiceProvider.GetRequiredService<CouponDbContext>();

        if (_db.Database.GetPendingMigrations().Count() > 0)
        {
            _db.Database.Migrate();
        }
    }
}

public class MangoOptions
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string DefaultConnection { get; set; } = string.Empty;
}