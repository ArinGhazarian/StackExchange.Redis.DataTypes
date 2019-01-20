using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StackExchange.Redis.DataTypes.Collections
{
    public class RedisDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>
    {
        private const string RedisKeyTemplate = "Dictionary:{0}";

        private static Exception KeyNotFoundException = new KeyNotFoundException("The given key was not present in the dictionary.");
        private static Exception KeyNullException = new ArgumentNullException("key", "Value cannot be null.");
        private static Exception KeyAlreadyExistsException = new ArgumentException("An item with the same key has already been added.");

        private readonly IDatabase database;
        private readonly string redisKey;

        public RedisDictionary(IDatabase database, string name)
        {
            this.database = database ?? throw new ArgumentNullException(nameof(database));
            this.redisKey = string.Format(RedisKeyTemplate, name ?? throw new ArgumentNullException(nameof(name)));
        }

        public void Add(TKey key, TValue value)
        {
            if (ContainsKey(key))
            {
                throw KeyAlreadyExistsException;
            }

            Set(key, value);
        }

        public bool TryAdd(TKey key, TValue value) => Set(key, value);

        public bool ContainsKey(TKey key)
        {
            if (IsKeyNull(key))
            {
                throw KeyNullException;
            }

            return database.HashExists(redisKey, key.ToRedisValue());
        }

        public ICollection<TKey> Keys => database.HashKeys(redisKey).Select(key => key.To<TKey>()).ToList();

        public bool Remove(TKey key)
        {
            if (IsKeyNull(key))
            {
                throw KeyNullException;
            }

            return database.HashDelete(redisKey, key.ToRedisValue());
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (IsKeyNull(key))
            {
                throw KeyNullException;
            }

            value = default(TValue);
            var redisValue = database.HashGet(redisKey, key.ToRedisValue());
            if (redisValue.IsNullOrEmpty)
            {
                return false;
            }
            value = redisValue.To<TValue>();

            return true;
        }

        public ICollection<TValue> Values => database.HashValues(redisKey).Select(val => val.To<TValue>()).ToList();

        public TValue this[TKey key]
        {
            get
            {
                TValue value;
                if (!TryGetValue(key, out value))
                {
                    throw KeyNotFoundException;
                }
                return value;
            }
            set
            {
                Set(key, value);
            }
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        public void Clear() => database.KeyDelete(redisKey);

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            bool keyExists = TryGetValue(item.Key, out TValue value);
            return keyExists && EqualityComparer<TValue>.Default.Equals(item.Value, value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            if (index < 0 || index > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (array.Length - index < this.Count)
            {
                throw new ArgumentException("Destination array is not long enough to copy all the items in the collection. Check array index and length.");
            }

            foreach (var item in this)
            {
                array[index++] = item;
            }
        }

        public int Count
        {
            get
            {
                long count = database.HashLength(redisKey);
                if (count > int.MaxValue)
                {
                    throw new OverflowException("Count exceeds maximum value of integer.");
                }
                return (int)count;
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() =>
            database.HashScan(redisKey)
                    .Select(he => new KeyValuePair<TKey, TValue>(he.Name.To<TKey>(), he.Value.To<TValue>()))
                    .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private bool Set(TKey key, TValue value)
        {
            if (IsKeyNull(key))
            {
                throw KeyNullException;
            }

            return database.HashSet(redisKey, key.ToRedisValue(), value.ToRedisValue());
        }

        private bool IsKeyNull(TKey key) => key == null;
    }
}
