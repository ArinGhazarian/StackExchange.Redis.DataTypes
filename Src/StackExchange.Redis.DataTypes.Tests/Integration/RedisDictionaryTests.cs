using FluentAssertions;
using Xunit;
using StackExchange.Redis.DataTypes.Tests.TestObjects;

namespace StackExchange.Redis.DataTypes.Tests.Integration
{
    public class RedisDictionaryTests : IClassFixture<ConnectionMultiplexerFixture>
    {
        private readonly IRedisTypeFactory redisTypeFactory;

        public RedisDictionaryTests(ConnectionMultiplexerFixture fixture)
        {
            this.redisTypeFactory = new RedisTypeFactory(fixture.ConnectionMultiplexer);
        }

        [Fact]
        public void RedisDictionary_Should_Add_Items()
        {
            // arrange
			var redisDictionary = redisTypeFactory.GetDictionary<int, Person>("Person");
            redisDictionary.Clear();
			
            // act
			redisDictionary.Add(1, new Person { Id = 1, Name = "Steve", Age = 20 });
			redisDictionary.Add(2, new Person { Id = 2, Name = "Mike", Age = 25 });
			redisDictionary.Add(3, new Person { Id = 3, Name = "Lara", Age = 30 });

            // assert
            redisDictionary.Count.Should().Be(3);
            redisDictionary[1].Id.Should().Be(1);
            redisDictionary[1].Name.Should().Be("Steve");
            redisDictionary[1].Age.Should().Be(20);
        }       
    }
}