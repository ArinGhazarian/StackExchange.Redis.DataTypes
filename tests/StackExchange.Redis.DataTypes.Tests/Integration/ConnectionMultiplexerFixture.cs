using System;
using Xunit;

namespace StackExchange.Redis.DataTypes.Tests.Integration 
{
    public class ConnectionMultiplexerFixture : IDisposable
    {
        public ConnectionMultiplexerFixture()
        {
            ConnectionMultiplexer = StackExchange.Redis.ConnectionMultiplexer.Connect("localhost,abortConnect=false");
        }

        public IConnectionMultiplexer ConnectionMultiplexer { get; private set; }

        public void Dispose() => ConnectionMultiplexer?.Dispose();
    }
}