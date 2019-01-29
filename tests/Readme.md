# How to run the tests

In order to run the integration tests, an instance of Redis should be running locally. The easiest way to run Redis locally is to spin up a docker contiainer. In order to do so, follow the steps below:

1. Downalod and install [Docker CE](https://docs.docker.com/install/).
2. Get [Redis from the Docker hub](https://hub.docker.com/_/redis) and spin up a Redis container:
    ```sh
    docker run --name redis-test -p 6379:6379 -d redis
    ```
  
    _Note: The above script needs to be run just once. Once you successfully configured and spun up a redis container, you can simply start it every time you want to run the tests:_
    ```sh
    docker start redis-test
    ```
3. Run the tests:
    ```sh
    dotnet test
    ```