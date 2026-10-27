# Running Docker - via any terminal

## Building image
docker build -t mango-couponapi-local:dev .

## Building container 
docker run --name mango-couponapi --env-file .env -p 5104:8080 mango-couponapi-local:dev
