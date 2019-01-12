using System;

namespace StackExchange.Redis.DataTypes.Tests.TestObjects
{
    public class Person : IEquatable<Person>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        
        public bool Equals(Person other) => other?.Id == Id && other?.Name == Name && other?.Age == Age;

        public override bool Equals(object obj) => obj is Person person && Equals(person);
        
        public override int GetHashCode() => Id.GetHashCode() ^ (Name?.GetHashCode() ?? 0) ^ Age.GetHashCode();
    }
}