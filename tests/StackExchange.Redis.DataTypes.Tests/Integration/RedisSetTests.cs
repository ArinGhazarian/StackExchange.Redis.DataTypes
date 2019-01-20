using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using StackExchange.Redis.DataTypes.Collections;
using StackExchange.Redis.DataTypes.Tests.TestObjects;
using Xunit;

namespace StackExchange.Redis.DataTypes.Tests.Integration
{
    public class RedisSetTests : IClassFixture<ConnectionMultiplexerFixture>
    {
        private readonly IConnectionMultiplexer connectionMultiplexer;

        public RedisSetTests(ConnectionMultiplexerFixture fixture)
        {
            this.connectionMultiplexer = fixture.ConnectionMultiplexer;    
        }

        [Fact]
        public void Add_Should_Add_Non_Existent_Item()
        {
            var set = GetSet<int>();

            var result = set.Add(1);

            set.Count.Should().Be(1);
            result.Should().BeTrue();
        }

        [Fact]
        public void Add_Should_Not_Add_Existing_Item()
        {
            var set = GetSet<Person>();

            var result = set.Add(new Person { Id = 1, Name = "Holly", Age = 21 });
            result.Should().BeTrue();
            set.Count.Should().Be(1);

            result = set.Add(new Person { Id = 1, Name = "Holly", Age = 21 });
            result.Should().BeFalse();
            set.Count.Should().Be(1);
        }

        [Fact]
        public void Add_Should_Add_A_Collection_Of_Items()
        {
            var set = GetSet<int>();

            var result = set.Add(new[] { 1, 1, 2, 2, 3, 3 });
            
            result.Should().Be(3);
            set.Count.Should().Be(3);
        }

        [Fact]
        public void Add_Should_Return_Zero_When_Adding_Empty_Collection()
        {
            var set = GetSet<int>();

            var result = set.Add(new int[] { });
            
            result.Should().Be(0);
            set.Count.Should().Be(0);
        }

        [Fact]
        public void Add_Should_Return_Zero_When_There_Is_No_New_Item()
        {
            var set = GetSet<int>(1, 2, 3, 4, 5);

            var result = set.Add(new[] { 1, 2, 3 });

            result.Should().Be(0);
            set.Count.Should().Be(5);
        }

        [Fact]
        public void Add_Should_Throw_If_Items_Collection_Is_Null()
        {
            var set = GetSet<int>();

            set
                .Invoking(s => s.Add((IEnumerable<int>)null))
                .Should()
                .Throw<ArgumentNullException>();
        }

        [Fact]
        public void Clear_Should_Empty_Out_The_Set()
        {
            var set = GetSet<int>();

            var result = set.Add(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            result.Should().Be(9);
            set.Count.Should().Be(9);

            set.Clear();

            set.Count.Should().Be(0);
        }

        [Fact]
        public void Count_Should_Return_Number_Of_Items()
        {
            var set = GetSet<int>();

            var result = set.Add(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });

            result.Should().Be(9);
            set.Count.Should().Be(9);
        }

        [Fact]
        public void Count_Should_Return_Zero_For_Empty_Set()
        {
            var set = GetSet<int>();

            set.Count.Should().Be(0);
        }

        [Fact]
        public void Contains_Should_Find_Existing_Item()
        {
            var set = GetSet<Person>();

            set.Add(new Person { Id = 1, Name = "Holly", Age = 21 });
            set.Add(new Person { Id = 2, Name = "Gretchen", Age = 31 });

            set.Contains(new Person { Id = 2, Name = "Gretchen", Age = 31}).Should().BeTrue();           
        }

        [Fact]
        public void Contains_Should_Not_Find_Not_Existing_Items()
        {
            var set = GetSet<Person>();

            set.Add(new Person { Id = 1, Name = "Holly", Age = 21 });

            set.Contains(new Person { Id = 1, Name = "Gretchen", Age = 21}).Should().BeFalse();
        }

        [Fact]
        public void Remove_Should_Remove_Existing_Item()
        {
            var set = GetSet<dynamic>();

            set.Add(new { Id = 1, Name = "Steve" });
            set.Add(new { Id = 2, Name = "Joe" });
            set.Count.Should().Be(2);

            var result  = set.Remove(new { Id = 2, Name = "Joe" });
            result.Should().BeTrue();
            set.Count.Should().Be(1);
        }

        [Fact]
        public void Remove_Should_Not_Remove_Not_Existing_Item()
        {
            var set = GetSet<dynamic>();

            set.Add(new { Id = 1, Name = "Steve" });
            set.Add(new { Id = 2, Name = "Joe" });
            set.Count.Should().Be(2);

            var result  = set.Remove(new { Id = 1, Name = "Stevenon" });
            result.Should().BeFalse();
            set.Count.Should().Be(2);
        }

        [Fact]
        public void ICollection_CopyTo_Should_Copy_All_Items_Into_Array()
        {
            var set = GetSet<int>();

            set.Add(new[] { 1, 2, 3, 4, 5 });

            var array = new int[5];
            (set as ICollection<int>).CopyTo(array, 0);

            array.Length.Should().Be(5);
            array[0].Should().Be(1);
            array[1].Should().Be(2);
            array[2].Should().Be(3);
            array[3].Should().Be(4);
            array[4].Should().Be(5);
        }

