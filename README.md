# StackExchange.Redis.DataTypes
Implementation of common .NET collection types (i.e IDictionary, ISet, IList) via abstraction over StackExchange.Redis. By using this library you will be able to seamlessly replace your common *in-memory* collection types with types stored in redis db. All you need to do is to instantiate a redis collection type and continue coding as if you are working with common *in-memory* collections.
# How to get started?
This library uses [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis) to communicate with redis db and seialize/deserialize the data using [Json.NET](https://github.com/JamesNK/Newtonsoft.Json) library. You don't need to worry about serialization/deserialization of your data because it works under the hood but you need to be familiar with [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis). Well at least you need to know how to configure and instantiate [ConnectionMultiplexer](https://github.com/StackExchange/StackExchange.Redis/blob/master/Docs/Basics.md). After you learned the [Basic usage of StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis/blob/master/Docs/Basics.md) you can start using this library.
# Some code samples

* Create a ConnectionMultiplexer. Creating ConnectionMultiplexer object is costly so it is recommended to store and reuse it.

```C#
var connectionMultiplexer = ConnectionMultiplexer.Connect("localhost,abortConnect=false"); // replace localhost with your redis db address
```

* You can either create a RedisType by using RedisTypeFactory or by instantiating the desired type directly. 

```C#
var redisTypeFactory = new RedisTypeFactory(connectionMultiplexer);
```

* Adding items to dictionary and iterate through it

```C#
redisDictionary.Add(1, new Person { ID = 1, Name = "Steve", Age = 20 });
redisDictionary.Add(2, new Person { ID = 2, Name = "Mike", Age = 25 });
redisDictionary.Add(3, new Person { ID = 3, Name = "Lara", Age = 30 });

foreach (var person in redisDictionary)
{
	Console.WriteLine("ID: {0}, Name: {1}, Age: {2}", person.Value.ID, person.Value.Name, person.Value.Age);
}
```

* Find a specific person

```C#
var lara = redisDictionary.First(kvp => kvp.Value.Name == "Lara");
Console.WriteLine("Lara's Age: {0}", lara.Value.Age);
```

* Delete a person

```C#
redisDictionary.Remove(lara.Key);
```

* Delete the Person dictionary from redis

```C#
redisDictionary.Clear();
```

* Creating a redis list without factory

```C#
var redisList = new RedisList<int>(connectionMultiplexer.GetDatabase(), "Numbers");
```

* Adding some numbers to redis list and iterate through it

```C#
for (int i = 0; i < 10; i++)
{
	redisList.Add(i);
}

foreach (var number in redisList)
{
	Console.WriteLine(number);
}
```

* Delete the Numbers list from redis

```C#
redisList.Clear();
```

* Using a DI container (This sample used [Unity](https://github.com/unitycontainer/unity))

```C#
var container = new UnityContainer();

// Register connectionMultiplexer as a singleton instance
container.RegisterInstance<IConnectionMultiplexer>(connectionMultiplexer);
// Register RedisTypeFactory
container.RegisterType<IRedisTypeFactory, RedisTypeFactory>();
// Resolve an IRedisTypeFacoty
var factory = container.Resolve<IRedisTypeFactory>();

// Get a redis set from factory and add some members to it
var redisSet = factory.GetSet<int>("NumbersSet");
redisSet.Add(1);
redisSet.Add(1);
redisSet.Add(2);
```

