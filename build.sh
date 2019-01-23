#!/bin/bash

docker --version &> /dev/null
if [ $? -eq 0 ]; then
    docker start redis-test 2> /dev/null || docker run --name redis-test -p 6379:6379 -d redis
else
    echo "Docker is not installed. Please install Docker and try again."
fi

dotnet test tests/StackExchange.Redis.DataTypes.Tests

if [[ $? -eq 0 && "$1" == "nuget" ]]; then
    dotnet pack --configuration release
fi