        [Fact]
        public void ExceptWith_Redis_Set_Should_Result_Empty_If_First_Empty_Second_Empty()
        {
            var first = GetSet<int>();
            var second = GetSet<int>();

            first.ExceptWith(second);

            first.Count.Should().Be(0);
        }

        [Fact]
        public void ExceptWith_IEnumerable_Should_Result_Empty_If_First_Empty_Second_Empty()
        {
            var first = GetSet<int>();
            var second = Enumerable.Empty<int>();

            first.ExceptWith(second);

            first.Count.Should().Be(0);
        }

        [Fact]
        public void ExceptWith_Redis_Set_Should_Result_Empty_If_First_Empty_Second_Not_Empty()
        {
            var first = GetSet<int>();
            var second = GetSet<int>(1, 2, 3);

            first.ExceptWith(second);

            first.Count.Should().Be(0);
        }

        [Fact]
        public void ExceptWith_IEnumerable_Should_Result_Empty_If_First_Empty_Second_Not_Empty()
        {
            var first = GetSet<int>();
            var second = new[] { 1, 2, 3 };

            first.ExceptWith(second);

            first.Count.Should().Be(0);
        }

        [Fact]
        public void ExceptWith_Redis_Set_Should_Result_First_If_First_Not_Empty_Second_Empty()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = GetSet<int>();

            first.ExceptWith(second);

            first.Count.Should().Be(3);
            first.Contains(1);
            first.Contains(2);
            first.Contains(3);
        }

        [Fact]
        public void ExceptWith_IEnumerable_Should_Result_First_If_First_Not_Empty_Second_Empty()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = Enumerable.Empty<int>();

            first.ExceptWith(second);

