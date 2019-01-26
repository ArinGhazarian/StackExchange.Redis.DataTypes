using StackExchange.Redis.DataTypes.Collections;
using System;

namespace StackExchange.Redis.DataTypes
{
    public class RedisTypeFactory : IRedisTypeFactory
    {
        private readonly IDatabase database;

        public RedisTypeFactory(IConnectionMultiplexer connectionMultiplexer)
        {
            this.database = connectionMultiplexer?.GetDatabase() ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
        }

        public RedisDictionary<TKey, TValue> GetDictionary<TKey, TValue>(string name) => new RedisDictionary<TKey, TValue>(database, name);

        public RedisSet<T> GetSet<T>(string name) => new RedisSet<T>(database, name);

        public RedisList<T> GetList<T>(string name) => new RedisList<T>(database, name);
    }
}
