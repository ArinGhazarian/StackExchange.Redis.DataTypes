using StackExchange.Redis.DataTypes.Collections;

namespace StackExchange.Redis.DataTypes
{
	public interface IRedisTypeFactory
	{
		RedisDictionary<TKey, TValue> GetDictionary<TKey, TValue>(string name);
		RedisSet<T> GetSet<T>(string name);
		RedisList<T> GetList<T>(string name);
	}
}
