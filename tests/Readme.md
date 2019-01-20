# How to run tests

In order to run the integration tests an instance of Redis should be running locally. The easiest way to run Redis locally is to spin up a docker contiainer. Follow the steps below:

1. Downalod and install [Docker CE](https://docs.docker.com/install/). For Windows and Mac you can also install [Docker Desktop](https://www.docker.com/products/docker-desktop).
2. Get [Redis from the Docker hub](https://hub.docker.com/_/redis) and spin up a Redis container:
    > `docker run --name redis-test -p 6379:6379 -d redis`
3. Run the tests:
    > `dotnet test` 