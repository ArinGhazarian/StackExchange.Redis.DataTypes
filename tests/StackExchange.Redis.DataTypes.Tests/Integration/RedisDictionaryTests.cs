using System;
using FluentAssertions;
using StackExchange.Redis.DataTypes.Tests.TestObjects;
using StackExchange.Redis.DataTypes.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace StackExchange.Redis.DataTypes.Tests.Integration
{
    public class RedisDictionaryTests : IClassFixture<ConnectionMultiplexerFixture>
    {
        private const string ARGUMENT_EXCEPTION_MESSAGE = "An item with the same key has already been added.";
        private const string KEY_NOT_FOUND_EXCEPTION_MESSAGE = "The given key was not present in the dictionary.";
        private const string ARGUMENT_NULL_EXCEPTION_MESSAGE = "Value cannot be Null.\nParameter name: key";
        private readonly IConnectionMultiplexer connectionMultiplexer;

        public RedisDictionaryTests(ConnectionMultiplexerFixture fixture)
        {
            this.connectionMultiplexer = fixture.ConnectionMultiplexer;
        }

        [Fact]
        public void Add_Should_Successfully_Add_Objects()
        {
            // arrange
			var redisDictionary = new RedisDictionary<int, Person>(connectionMultiplexer.GetDatabase(), "Person");
            redisDictionary.Clear();
			
            // act
			redisDictionary.Add(1, new Person { Id = 1, Name = "Steve", Age = 20 });
            redisDictionary.Add(2, new Person { Id = 2, Name = "Lara", Age = 25 });

            // assert
            redisDictionary.Count.Should().Be(2);

            redisDictionary[1].Id.Should().Be(1);
            redisDictionary[1].Name.Should().Be("Steve");
            redisDictionary[1].Age.Should().Be(20);
            
            redisDictionary[2].Id.Should().Be(2);
            redisDictionary[2].Name.Should().Be("Lara");
            redisDictionary[2].Age.Should().Be(25);
        }

        [Fact]
        public void Add_Should_Throw_When_Duplicate_Keys()
        {
            var redisDictionary = GetDictionary();
            redisDictionary.Clear();

            redisDictionary.Add(1, "one");

            redisDictionary
                .Invoking(dic => dic.Add(1, "duplicate"))
                .Should()
                .Throw<ArgumentException>()
                .WithMessage(ARGUMENT_EXCEPTION_MESSAGE);
        }

        [Fact]
        public void Add_Should_Throw_If_Key_Is_Null()
        {
            var dic = new RedisDictionary<int?, int>(connectionMultiplexer.GetDatabase(), "dic");

            dic
                .Invoking(d => d.Add(null, 1))
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(ARGUMENT_NULL_EXCEPTION_MESSAGE);
        }

        [Fact]
        public void TryAdd_Should_Add_And_Return_False_For_Duplicates()
        {
            var redisDictionary = GetDictionary();

            var res = redisDictionary.TryAdd(1, "one");
            res.Should().BeTrue();

            res = redisDictionary.TryAdd(1, "duplicate");
            res.Should().BeFalse();
        }

        [Fact]
        public void TryAdd_Should_Throw_If_Key_Is_Null()
        {
            var dic = new RedisDictionary<int?, int>(connectionMultiplexer.GetDatabase(), "dic");

            dic
                .Invoking(d => d.TryAdd(null, 1))
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(ARGUMENT_NULL_EXCEPTION_MESSAGE);
        }

        [Fact]
        public void ContainsKey_Should_Return_True_If_Finds_The_Key()
        {
            var redisDictionary = GetDictionary();

            redisDictionary.Add(1, "one");

            redisDictionary.ContainsKey(1).Should().BeTrue();
        }

        [Fact]
        public void ContainsKey_Should_Return_False_If_Could_Not_Find_The_Key()
        {
            var redisDictionary = GetDictionary();

            redisDictionary.Add(1, "One");

            redisDictionary.ContainsKey(2).Should().BeFalse();
        }

        [Fact]
        public void ContainsKey_Should_Throw_If_Key_Is_Null()
        {
            var dic = new RedisDictionary<int?, string>(connectionMultiplexer.GetDatabase(), "dic");

            dic
                .Invoking(d => d.ContainsKey(null))
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(ARGUMENT_NULL_EXCEPTION_MESSAGE);
        }

        [Fact]
        public void Keys_Should_Return_All_The_Keys()
        {
            var dic = GetDictionary();

            dic.Add(1, "one");
            dic.Add(2, "two");
            dic.Add(3, "three");

            dic.Keys.Count().Should().Be(3);
            dic.Keys.Should().BeEquivalentTo(new[] { 1, 2, 3 });
        }

        [Fact]
        public void Remove_Should_Remove_The_Key()
        {
            var dic = GetDictionary();

            dic.Add(1, "one");
            dic.Add(2, "two");

            dic.Count().Should().Be(2);
            dic.Remove(1);
            dic.Count().Should().Be(1);
            dic[2].Should().Be("two");
        }

        [Fact]
        public void Remove_Should_Throw_If_Key_Is_Null()
        {
            var dic = new RedisDictionary<int?, int>(connectionMultiplexer.GetDatabase(), "dic");

            dic
                .Invoking(d => d.Remove(null))
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(ARGUMENT_NULL_EXCEPTION_MESSAGE);
        }

        [Fact]
        public void TryGetValue_Should_Get_Corresponding_Value_Of_A_Key()
        {
            var dic = GetDictionary();

            dic.Add(1, "one");

            var result = dic.TryGetValue(1, out string value);
            result.Should().BeTrue();
            value.Should().Be("one");
        }

        [Fact]
        public void TryGetValue_Should_Return_False_If_Key_Not_Found()
        {
            var dic = GetDictionary();

            dic.Add(1, "one");

            var result = dic.TryGetValue(2, out string value);
            result.Should().BeFalse();
            value.Should().BeNull();
        }

        [Fact]
        public void TryGetValue_Should_Throw_If_Key_Is_Null()
        {
            var dic = new RedisDictionary<int?, int>(connectionMultiplexer.GetDatabase(), "dic");

            dic
                .Invoking(d => d.TryGetValue(null, out int value))
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(ARGUMENT_NULL_EXCEPTION_MESSAGE);
        }

        [Fact]
        public void Values_Should_Return_All_The_Values()
        {
            var dic = GetDictionary();

            dic.Add(1, "one");
            dic.Add(2, "two");
            dic.Add(3, "threee");

            dic.Values.Should().BeEquivalentTo(new[] { "one", "two", "threee" });
        }

        [Fact]
        public void Clear_Should_Empty_The_Dictionary()
        {
            var dic = GetDictionary();

            dic.Add(1, "one");
            dic.Count.Should().Be(1);

            dic.Clear();
            dic.Count().Should().Be(0);            
        }

        [Fact]
        public void ICollection_Contains_Should_Find_Existing_Key_Value_Paie()
        {
            var dic = new RedisDictionary<int, Person>(connectionMultiplexer.GetDatabase(), "Person");
            dic.Clear();

            dic.Add(1, new Person { Id = 1, Name = "John", Age = 25 });
            
            (dic as ICollection<KeyValuePair<int, Person>>)
                .Contains(new KeyValuePair<int, Person>(1, new Person { Id = 1, Name = "John", Age = 25 }))
                .Should().BeTrue();
        }

        [Fact]
        public void ICollection_CopyTo_Should_Copy_All_Elements_Into_Array()
        {
            var dic = GetDictionary();

            dic.Add(1, "one");
            dic.Add(2, "two");

            var arr = new KeyValuePair<int, string>[2];
            (dic as ICollection<KeyValuePair<int, string>>).CopyTo(arr, 0);

            arr[0].Should().Be(new KeyValuePair<int, string>(1, "one"));
            arr[1].Should().Be(new KeyValuePair<int, string>(2, "two"));
        }

        [Fact]
        public void Count_Should_Return_Number_Of_Items()
        {
            var dic = GetDictionary();

            dic.Add(1, "one");
            dic.Add(2, "two");
            dic.Add(3, "three");

            dic.Count.Should().Be(3);
        }

        [Fact]
        public void Indexer_Should_Throw_If_Key_Not_Found()
        {
            var dic = GetDictionary();

            dic.Add(1, "one");

            dic
                .Invoking(d => { var item = d[2]; })
                .Should()
                .Throw<KeyNotFoundException>()
                .WithMessage(KEY_NOT_FOUND_EXCEPTION_MESSAGE);
        }

        private RedisDictionary<int, string> GetDictionary()
        {
            var dic = new RedisDictionary<int, string>(connectionMultiplexer.GetDatabase(), "dic");
            dic.Clear();
            return dic;
        }
    }
}