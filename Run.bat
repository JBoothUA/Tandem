dotnet dev-certs https --clean
dotnet dev-certs https --trust

start /d "." dotnet run --project ./TandemUserService/TandemUserService.csproj
start /d "." dotnet run --project ./IntegrationTests/IntegrationTests.csproj