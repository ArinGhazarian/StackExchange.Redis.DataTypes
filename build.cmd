@echo off

docker --version > nul 2>&1
if %errorlevel% neq 0 (
    echo Docker is not installed. Please install Docker and try again.
    exit 1
)

docker start redis-test 2> nul
if %errorlevel% neq 0 (
    docker run --name redis-test -p 6379:6379 -d redis
)

dotnet test tests/StackExchange.Redis.DataTypes.Tests

if %errorlevel% equ 0 (
    if "%~1"=="nuget" (
        dotnet pack --configuration release
    ) 
)