            first.Count.Should().Be(3);
            first.Contains(1);
            first.Contains(2);
            first.Contains(3);
        }

        [Fact]
        public void ExceptWith_Redis_Set_Should_Result_Set_Difference_If_First_Not_Empty_Second_Not_Empty_With_Intersection()
        {
            var first = GetSet<int>(1, 2, 3, 4, 5);
            var second = GetSet<int>(1, 1, 2, 3);

            first.Count.Should().Be(5);
            second.Count.Should().Be(3);

            first.ExceptWith(second);

            first.Count.Should().Be(2);
            first.Contains(4);
            first.Contains(5);
        }

        [Fact]
        public void ExceptWith_IEnumerable_Should_Result_Set_Difference_If_First_Not_Empty_Second_Not_Empty_With_Intersection()
        {
            var first = GetSet<int>(1, 2, 3, 4, 5);
            var second = new[] { 1, 2, 3 };

            first.Count.Should().Be(5);
            second.Count().Should().Be(3);

            first.ExceptWith(second);

            first.Count.Should().Be(2);
            first.Contains(4);
            first.Contains(5);
        }

        [Fact]
        public void ExceptWith_Redis_Set_Should_Result_First_If_First_Not_Empty_Second_Not_Empty_No_Intersection()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = GetSet<int>(4, 5, 6);

            first.ExceptWith(second);

            first.Count.Should().Be(3);
            first.Contains(1);
            first.Contains(2);
            first.Contains(3);
        }

        [Fact]
        public void ExceptWith_IEnumerable_Should_Result_First_If_First_Not_Empty_Second_Not_Empty_No_Intersection()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = new[] { 4, 5, 6 };

            first.ExceptWith(second);

            first.Count.Should().Be(3);
            first.Contains(1);
            first.Contains(2);
            first.Contains(3);
        }

        [Fact]
        public void ExceptWith_Redis_Set_Should_Result_Empty_Set_If_First_Same_As_Second()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = GetSet<int>(3, 3, 2, 1, 1);

            first.ExceptWith(second);

            first.Count.Should().Be(0);
        }

        [Fact]
        public void ExceptWith_IEnumerable_Should_Result_Empty_Set_If_First_Same_As_Second()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = new[] { 3, 2, 1 };

            first.ExceptWith(second);

            first.Count.Should().Be(0);
        }

        [Fact]
        public void ExceptWith_Should_Throw_If_Other_Set_Is_Null()
        {
            var first = GetSet<int>();

            first
                .Invoking(s => s.ExceptWith(null))
                .Should()
                .Throw<ArgumentNullException>();
        }

        [Fact]
        public void IntersectWith_Redis_Set_Should_Result_Empty_Set_If_First_Empty_Second_Empty()
        {
            var first = GetSet<int>();
            var second = GetSet<int>();

            first.IntersectWith(second);

            first.Count.Should().Be(0);
        }

        [Fact]
        public void IntersectWith_IEnumerable_Should_Result_Empty_Set_If_First_Empty_Second_Empty()
        {
            var first = GetSet<int>();
            var second = Enumerable.Empty<int>();

            first.IntersectWith(second);

            first.Count.Should().Be(0);
        }

        [Fact]
        public void IntersectWith_Redis_Set_Should_Result_Empty_Set_If_First_Empty_Second_Not_Empty()
        {
            var first = GetSet<int>();
            var second = GetSet<int>(1, 2, 3);

            first.IntersectWith(second);

            first.Count.Should().Be(0);
        }

        [Fact]
        public void IntersectWith_IEnumerable_Should_Result_Empty_Set_If_First_Empty_Second_Not_Empty()
        {
            var first = GetSet<int>();
            var second = new[] { 1, 2, 3 };

            first.IntersectWith(second);

            first.Count.Should().Be(0);
        }

        [Fact]
        public void IntersectWith_Redis_Set_Should_Result_Empty_If_First_Not_Empty_Second_Empty()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = GetSet<int>();

            first.IntersectWith(second);

            first.Count.Should().Be(0);
        }

        [Fact]
        public void IntersectWith_IEnumerable_Should_Result_Empty_If_First_Not_Empty_Second_Empty()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = new int[] { };

            first.IntersectWith(second);

            first.Count.Should().Be(0);
        }

        [Fact]
        public void IntersectWith_Redis_Set_Should_Result_Set_Intersection_If_First_Not_Empty_Second_Not_Empty_With_Intersection()
        {
            var first = GetSet<int>(1, 2, 3, 4, 5);
            var second = GetSet<int>(4, 5);

            first.Count.Should().Be(5);
            second.Count().Should().Be(2);

            first.IntersectWith(second);

            first.Count.Should().Be(2);
        }

        [Fact]
        public void IntersectWith_IEnumerable_Should_Result_Set_Intersection_If_First_Not_Empty_Second_Not_Empty_With_Intersection()
        {
            var first = GetSet<int>(1, 2, 3, 4, 5);
            var second = new[]  { 4, 5 };

            first.Count.Should().Be(5);
            second.Count().Should().Be(2);

            first.IntersectWith(second);

            first.Count.Should().Be(2);
        }

        [Fact]
        public void IntersectWith_Redis_Set_Should_Result_Empty_If_First_Not_Empty_Second_Not_Empty_No_Intersection()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = GetSet<int>(4, 5, 6);

            first.IntersectWith(second);

            first.Count.Should().Be(0);
        }

        [Fact]
        public void IntersectWith_IEnumerable_Should_Result_Empty_If_First_Not_Empty_Second_Not_Empty_No_Intersection()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = new[] { 4, 5, 6 };

            first.IntersectWith(second);

            first.Count.Should().Be(0);
        }

        [Fact]
        public void IntersectWith_Redis_Set_Should_Result_First_Set_If_First_Same_As_Second()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = GetSet<int>(3, 3, 2, 1, 1);

            first.IntersectWith(second);

            first.Count.Should().Be(3);
            first.Contains(1).Should().BeTrue();
            first.Contains(2).Should().BeTrue();
            first.Contains(3).Should().BeTrue();
        }

        [Fact]
        public void IntersectWith_IEnumerable_Should_Result_First_Set_If_First_Same_As_Second()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = new[] { 3, 2, 1 };

            first.IntersectWith(second);

            first.Count.Should().Be(3);
            first.Contains(1).Should().BeTrue();
            first.Contains(2).Should().BeTrue();
            first.Contains(3).Should().BeTrue();
        }

        [Fact]
        public void IntersectWith_Should_Throw_If_Other_Set_Is_Null()
        {
            var first = GetSet<int>();

            first
                .Invoking(s => s.IntersectWith(null))
                .Should()
                .Throw<ArgumentNullException>();
        }

        [Fact]
        public void UnionWith_Redis_Set_Should_Result_Empty_Set_If_First_Empty_Second_Empty()
        {
            var first = GetSet<int>();
            var second = GetSet<int>();

            first.UnionWith(second);

            first.Count.Should().Be(0);
        }

        [Fact]
        public void UnionWith_IEnumerable_Should_Result_Empty_Set_If_First_Empty_Second_Empty()
        {
            var first = GetSet<int>();
            var second = Enumerable.Empty<int>();

            first.UnionWith(second);

            first.Count.Should().Be(0);
        }

        [Fact]
        public void UnionWith_Redis_Set_Should_Result_Second_Set_If_First_Empty_Second_Not_Empty()
        {
            var first = GetSet<int>();
            var second = GetSet<int>(1, 2, 3);

            first.UnionWith(second);

            first.Count.Should().Be(3);
            first.Contains(1).Should().BeTrue();
            first.Contains(2).Should().BeTrue();
            first.Contains(3).Should().BeTrue();
        }

        [Fact]
        public void UnionWith_IEnumerable_Should_Result_Second_Set_If_First_Empty_Second_Not_Empty()
        {
            var first = GetSet<int>();
            var second = new[] { 1, 2, 3 };

            first.UnionWith(second);

            first.Count.Should().Be(3);
            first.Contains(1).Should().BeTrue();
            first.Contains(2).Should().BeTrue();
            first.Contains(3).Should().BeTrue();
        }

        [Fact]
        public void UnionWith_Redis_Set_Should_Result_First_If_First_Not_Empty_Second_Empty()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = GetSet<int>();

            first.UnionWith(second);

            first.Count.Should().Be(3);
            first.Contains(1).Should().BeTrue();
            first.Contains(2).Should().BeTrue();
            first.Contains(3).Should().BeTrue();
        }

        [Fact]
        public void UnionWith_IEnumerable_Should_Result_First_If_First_Not_Empty_Second_Empty()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = Enumerable.Empty<int>();

            first.UnionWith(second);

            first.Count.Should().Be(3);
            first.Contains(1).Should().BeTrue();
            first.Contains(2).Should().BeTrue();
            first.Contains(3).Should().BeTrue();
        }

        [Fact]
        public void UnionWith_Redis_Set_Should_Result_Set_Union_If_First_Not_Empty_Second_Not_Empty_With_Intersection()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = GetSet<int>(2, 3, 4, 5);

            first.UnionWith(second);

            first.Count.Should().Be(5);
            first.Contains(1).Should().BeTrue();
            first.Contains(2).Should().BeTrue();
            first.Contains(3).Should().BeTrue();
            first.Contains(4).Should().BeTrue();
            first.Contains(5).Should().BeTrue();
        }

        [Fact]
        public void UnionWith_IEnumerable_Should_Result_Set_Union_If_First_Not_Empty_Second_Not_Empty_With_Intersection()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = new[] { 2, 3, 4, 5 };

            first.UnionWith(second);

            first.Count.Should().Be(5);
            first.Contains(1).Should().BeTrue();
            first.Contains(2).Should().BeTrue();
            first.Contains(3).Should().BeTrue();
            first.Contains(4).Should().BeTrue();
            first.Contains(5).Should().BeTrue();
        }

        [Fact]
        public void UnionWith_Redis_Set_Should_Result_Set_Union_If_First_Not_Empty_Second_Not_Empty_No_Intersection()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = GetSet<int>(4, 5, 6);

            first.UnionWith(second);

            first.Count.Should().Be(6);
            first.Contains(1).Should().BeTrue();
            first.Contains(2).Should().BeTrue();
            first.Contains(3).Should().BeTrue();
            first.Contains(4).Should().BeTrue();
            first.Contains(5).Should().BeTrue();
            first.Contains(6).Should().BeTrue();
        }

        [Fact]
        public void UnionWith_IEnumerable_Should_Result_Set_Union_If_First_Not_Empty_Second_Not_Empty_No_Intersection()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = new[] { 4, 5, 6 };

            first.UnionWith(second);

            first.Count.Should().Be(6);
            first.Contains(1).Should().BeTrue();
            first.Contains(2).Should().BeTrue();
            first.Contains(3).Should().BeTrue();
            first.Contains(4).Should().BeTrue();
            first.Contains(5).Should().BeTrue();
            first.Contains(6).Should().BeTrue();
        }

        [Fact]
        public void UnionWith_Redis_Set_Should_Result_First_Set_If_First_Same_As_Second()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = GetSet<int>(3, 3, 2, 1, 1);

            first.IntersectWith(second);

            first.Count.Should().Be(3);
            first.Contains(1).Should().BeTrue();
            first.Contains(2).Should().BeTrue();
            first.Contains(3).Should().BeTrue();
        }

        [Fact]
        public void UnionWith_IEnumerable_Should_Result_First_Set_If_First_Same_As_Second()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = new[] { 3, 3, 2, 1, 1 };

            first.IntersectWith(second);

            first.Count.Should().Be(3);
            first.Contains(1).Should().BeTrue();
            first.Contains(2).Should().BeTrue();
            first.Contains(3).Should().BeTrue();
        }

        [Fact]
        public void UnionWith_Should_Throw_If_Other_Set_Is_Null()
        {
            var first = GetSet<int>();

            first
                .Invoking(s => s.IntersectWith(null))
                .Should()
                .Throw<ArgumentNullException>();
        }

        [Fact]
        public void SymmetricExceptWith_Redis_Set_Should_Result_First_And_Second_Minus_Intersection()
        {
            var first = GetSet<int>(1, 2, 3, 4, 5);
            var second = GetSet<int>(4, 5, 6, 7);

            first.SymmetricExceptWith(second);

            first.Count.Should().Be(5);
            first.Contains(1).Should().BeTrue();
            first.Contains(2).Should().BeTrue();
            first.Contains(3).Should().BeTrue();
            first.Contains(6).Should().BeTrue();
            first.Contains(7).Should().BeTrue();
        }

        [Fact]
        public void SymmetricExceptWith_IEnumerable_Should_Result_First_And_Second_Minus_Intersection()
        {
            var first = GetSet<int>(1, 2, 3, 4, 5);
            var second = new[] { 4, 5, 6, 7 };

            first.SymmetricExceptWith(second);

            first.Count.Should().Be(5);
            first.Contains(1).Should().BeTrue();
            first.Contains(2).Should().BeTrue();
            first.Contains(3).Should().BeTrue();
            first.Contains(6).Should().BeTrue();
            first.Contains(7).Should().BeTrue();
        }

        [Fact]
        public void SymmetricExceptWith_Redis_Set_Should_Result_First_And_Second_When_No_Intersection()
        {
            var first = GetSet<int>(1, 2);
            var second = GetSet<int>(3, 4);

            first.SymmetricExceptWith(second);

            first.Count.Should().Be(4);
            first.Contains(1).Should().BeTrue();
            first.Contains(2).Should().BeTrue();
            first.Contains(3).Should().BeTrue();
            first.Contains(4).Should().BeTrue();
        }

        [Fact]
        public void SymmetricExceptWith_IEnumerable_Should_Result_First_And_Second_When_No_Intersection()
        {
            var first = GetSet<int>(1, 2);
            var second = new[] { 3, 4 };

            first.SymmetricExceptWith(second);

            first.Count.Should().Be(4);
            first.Contains(1).Should().BeTrue();
            first.Contains(2).Should().BeTrue();
            first.Contains(3).Should().BeTrue();
            first.Contains(4).Should().BeTrue();
        }

        [Fact]
        public void SymmetricExceptWith_Redis_Set_Should_Result_Second_If_First_Empty_Second_Not_Empty()
        {
            var first = GetSet<int>();
            var second = GetSet<int>(1, 2);

            first.SymmetricExceptWith(second);

            first.Count.Should().Be(2);
            first.Contains(1).Should().BeTrue();
            first.Contains(2).Should().BeTrue();
        }

        [Fact]
        public void SymmetricExceptWith_IEnumerable_Should_Result_Second_If_First_Empty_Second_Not_Empty()
        {
            var first = GetSet<int>();
            var second = new[] { 1, 2 };

            first.SymmetricExceptWith(second);

            first.Count.Should().Be(2);
            first.Contains(1).Should().BeTrue();
            first.Contains(2).Should().BeTrue();
        }

        [Fact]
        public void SymmetricExceptWith_Redis_Set_Should_Result_First_If_First_Not_Empty_Second_Empty()
        {
            var first = GetSet<int>(1, 2);
            var second = GetSet<int>();

            first.SymmetricExceptWith(second);

            first.Count.Should().Be(2);
            first.Contains(1).Should().BeTrue();
            first.Contains(2).Should().BeTrue();
        }

        [Fact]
        public void SymmetricExceptWith_IEnumerable_Should_Result_First_If_First_Not_Empty_Second_Empty()
        {
            var first = GetSet<int>(1, 2);
            var second = Enumerable.Empty<int>();

            first.SymmetricExceptWith(second);

            first.Count.Should().Be(2);
            first.Contains(1).Should().BeTrue();
            first.Contains(2).Should().BeTrue();
        }

        [Fact]
        public void SymmetricExceptWith_Redis_Set_Should_Result_Empty_If_First_Empty_Second_Empty()
        {
            var first = GetSet<int>();
            var second = GetSet<int>();

            first.SymmetricExceptWith(second);

            first.Count.Should().Be(0);
        }        
        
        [Fact]
        public void SymmetricExceptWith_IEnumerable_Should_Result_Empty_If_First_Empty_Second_Empty()
        {
            var first = GetSet<int>();
            var second = Enumerable.Empty<int>();

            first.SymmetricExceptWith(second);

            first.Count.Should().Be(0);
        }        

        [Fact]
        public void SymmetricExceptWith_Should_Throw_If_Other_Is_Null()
        {
            var first = GetSet<int>();

            first
                .Invoking(s => s.SymmetricExceptWith(null))
                .Should()
                .Throw<ArgumentNullException>();
        }

        [Fact]
        public void SetEquals_Redis_Set_Should_Return_True_If_First_Equals_To_Second()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = GetSet<int>(3, 3, 1, 1, 2, 2);

            first.SetEquals(second).Should().BeTrue();
        }

        [Fact]
        public void SetEquals_IEnumerable_Should_Return_True_If_First_Equals_To_Second()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = new[] { 3, 1, 2, 2 };

            first.SetEquals(second).Should().BeTrue();
        }

        [Fact]
        public void SetEquals_Redis_Set_Should_Return_False_If_First_Not_Equals_To_Second()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = GetSet<int>(3, 2, 1, 4);

            first.SetEquals(second).Should().BeFalse();
        }

        [Fact]
        public void SetEquals_IEnumerable_Should_Return_False_If_First_Not_Equals_To_Second()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = new[] { 3, 2, 1, 4 };

            first.SetEquals(second).Should().BeFalse();
        }

        [Fact]
        public void SetEquals_Redis_Set_Should_Return_True_If_First_Empty_Second_Empty()
        {
            var first = GetSet<int>();
            var second = GetSet<int>();

            first.SetEquals(second).Should().BeTrue();
        }

        [Fact]
        public void SetEquals_IEnumerable_Should_Return_True_If_First_Empty_Second_Empty()
        {
            var first = GetSet<int>();
            var second = Enumerable.Empty<int>();

            first.SetEquals(second).Should().BeTrue();
        }

        [Fact]
        public void IsProperSubsetOf_Redis_Set_Should_Return_True_If_All_Elements_Of_First_In_Second_And_Not_Equal_To_Second()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = GetSet<int>(1, 2, 3, 4, 5);

            first.IsProperSubsetOf(second).Should().BeTrue();
        }

        [Fact]
        public void IsProperSubsetOf_IEnumerable_Should_Return_True_If_All_Elements_Of_First_In_Second_And_Not_Equal_To_Second()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = new[] { 1, 2, 3, 4, 5 };

            first.IsProperSubsetOf(second).Should().BeTrue();
        }

        [Fact]
        public void IsProperSubsetOf_Redis_Set_Should_Return_False_If_Frist_Equals_To_Second()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = GetSet<int>(1, 2, 3);

            first.IsProperSubsetOf(second).Should().BeFalse();
        }

        [Fact]
        public void IsProperSubsetOf_IEnumerable_Should_Return_False_If_Frist_Equals_To_Second()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = new[] { 1, 2, 3 };

            first.IsProperSubsetOf(second).Should().BeFalse();
        }

        [Fact]
        public void IsProperSubsetOf_Redis_Set_Should_Return_False_If_At_Least_One_Element_In_First_Not_In_Second()
        {
            var first = GetSet<int>(1, 2, 3, 4);
            var second = GetSet<int>(1, 2, 3, 5, 6);

            first.IsProperSubsetOf(second).Should().BeFalse();
        }

        [Fact]
        public void IsProperSubsetOf_IEnumerable_Should_Return_False_If_At_Least_One_Element_In_First_Not_In_Second()
        {
            var first = GetSet<int>(1, 2, 3, 4);
            var second = new[] { 1, 2, 3, 5, 6 };

            first.IsProperSubsetOf(second).Should().BeFalse();
        }

        [Fact]
        public void IsProperSubsetOf_Redis_Set_Should_Return_True_If_First_Is_Empty()
        {
            var first = GetSet<int>();
            var second = GetSet<int>(1, 2, 3);

            first.IsProperSubsetOf(second).Should().BeTrue();
        }

        [Fact]
        public void IsProperSubsetOf_IEnumerable_Should_Return_True_If_First_Is_Empty()
        {
            var first = GetSet<int>();
            var second = new[] { 1, 2, 3 };

            first.IsProperSubsetOf(second).Should().BeTrue();
        }

        [Fact]
        public void IsProperSubsetOf_Redis_Set_Should_Return_False_If_First_Empty_Second_Empty()
        {
            var first = GetSet<int>();
            var second = GetSet<int>();

            first.IsProperSubsetOf(second).Should().BeFalse();
        }

        [Fact]
        public void IsProperSubsetOf_IEnumerable_Should_Return_False_If_First_Empty_Second_Empty()
        {
            var first = GetSet<int>();
            var second = Enumerable.Empty<int>();

            first.IsProperSubsetOf(second).Should().BeFalse();
        }

        [Fact]
        public void IsProperSubsetOf_Should_Throw_If_Second_Is_Null()
        {
            var first = GetSet<int>();

            first
                .Invoking(s => s.IsProperSubsetOf(null))
                .Should()
                .Throw<ArgumentNullException>();
        }

        [Fact]
        public void IsSubsetOf_Redis_Set_Should_Return_True_If_All_Elements_Of_First_In_Second()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = GetSet<int>(1, 2, 3, 4, 5);

            first.IsSubsetOf(second).Should().BeTrue();
        }

        [Fact]
        public void IsSubsetOf_IEnumerable_Should_Return_True_If_All_Elements_Of_First_In_Second()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = new[] { 1, 2, 3, 4, 5 };

            first.IsSubsetOf(second).Should().BeTrue();
        }

        [Fact]
        public void IsSubsetOf_Redis_Set_Should_Return_True_If_Frist_Equals_To_Second()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = GetSet<int>(1, 2, 3);

            first.IsSubsetOf(second).Should().BeTrue();
        }

        [Fact]
        public void IsSubsetOf_IEnumerable_Should_Return_True_If_Frist_Equals_To_Second()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = new[] { 1, 2, 3 };

            first.IsSubsetOf(second).Should().BeTrue();
        }

        [Fact]
        public void IsSubsetOf_Redis_Set_Should_Return_False_If_At_Least_One_Element_In_First_Not_In_Second()
        {
            var first = GetSet<int>(1, 2, 3, 4);
            var second = GetSet<int>(1, 2, 3, 5, 6);

            first.IsSubsetOf(second).Should().BeFalse();
        }

        [Fact]
        public void IsSubsetOf_IEnumerable_Should_Return_False_If_At_Least_One_Element_In_First_Not_In_Second()
        {
            var first = GetSet<int>(1, 2, 3, 4);
            var second = new[] { 1, 2, 3, 5, 6 };

            first.IsSubsetOf(second).Should().BeFalse();
        }

        [Fact]
        public void IsSubsetOf_Redis_Set_Should_Return_True_If_First_Is_Empty()
        {
            var first = GetSet<int>();
            var second = GetSet<int>(1, 2, 3);

            first.IsSubsetOf(second).Should().BeTrue();
        }

        [Fact]
        public void IsSubsetOf_IEnumerable_Should_Return_True_If_First_Is_Empty()
        {
            var first = GetSet<int>();
            var second = new[] { 1, 2, 3 };

            first.IsSubsetOf(second).Should().BeTrue();
        }

        [Fact]
        public void IsSubsetOf_Redis_Set_Should_Return_True_If_First_Empty_Second_Empty()
        {
            var first = GetSet<int>();
            var second = GetSet<int>();

            first.IsSubsetOf(second).Should().BeTrue();
        }

        [Fact]
        public void IsSubsetOf_IEnumerable_Should_Return_True_If_First_Empty_Second_Empty()
        {
            var first = GetSet<int>();
            var second = Enumerable.Empty<int>();

            first.IsSubsetOf(second).Should().BeTrue();
        }

        [Fact]
        public void IsSubsetOf_Should_Throw_If_Second_Is_Null()
        {
            var first = GetSet<int>();

            first
                .Invoking(s => s.IsSubsetOf(null))
                .Should()
                .Throw<ArgumentNullException>();
        }

        [Fact]
        public void IsProperSupersetOf_Redis_Set_Should_Return_True_If_All_Elements_Of_Second_In_First_And_Not_Equal_To_First()
        {
            var first = GetSet<int>(1, 2, 3, 4, 5);
            var second = GetSet<int>(1, 2, 3);

            first.IsProperSupersetOf(second).Should().BeTrue();
        }

        [Fact]
        public void IsProperSupersetOf_IEnumerable_Should_Return_True_If_All_Elements_Of_Second_In_First_And_Not_Equal_To_First()
        {
            var first = GetSet<int>(1, 2, 3, 4, 5);
            var second = new[] { 1, 2, 3 };

            first.IsProperSupersetOf(second).Should().BeTrue();
        }

        [Fact]
        public void IsProperSupersetOf_Redis_Set_Should_Return_False_If_Frist_Equals_To_Second()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = GetSet<int>(1, 2, 3);

            first.IsProperSupersetOf(second).Should().BeFalse();
        }

        [Fact]
        public void IsProperSupersetOf_IEnumerable_Should_Return_False_If_Frist_Equals_To_Second()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = new[] { 1, 2, 3 };

            first.IsProperSupersetOf(second).Should().BeFalse();
        }

        [Fact]
        public void IsProperSupersetOf_Redis_Set_Should_Return_False_If_At_Least_One_Element_In_Second_Not_In_First()
        {
            var first = GetSet<int>(1, 2, 3, 5, 6);
            var second = GetSet<int>(1, 2, 3, 4);

            first.IsProperSupersetOf(second).Should().BeFalse();
        }

        [Fact]
        public void IsProperSupersetOf_IEnumerable_Should_Return_False_If_At_Least_One_Element_In_Second_Not_In_First()
        {
            var first = GetSet<int>(1, 2, 3, 5, 6);
            var second = new[] { 1, 2, 3, 4 };

            first.IsProperSupersetOf(second).Should().BeFalse();
        }

        [Fact]
        public void IsProperSupersetOf_Redis_Set_Should_Return_True_If_Second_Is_Empty()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = GetSet<int>();

            first.IsProperSupersetOf(second).Should().BeTrue();
        }

        [Fact]
        public void IsProperSupersetOf_IEnumerable_Should_Return_True_If_Second_Is_Empty()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = new int[] { };

            first.IsProperSupersetOf(second).Should().BeTrue();
        }

        [Fact]
        public void IsProperSupersetOf_Redis_Set_Should_Return_False_If_First_Empty_Second_Empty()
        {
            var first = GetSet<int>();
            var second = GetSet<int>();

            first.IsProperSupersetOf(second).Should().BeFalse();
        }

        [Fact]
        public void IsProperSupersetOf_IEnumerable_Should_Return_False_If_First_Empty_Second_Empty()
        {
            var first = GetSet<int>();
            var second = Enumerable.Empty<int>();

            first.IsProperSupersetOf(second).Should().BeFalse();
        }

        [Fact]
        public void IsProperSupersetOf_Should_Throw_If_Second_Is_Null()
        {
            var first = GetSet<int>();

            first
                .Invoking(s => s.IsProperSupersetOf(null))
                .Should()
                .Throw<ArgumentNullException>();
        }

        [Fact]
        public void IsSupersetOf_Redis_Set_Should_Return_True_If_All_Elements_Of_Second_In_First()
        {
            var first = GetSet<int>(1, 2, 3, 4, 5);
            var second = GetSet<int>(1, 2, 3);

            first.IsSupersetOf(second).Should().BeTrue();
        }

        [Fact]
        public void IsSupersetOf_IEnumerable_Should_Return_True_If_All_Elements_Of_Second_In_First()
        {
            var first = GetSet<int>(1, 2, 3, 4, 5);
            var second = new[] { 1, 2, 3 };

            first.IsSupersetOf(second).Should().BeTrue();
        }

        [Fact]
        public void IsSupersetOf_Redis_Set_Should_Return_True_If_Frist_Equals_To_Second()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = GetSet<int>(1, 2, 3);

            first.IsSupersetOf(second).Should().BeTrue();
        }

        [Fact]
        public void IsSupersetOf_IEnumerable_Should_Return_True_If_Frist_Equals_To_Second()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = new[] { 1, 2, 3 };

            first.IsSupersetOf(second).Should().BeTrue();
        }

        [Fact]
        public void IsSupersetOf_Redis_Set_Should_Return_False_If_At_Least_One_Element_In_Second_Not_In_First()
        {
            var first = GetSet<int>(1, 2, 3, 5, 6);
            var second = GetSet<int>(1, 2, 3, 4);

            first.IsSupersetOf(second).Should().BeFalse();
        }

        [Fact]
        public void IsSupersetOf_IEnumerable_Should_Return_False_If_At_Least_One_Element_In_Second_Not_In_First()
        {
            var first = GetSet<int>(1, 2, 3, 5, 6);
            var second = new[] { 1, 2, 3, 4 };

            first.IsSupersetOf(second).Should().BeFalse();
        }

        [Fact]
        public void IsSupersetOf_Redis_Set_Should_Return_True_If_Second_Is_Empty()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = GetSet<int>();

            first.IsSupersetOf(second).Should().BeTrue();
        }

        [Fact]
        public void IsSupersetOf_IEnumerable_Should_Return_True_If_Second_Is_Empty()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = new int[] { };

            first.IsSupersetOf(second).Should().BeTrue();
        }

        [Fact]
        public void IsSupersetOf_Redis_Set_Should_Return_True_If_First_Empty_Second_Empty()
        {
            var first = GetSet<int>();
            var second = GetSet<int>();

            first.IsSupersetOf(second).Should().BeTrue();
        }

        [Fact]
        public void IsSupersetOf_IEnumerable_Should_Return_True_If_First_Empty_Second_Empty()
        {
            var first = GetSet<int>();
            var second = Enumerable.Empty<int>();

            first.IsSupersetOf(second).Should().BeTrue();
        }

        [Fact]
        public void IsSupersetOf_Should_Throw_If_Second_Is_Null()
        {
            var first = GetSet<int>();

            first
                .Invoking(s => s.IsSupersetOf(null))
                .Should()
                .Throw<ArgumentNullException>();
        }

        [Fact]
        public void Overlaps_Redis_Set_Should_Return_False_If_First_Empty_Second_Empty()
        {
            var first = GetSet<int>();
            var second = GetSet<int>();

            first.Overlaps(second).Should().BeFalse();
        }

        [Fact]
        public void Overlaps_IEnumerable_Should_Return_False_If_First_Empty_Second_Empty()
        {
            var first = GetSet<int>();
            var second = new int[] { };

            first.Overlaps(second).Should().BeFalse();
        }

        [Fact]
        public void Overlaps_Redis_Set_Should_Return_False_If_First_Empty_Second_Not_Empty()
        {
            var first = GetSet<int>();
            var second = GetSet<int>(1, 2, 3);

            first.Overlaps(second).Should().BeFalse();
        }

        [Fact]
        public void Overlaps_IEnumerable_Should_Return_False_If_First_Empty_Second_Not_Empty()
        {
            var first = GetSet<int>();
            var second = new[] { 1, 2, 3 };

            first.Overlaps(second).Should().BeFalse();
        }

        [Fact]
        public void Overlaps_Redis_Set_Should_Return_False_If_First_Not_Empty_Second_Empty()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = GetSet<int>();

            first.Overlaps(second).Should().BeFalse();
        }

        [Fact]
        public void Overlaps_IEnumerable_Should_Return_False_If_First_Not_Empty_Second_Empty()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = new int[] { };

            first.Overlaps(second).Should().BeFalse();
        }

        [Fact]
        public void Overlaps_Redis_Set_Should_Return_True_If_First_Not_Empty_Second_Not_Empty_With_Intersection()
        {
            var first = GetSet<int>(1, 2, 3, 4, 5);
            var second = GetSet<int> (4, 5);

            first.Overlaps(second).Should().BeTrue();
        }

        [Fact]
        public void Overlaps_IEnumerable_Should_Return_True_If_First_Not_Empty_Second_Not_Empty_With_Intersection()
        {
            var first = GetSet<int>(1, 2, 3, 4, 5);
            var second = new[]  { 4, 5 };

            first.Overlaps(second).Should().BeTrue();
        }

        [Fact]
        public void Overlaps_Redis_Set_Should_Return_False_If_First_Not_Empty_Second_Not_Empty_No_Intersection()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = GetSet<int>(4, 5, 6);

            first.Overlaps(second).Should().BeFalse();
        }

        [Fact]
        public void Overlaps_IEnumerable_Should_Return_False_If_First_Not_Empty_Second_Not_Empty_No_Intersection()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = new[] { 4, 5, 6 };

            first.Overlaps(second).Should().BeFalse();
        }

        [Fact]
        public void Overlaps_Redis_Set_Should_Return_True_If_First_Same_As_Second()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = GetSet<int>(3, 3, 2, 1, 1);

            first.Overlaps(second).Should().BeTrue();
        }

        [Fact]
        public void Overlaps_IEnumerable_Should_Return_True_If_First_Same_As_Second()
        {
            var first = GetSet<int>(1, 2, 3);
            var second = new[] { 3, 2, 1 };

            first.Overlaps(second).Should().BeTrue();
        }

        [Fact]
        public void Overlaps_Should_Throw_If_Other_Set_Is_Null()
        {
            var first = GetSet<int>();

            first
                .Invoking(s => s.Overlaps(null))
                .Should()
                .Throw<ArgumentNullException>();
        }

        [Fact]
        public void Ctor_Should_Throw_If_Database_Is_Null()
        {
            Action action = () => new RedisSet<int>(null, "set");
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage("Value cannot be Null.\nParameter name: database");
        }

        [Fact]
        public void Ctor_Should_Throw_If_Name_Is_Null()
        {
            Action action = () => new RedisSet<int>(connectionMultiplexer.GetDatabase(), null);
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage("Value cannot be Null.\nParameter name: name");
        }

        private RedisSet<T> GetSet<T>(params T[] items)
        {
            var set = new RedisSet<T>(connectionMultiplexer.GetDatabase(), Guid.NewGuid().ToString());
            set.Clear();
            set.Add(items);
            return set;
        }
    }
}