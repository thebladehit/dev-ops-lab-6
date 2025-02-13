using DevOpsProject.CommunicationControl.Logic.Services.Interfaces;
using DevOpsProject.Shared.Clients;
using DevOpsProject.Shared.Configuration;
using DevOpsProject.Shared.Exceptions;
using DevOpsProject.Shared.Messages;
using DevOpsProject.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DevOpsProject.CommunicationControl.Logic.Services
{
    public class CommunicationControlService : ICommunicationControlService
    {
        private readonly ISpatialService _spatialService;
        private readonly IRedisKeyValueService _redisService;
        private readonly RedisKeys _redisKeys;
        private readonly IPublishService _messageBus;
        private readonly HiveHttpClient _hiveHttpClient;
        private readonly ILogger<CommunicationControlService> _logger;

        public CommunicationControlService(ISpatialService spatialService, IRedisKeyValueService redisService, IOptionsSnapshot<RedisKeys> redisKeysSnapshot, 
            IPublishService messageBus, HiveHttpClient hiveHttpClient, ILogger<CommunicationControlService> logger)
        {
            _spatialService = spatialService;
            _redisService = redisService;
            _redisKeys = redisKeysSnapshot.Value;
            _messageBus = messageBus;
            _hiveHttpClient = hiveHttpClient;
            _logger = logger;
        }

        public async Task<bool> DisconnectHive(string hiveId)
        {
            bool isSuccessfullyDisconnected = false;
            try
            {
                var result = await _redisService.DeleteAsync(hiveId);
                isSuccessfullyDisconnected = result;
                return result;
            }
            finally
            {
                await _messageBus.Publish(new HiveDisconnectedMessage
                {
                    HiveID = hiveId,
                    IsSuccessfullyDisconnected = isSuccessfullyDisconnected
                });
            }
        }

        public async Task<HiveModel> GetHiveModel(string hiveId)
        {
            var result = await _redisService.GetAsync<HiveModel>(GetHiveKey(hiveId));
            return result;
        }

        public async Task<List<HiveModel>> GetAllHives()
        {
            var result = await _redisService.GetAllAsync<HiveModel>($"{_redisKeys.HiveKey}:");
            return result;
        }

        public async Task<HiveOperationalArea> ConnectHive(HiveModel model)
        {
            bool result = await _redisService.SetAsync(GetHiveKey(model.HiveID), model);
            if (result)
            {
                var operationalArea = await _spatialService.GetHiveOperationalArea(model);
                await _messageBus.Publish(new HiveConnectedMessage
                {
                    HiveID = model.HiveID,
                    Hive = model,
                    InitialOperationalArea = operationalArea,
                    IsSuccessfullyConnected = result
                });
                return operationalArea;
            }
            else
            {
                await _messageBus.Publish(new HiveConnectedMessage
                {
                    HiveID = model.HiveID,
                    Hive = model,
                    IsSuccessfullyConnected = result
                });
                throw new HiveConnectionException($"Failed to connect hive for HiveId: {model.HiveID}");
            }
        }

        public async Task<DateTime> AddTelemetry(HiveTelemetryModel model)
        {
            string hiveKey = GetHiveKey(model.HiveID);
            bool hiveExists = await _redisService.CheckIfKeyExists(hiveKey);
            if (hiveExists)
            {
                bool result = await _redisService.UpdateAsync(hiveKey, (HiveModel hive) =>
                {
                    hive.Telemetry = model;
                });

                await _messageBus.Publish(new TelemetrySentMessage
                {
                    HiveID = model.HiveID,
                    Telemetry = model,
                    IsSuccessfullySent = result
                });
                return model.Timestamp;
            }
            else
            {
                await _messageBus.Publish(new TelemetrySentMessage
                {
                    HiveID = model.HiveID,
                    Telemetry = model,
                    IsSuccessfullySent = false
                });
                throw new HiveNotFoundException($"Hive not found for id: {model.HiveID}");
            }

        }

        public async Task<string?> SendHiveControlSignal(string hiveId, Location destination)
        {
            var hive = await GetHiveModel(hiveId);
            if (hive == null)
            {
                throw new Exception($"Hive control signal error: cannot find hive with id: {hiveId}");
            }

            bool isSuccessfullySent = false;

            try
            {
                // TODO: Schema can be moved to appsettings
                var result = await _hiveHttpClient.SendHiveControlCommandAsync("http", hive.HiveIP, hive.HivePort, destination);
                isSuccessfullySent = true;
                return result;
            }
            finally
            {
                await _messageBus.Publish(new MoveHiveMessage
                {
                    IsSuccessfullySent = isSuccessfullySent,
                    Destination = destination,
                    HiveID = hiveId
                });
            }
        }

        private string GetHiveKey(string hiveId)
        {
            return $"{_redisKeys.HiveKey}:{hiveId}";
        }
    }
}
