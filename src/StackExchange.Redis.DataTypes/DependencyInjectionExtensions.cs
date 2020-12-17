using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace StackExchange.Redis.DataTypes
{
    #region Abstractions
    public class RedisDataTypeOptions
    {
        public JsonSerializerSettings SerializerSettings { get; set; }
    }
    #endregion

    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddStackExchangeRedisDataTypes(this IServiceCollection services, Action<RedisDataTypeOptions> setupAction = null)
        {
            var options = new RedisDataTypeOptions();

            setupAction?.Invoke(options);
            Extensions.SerializerSettings = options.SerializerSettings ?? new JsonSerializerSettings();

            services.AddSingleton<IRedisTypeFactory, RedisTypeFactory>();
            return services;
        }
    }
}

