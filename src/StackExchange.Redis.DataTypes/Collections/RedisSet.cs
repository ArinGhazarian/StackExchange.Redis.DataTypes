using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StackExchange.Redis.DataTypes.Collections
{
    public class RedisSet<T> : ISet<T>
    {
        private const string RedisKeyTemplate = "Set:{0}";

        private readonly IDatabase database;
        private readonly string redisKey;

        public RedisSet(IDatabase database, string name)
        {
            this.database = database ?? throw new ArgumentNullException(nameof(database));
            this.redisKey = string.Format(RedisKeyTemplate, name ?? throw new ArgumentNullException(nameof(name)));
        }

        public bool Add(T item) => database.SetAdd(redisKey, item.ToRedisValue());

        public long Add(IEnumerable<T> items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            if (!items.Any())
            {
                return 0;
            }

            return database.SetAdd(redisKey, items.ToRedisValues());
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            SetCombineAndStore(SetOperation.Difference, other);
        }

        public void ExceptWith(RedisSet<T> other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            SetCombineAndStore(SetOperation.Difference, other);
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            SetCombineAndStore(SetOperation.Intersect, other);
        }

        public void IntersectWith(RedisSet<T> other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            SetCombineAndStore(SetOperation.Intersect, other);
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return Count < other.Count() && IsSubsetOf(other);
        }

        public bool IsProperSubsetOf(RedisSet<T> other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return Count < other.Count && IsSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return Count > other.Count() && IsSupersetOf(other);
        }

        public bool IsProperSupersetOf(RedisSet<T> other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return Count > other.Count && IsSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return !this.Except(other).Any();
        }

        public bool IsSubsetOf(RedisSet<T> other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return !SetCombine(SetOperation.Difference, other).Any();
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return !other.Except(this).Any();
        }

        public bool IsSupersetOf(RedisSet<T> other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return !other.SetCombine(SetOperation.Difference, this).Any();
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            foreach (var item in other)
            {
                if (Contains(item))
                {
                    return true;
                }
            }

            return false;
        }

        public bool Overlaps(RedisSet<T> other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return SetCombine(SetOperation.Intersect, other).Any();
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return IsSubsetOf(other) && IsSupersetOf(other);
        }

        public bool SetEquals(RedisSet<T> other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return IsSubsetOf(other) && IsSupersetOf(other);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            var otherSet = new RedisSet<T>(database, Guid.NewGuid().ToString());
            try
            {
                otherSet.Add(other);
                SymmetricExceptWith(otherSet);
            }
            finally
            {
                otherSet.Clear();
            }
        }

        public void SymmetricExceptWith(RedisSet<T> other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            var intersectedSet = new RedisSet<T>(database, Guid.NewGuid().ToString());
            try
            {
                SetCombineAndStore(SetOperation.Intersect, intersectedSet, this, other);
                SetCombineAndStore(SetOperation.Union, other);
                SetCombineAndStore(SetOperation.Difference, intersectedSet);
            }
            finally
            {
                intersectedSet.Clear();
            }
        }

        public void UnionWith(IEnumerable<T> other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            SetCombineAndStore(SetOperation.Union, other);
        }

        public void UnionWith(RedisSet<T> other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            SetCombineAndStore(SetOperation.Union, other);
        }

        void ICollection<T>.Add(T item) => Add(item);

        public void Clear() => database.KeyDelete(redisKey);

        public bool Contains(T item) => database.SetContains(redisKey, item.ToRedisValue());

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
                long count = database.SetLength(redisKey);
                if (count > int.MaxValue)
                {
                    throw new OverflowException("Count exceeds maximum value of integer.");
                }
                return (int)count;
            }
        }

        bool ICollection<T>.IsReadOnly => false;

        public bool Remove(T item) => database.SetRemove(redisKey, item.ToRedisValue());

        public IEnumerator<T> GetEnumerator() =>
            database.SetScan(redisKey)
                    .Select(redisValue => redisValue.To<T>())
                    .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private void SetCombineAndStore(SetOperation operation, IEnumerable<T> other)
        {
            var redisTempSet = new RedisSet<T>(database, Guid.NewGuid().ToString());
            try
            {
                redisTempSet.Add(other);
                SetCombineAndStore(operation, redisTempSet);
            }
            finally
            {
                redisTempSet.Clear();
            }
        }

        private void SetCombineAndStore(SetOperation operation, RedisSet<T> other)
        {
            SetCombineAndStore(operation, this, this, other);
        }

        private void SetCombineAndStore(SetOperation operation, RedisSet<T> destination, RedisSet<T> first, RedisSet<T> second)
        {
            database.SetCombineAndStore(operation, destination.redisKey, first.redisKey, second.redisKey);
        }

        private RedisValue[] SetCombine(SetOperation operation, RedisSet<T> other) =>
            database.SetCombine(operation, redisKey, other.redisKey);
    }
}
