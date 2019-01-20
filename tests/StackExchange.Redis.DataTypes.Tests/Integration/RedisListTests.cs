using System;
using System.Collections.Generic;
using StackExchange.Redis.DataTypes.Collections;
using StackExchange.Redis.DataTypes.Tests.TestObjects;
using FluentAssertions;
using Xunit;

namespace StackExchange.Redis.DataTypes.Tests.Integration
{
    public class RedisListTests : IClassFixture<ConnectionMultiplexerFixture>
    {
        private const string ARGUMENT_OUT_OF_RANGE_EXCEPTION_MESSAGE = "Index must be within the bounds of the List.\nParameter name: index";
        private readonly IConnectionMultiplexer connectionMultiplexer;
        
        public RedisListTests(ConnectionMultiplexerFixture fixture)
        {
            this.connectionMultiplexer = fixture.ConnectionMultiplexer;
        }

        [Fact]
        public void Add_Should_Add_Item()
        {
            var list = GetList<Person>();

            list.Add(new Person { Id = 1, Name = "Lars", Age = 50 });

            list.Count.Should().Be(1);
            list[0].Id.Should().Be(1);
            list[0].Name.Should().Be("Lars");
            list[0].Age.Should().Be(50);
        }

        [Fact]
        public void Insert_Should_Insert_At_Index()
        {
            var list = GetList<int>();
            
            list.Add(1);
            list.Add(2);
            list.Insert(0, -1);
            list.Insert(1, -2);

            list.Count.Should().Be(2);
            list[0].Should().Be(-1);
            list[1].Should().Be(-2);
        }

        [Fact]
        public void Insert_Should_Throw_If_Index_Out_Of_Range()
        {
            var list = GetList<int>();

            list.Add(1);
            
            list
                .Invoking(l => l.Insert(1, 1))
                .Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithMessage(ARGUMENT_OUT_OF_RANGE_EXCEPTION_MESSAGE);
        }

        [Fact]
        public void Insert_Should_Throw_Argument_Out_Of_Range_Exception_If_No_List_Yet()
        {
            var list = GetList<int>();

            list
                .Invoking(l => l.Insert(1, 1))
                .Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithMessage(ARGUMENT_OUT_OF_RANGE_EXCEPTION_MESSAGE);
        }

        [Fact]
        public void IndexOf_Should_Return_Index_If_Found()
        {
            var list = GetList<int>();

            list.Add(1);
            list.Add(2);
            list.Add(3);

            list.IndexOf(2).Should().Be(1);
        }

        [Fact]
        public void IndexOf_Should_Return_Minus_One_If_Not_Found()
        {
            var list = GetList<int>();

            list.Add(1);
            list.Add(2);

            list.IndexOf(3).Should().Be(-1);
        }

        [Fact]
        public void Count_Should_Be_Zero_For_Empty_List()
        {
            var list = GetList<int>();

            list.Count.Should().Be(0);
        }

        [Fact]
        public void RemoteAt_Should_Remove_Element_At_Index()
        {
            var list = GetList<int>();

            list.Add(1);
            list.Add(2);

            list.Count.Should().Be(2);
            list.RemoveAt(1);
            list.Count.Should().Be(1);
            list[0].Should().Be(1);
        }

        [Fact]
        public void RemoveAt_Should_Throw_If_Index_Not_Found()
        {
            var list = GetList<int>();

            list.Add(1);
    
            list
                .Invoking(l => l.RemoveAt(1))
                .Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithMessage(ARGUMENT_OUT_OF_RANGE_EXCEPTION_MESSAGE);
        }

        [Fact]
        public void RemoveAt_Should_Throw_Argument_Out_Of_Range_Exception_If_No_List_Yet()
        {
            var list = GetList<int>();

            list
                .Invoking(l => l.RemoveAt(1))
                .Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithMessage(ARGUMENT_OUT_OF_RANGE_EXCEPTION_MESSAGE);
        }

        [Fact]
        public void Indexer_Should_Get_Element_At_Index()
        {
            var list = GetList<int>();

            list.Add(1);

            list[0].Should().Be(1);
        }

        [Fact]
        public void Indexer_Should_Throw_If_Index_Not_Found()
        {
            var list = GetList<int>();

            list.Add(1);

            list
                .Invoking(l => { var item = l[1]; })
                .Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithMessage(ARGUMENT_OUT_OF_RANGE_EXCEPTION_MESSAGE);
        }

        [Fact]
        public void Indexer_Should_Throw_Argument_Out_Of_Range_Exception_If_No_List_Yet()
        {
            var list = GetList<int>();

            list
                .Invoking(l => { var item = l[1]; })
                .Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithMessage(ARGUMENT_OUT_OF_RANGE_EXCEPTION_MESSAGE);
        }

        [Fact]
        public void Clear_Should_Empty_Out_The_List()
        {
            var list = GetList<int>();
            
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Count.Should().Be(3);

            list.Clear();

            list.Count.Should().Be(0);
        }

        [Fact]
        public void Contains_Should_Return_True_If_Found()
        {
            var list = GetList<int>();

            list.Add(1);
            list.Add(2);

            list.Contains(1).Should().BeTrue();
        }

        [Fact]
        public void Contains_Should_Return_False_If_Not_Found()
        {
            var list = GetList<int>();

            list.Add(1);
            list.Add(2);

            list.Contains(-1).Should().BeFalse();
        }

        [Fact]
        public void ICollection_CopyTo_Should_Copy_All_Elements_Into_Array()
        {
            var list = GetList<string>();

            list.Add("one");
            list.Add("two");
            list.Add("three");

            var array = new string[3];
            (list as ICollection<string>).CopyTo(array, 0);

            array.Length.Should().Be(3);
            array[0].Should().Be("one");
            array[1].Should().Be("two");
            array[2].Should().Be("three");
        }

        [Fact]
        public void Count_Should_Return_Number_Of_Items()
        {
            var list = GetList<int>();

            list.Add(1);
            list.Add(2);
            list.Add(3);

            list.Count.Should().Be(3);
        }

        [Fact]
        public void Remove_Should_Remove_First_Occurrence_Of_An_Element()
        {
            var list = GetList<int>();

            list.Add(1);
            list.Add(1);
            list.Add(2);
            list.Add(3);

            list.Remove(1);
            
            list.Count.Should().Be(3);
            list[0].Should().Be(1);
            list[1].Should().Be(2);
            list[2].Should().Be(3);
        }
        
        [Fact]
        public void Ctor_Should_Throw_If_Database_Is_Null()
        {
            Action action = () => new RedisList<int>(null, "list");
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage("Value cannot be Null.\nParameter name: database");
        }

        [Fact]
        public void Ctor_Should_Throw_If_Name_Is_Null()
        {
            Action action = () => new RedisList<int>(connectionMultiplexer.GetDatabase(), null);
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage("Value cannot be Null.\nParameter name: name");
        }

        private RedisList<T> GetList<T>()
        {
            var list = new RedisList<T>(connectionMultiplexer.GetDatabase(), "list");
            list.Clear();
            return list;
        }
    }
}