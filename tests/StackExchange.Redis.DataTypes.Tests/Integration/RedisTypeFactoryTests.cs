using System;
using FluentAssertions;
using StackExchange.Redis.DataTypes.Collections;
using Xunit;

namespace StackExchange.Redis.DataTypes.Tests.Integration
{
    public class RedisTypeFactoryTests : IClassFixture<ConnectionMultiplexerFixture>
    {
        private const string ARGUMENT_NULL_EXCEPTION_MESSAGE = "Value cannot be Null.\nParameter name: connectionMultiplexer";
        private readonly IConnectionMultiplexer connectionMultiplexer;

        public RedisTypeFactoryTests(ConnectionMultiplexerFixture fixture)
        {
            this.connectionMultiplexer = fixture.ConnectionMultiplexer;
        }

        [Fact]
        public void Ctor_Throws_If_ConnectionMultiplexer_Is_Null()
        {
            Action action = () => new RedisTypeFactory(null);
            action.Should().Throw<ArgumentNullException>().WithMessage(ARGUMENT_NULL_EXCEPTION_MESSAGE);
        }

        [Fact]
        public void GetDictionary_Returns_A_RedisDictionary()
        {
            var redisTypeFactory = new RedisTypeFactory(connectionMultiplexer);

            var redisDictionary = redisTypeFactory.GetDictionary<int, string>("dic");

            redisDictionary.Should().BeOfType<RedisDictionary<int, string>>();
        }

        [Fact]
        public void GetList_Returns_A_RedisList()
        {
            var redisTypeFactory = new RedisTypeFactory(connectionMultiplexer);

            var redisDictionary = redisTypeFactory.GetList<int>("list");

            redisDictionary.Should().BeOfType<RedisList<int>>();
        }

        [Fact]
        public void GetSet_Returns_A_RedisSet()
        {
            var redisTypeFactory = new RedisTypeFactory(connectionMultiplexer);

            var redisDictionary = redisTypeFactory.GetSet<string>("set");

            redisDictionary.Should().BeOfType<RedisSet<string>>();
        }
    }
}