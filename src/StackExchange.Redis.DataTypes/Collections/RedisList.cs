using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StackExchange.Redis.DataTypes.Collections
{
    public class RedisList<T> : IList<T>
    {
        private const string RedisKeyTemplate = "List:{0}";
        private const string RedisIndexOutOfRangeExceptionMessage = "ERR index out of range";
        private const string RedisNoSuchKeyExceptionMessage = "ERR no such key";

        private static Exception IndexOutOfRangeException = new ArgumentOutOfRangeException("index", "Index must be within the bounds of the List.");

        private readonly IDatabase database;
        private readonly string redisKey;

        public RedisList(IDatabase database, string name)
        {
            this.database = database ?? throw new ArgumentNullException(nameof(database));
            this.redisKey = string.Format(RedisKeyTemplate, name ?? throw new ArgumentNullException(nameof(name)));
        }

        public int IndexOf(T item)
        {
            int index = 0;
            foreach (var member in this)
            {
                if (EqualityComparer<T>.Default.Equals(member, item))
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        public void Insert(int index, T item)
        {
            try
            {
                database.ListSetByIndex(redisKey, index, item.ToRedisValue());
            }
            catch (RedisServerException redisServerException)
            when (IsIndexOutOfRangeExcepiton(redisServerException) || IsNoSuchKeyExcption(redisServerException))
            {
                throw IndexOutOfRangeException;
            }
        }

        public void RemoveAt(int index)
        {
            string deleteFlag = Guid.NewGuid().ToString();
            try
            {
                database.ListSetByIndex(redisKey, index, deleteFlag);
            }
            catch (RedisServerException redisServerException)
			when (IsIndexOutOfRangeExcepiton(redisServerException) || IsNoSuchKeyExcption(redisServerException))
            {
                throw IndexOutOfRangeException;
            }
            database.ListRemove(redisKey, deleteFlag, flags: CommandFlags.FireAndForget);
        }

        public T this[int index]
        {
            get
            {
                var redisValue = database.ListGetByIndex(redisKey, index);
                if (!redisValue.HasValue)
                {
                    throw IndexOutOfRangeException;
                }
                return redisValue.To<T>();
            }
            set
            {
                Insert(index, value);
            }
        }

        public void Add(T item) => database.ListRightPush(redisKey, item.ToRedisValue());

        public void Clear() => database.KeyDelete(redisKey);

        public bool Contains(T item) => IndexOf(item) != -1;

        void ICollection<T>.CopyTo(T[] array, int index)
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
                long count = database.ListLength(redisKey);
                if (count > int.MaxValue)
                {
                    throw new OverflowException("Count exceeds maximum value of integer.");
                }
                return (int)count;
            }
        }

        public bool IsReadOnly => false;

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index != -1)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public IEnumerator<T> GetEnumerator() => new Enumerator(this);

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        private class Enumerator : IEnumerator<T>
        {
            private int index;
            private T current;
            private int listSize;
            private RedisList<T> redisList;

            public Enumerator(RedisList<T> redisList)
            {
                this.redisList = redisList;
                this.index = 0;
                this.listSize = redisList.Count;
                this.current = default(T);
            }

            public T Current
            {
                get
                {
                    return current;
                }
            }

            public void Dispose()
            {
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    if (index == 0 || index == listSize + 1)
                    {
                        throw new InvalidOperationException("Enumeration has either not started or has already finished.");
                    }
                    return Current;
                }
            }

            public bool MoveNext()
            {
                if (index >= listSize)
                {
                    index = listSize + 1;
                    current = default(T);
                    return false;
                }
                current = redisList[index];
                ++index;
                return true;
            }

            public void Reset()
            {
                index = 0;
                current = default(T);
            }
        }

        private bool IsIndexOutOfRangeExcepiton(RedisServerException redisServerException) => 
			redisServerException.Message == RedisIndexOutOfRangeExceptionMessage;

        private bool IsNoSuchKeyExcption(RedisServerException redisServerException) => 
			redisServerException.Message == RedisNoSuchKeyExceptionMessage;
    }
}
