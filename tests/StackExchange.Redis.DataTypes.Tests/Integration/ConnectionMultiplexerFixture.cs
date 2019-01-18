using System;
using Xunit;

namespace StackExchange.Redis.DataTypes.Tests.Integration 
{
    public class ConnectionMultiplexerFixture : IDisposable
    {
        public ConnectionMultiplexerFixture()
        {
            ConnectionMultiplexer = StackExchange.Redis.ConnectionMultiplexer.Connect("localhost,abortConnect=false,allowAdmin=true");
        }

        public IConnectionMultiplexer ConnectionMultiplexer { get; private set; }

        public void Dispose()
        {
            var endpoint = ConnectionMultiplexer.GetEndPoints(true)[0];
            ConnectionMultiplexer.GetServer(endpoint).FlushDatabase();
            ConnectionMultiplexer?.Dispose();
        }
    }
}