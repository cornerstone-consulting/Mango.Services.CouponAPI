using AutoMapper;
using Mango.Services.CouponAPI.Data;
using Mango.Services.CouponAPI.Models;
using Mango.Services.CouponAPI.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.CouponAPI.Controllers
{
    /// <summary>
    /// You Will Notice Projects Are Completely Independent Of 
    /// Other API Projects Or Even UI Project
    /// </summary>
    [Route("api/coupon")]//-> this is shown in Swagger
    [ApiController]
    [Authorize]
    public class CouponAPIController : ControllerBase
    {
        private readonly AppDbContext _db;
        private ResponseDto _response;
        private IMapper _mapper;

        //TODO: With Clean Architecture Try To Decouple This
        /// <summary> 
        /// SDE-Observation
        /// You are coupling DbContext with Controllers. This will also couple 
        /// EFCore with application. Changing ORM, will break solution.
        /// Violating Separation Of Concerns principles at architect level
        /// </summary>
        /// <param name="db"></param>
        /// <param name="mapper"></param>
        public CouponAPIController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
            _response = new ResponseDto();
        }

        [HttpGet]//-> internal name for URL
        public ResponseDto Get()
        {
            try
            {
                IEnumerable<Coupon> coupons = _db.Coupons.ToList();
                _response.Result = _mapper.Map<IEnumerable<CouponDto>>(coupons);
            }
            catch (Exception ex)
            {
                _response.Result = StatusCodes.Status500InternalServerError;
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpGet]//-> internal name for URL
        [Route("{id:int}")]
        public ResponseDto Get(int id)
        {
            try
            {
                Coupon coupon = _db.Coupons.First(c => c.Id == id);
                _response.Result = _mapper.Map<CouponDto>(coupon);
            }
            catch (Exception ex)
            {
                _response.Result = StatusCodes.Status500InternalServerError;
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpGet]//-> internal name for URL
        [Route("GetByCode/{code}")]
        public ResponseDto GetByCode(string code)
        {
            try
            {
                Coupon coupon = _db.Coupons.FirstOrDefault(c => c.CouponCode.ToLower() == code.ToLower());
                _response.Result = _mapper.Map<CouponDto>(coupon);
            }
            catch (Exception ex)
            {
                _response.Result = StatusCodes.Status500InternalServerError;
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpPost]//-> internal name for URL
        //[Route("{discount:int}")] //---> No need for this
        [Authorize(Roles = "ADMIN")]
        public ResponseDto Post([FromBody] CouponDto couponDto)
        {
            try
            {
                Coupon coupon = _mapper.Map<Coupon>(couponDto);

                //coupon.DiscountAmount = discount; 
                // -> This line will not work, since value from query-parameter
                //are not mentioned in action method

                _db.Coupons.Add(coupon);
                _db.SaveChanges();
                //_db.SaveChangesAsync(); ---> If use async, id will not be assigned the latest one.
                _response.Result = _mapper.Map<CouponDto>(coupon);
            }
            catch (Exception ex)
            {
                _response.Result = StatusCodes.Status500InternalServerError;
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpPut]//-> internal name for URL
        [Authorize(Roles = "ADMIN")]
        public ResponseDto Put([FromBody] CouponDto couponDto)
        {
            try
            {
                Coupon coupon = _mapper.Map<Coupon>(couponDto);

                // It will used id available in coupon
                _db.Coupons.Update(coupon);
                _db.SaveChanges();
                //_db.SaveChangesAsync(); ---> If use async, id will not be assigned the latest one.
                _response.Result = _mapper.Map<CouponDto>(coupon);
            }
            catch (Exception ex)
            {
                _response.Result = StatusCodes.Status500InternalServerError;
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpDelete]//-> internal name for URL
        [Route("{id:int}")]
        [Authorize(Roles = "ADMIN")]

        public ResponseDto Delete(int id)
        {
            try
            {

                Coupon coupon = _db.Coupons.First(u => u.Id == id);
                // It will used id available in coupon
                _db.Coupons.Remove(coupon);
                _db.SaveChanges();
                //_db.SaveChangesAsync(); ---> If use async, id will not be assigned the latest one.
                //_response.Result = _mapper.Map<CouponDto>(coupon);
            }
            catch (Exception ex)
            {
                _response.Result = StatusCodes.Status500InternalServerError;
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
    }
}
