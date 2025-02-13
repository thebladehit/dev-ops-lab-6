using DevOpsProject.CommunicationControl.Logic.Services.Interfaces;
using DevOpsProject.Shared.Configuration;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace DevOpsProject.CommunicationControl.Logic.Services
{
    public class RedisPublishService : IPublishService
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly RedisOptions _redisOptions;

        public RedisPublishService(IConnectionMultiplexer connectionMultiplexer, IOptions<RedisOptions> redisOptions)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _redisOptions = redisOptions.Value;
        }

        public async Task Publish<T>(T message)
        {
            var pubsub = _connectionMultiplexer.GetSubscriber();
            var messageJson = JsonSerializer.Serialize(message);

            await pubsub.PublishAsync(_redisOptions.PublishChannel, messageJson);
        }
    }
}
