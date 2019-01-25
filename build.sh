#!/bin/bash

docker --version &> /dev/null
if [ $? -ne 0 ]; then
    echo "Docker is not installed. Please install Docker and try again."
    exit 1
fi

docker start redis-test 2> /dev/null || docker run --name redis-test -p 6379:6379 -d redis

dotnet test tests/StackExchange.Redis.DataTypes.Tests

if [[ $? -eq 0 && "$1" == "nuget" ]]; then
    dotnet pack --configuration release
fi