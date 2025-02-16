using DevOpsProject.HiveMind.Logic.Services.Interfaces;
using DevOpsProject.Shared.Clients;
using DevOpsProject.Shared.Models;
using DevOpsProject.Shared.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using DevOpsProject.HiveMind.Logic.State;

namespace DevOpsProject.HiveMind.Logic.Services
{
    public class HiveMindService : IHiveMindService
    {
        private readonly HiveMindHttpClient _httpClient;
        private readonly ILogger<HiveMindService> _logger;
        private readonly HiveCommunicationConfig _communicationConfigurationOptions;
        private Timer _telemetryTimer;

        public HiveMindService(HiveMindHttpClient httpClient, ILogger<HiveMindService> logger, IOptionsSnapshot<HiveCommunicationConfig> communicationConfigurationOptions)
        {
            _httpClient = httpClient;
            _logger = logger;
            _communicationConfigurationOptions = communicationConfigurationOptions.Value;
        }

        public async Task ConnectHive()
        {
            var request = new HiveConnectRequest
            {
                HiveIP = _communicationConfigurationOptions.HiveIP,
                HivePort = _communicationConfigurationOptions.HivePort,
                HiveID = _communicationConfigurationOptions.HiveID
            };
            
            var connectResult = await _httpClient.SendCommunicationControlConnectAsync(_communicationConfigurationOptions.RequestSchema, 
                _communicationConfigurationOptions.CommunicationControlIP, _communicationConfigurationOptions.CommunicationControlPort,  
                _communicationConfigurationOptions.CommunicationControlPath, request);

            _logger.LogInformation($"Connect result for HiveID: {request.HiveID}: {connectResult}");

            if (connectResult != null)
            {
                var hiveConnectResponse = JsonSerializer.Deserialize<HiveConnectResponse>(connectResult);

                if (hiveConnectResponse != null && hiveConnectResponse.ConnectResult)
                {
                    HiveInMemoryState.OperationalArea = hiveConnectResponse.OperationalArea;
                    HiveInMemoryState.CurrentLocation = _communicationConfigurationOptions.InitialLocation;

                    // HERE - we are starting to send telemetry
                    StartTelemetry();
                }
                else
                {
                    _logger.LogInformation($"Connecting hive failed for ID: {request.HiveID}");
                    throw new Exception($"Failed to connect HiveID: {request.HiveID}");
                }
            }
            else
            {
                _logger.LogError($"Unable to connect Hive with ID: {request.HiveID}, Port: {request.HivePort}, IP: {request.HiveIP} to Communication Control. \n" +
                    $"Requested IP: {_communicationConfigurationOptions.CommunicationControlIP}, Port: {_communicationConfigurationOptions.HivePort}");
                throw new Exception($"Failed to connect hive for HiveID: {request.HiveID}");
            }
        }

        public void StopAllTelemetry()
        {
            StopTelemetry();
        }

        #region private methods
        private void StartTelemetry()
        {
            if (HiveInMemoryState.IsTelemetryRunning) return;
            // TODO: Sending telemetry each N seconds
            _telemetryTimer = new Timer(SendTelemetry, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            _logger.LogInformation("Telemetry timer started.");
        }

        private void StopTelemetry()
        {
            _telemetryTimer?.Dispose();
            HiveInMemoryState.IsTelemetryRunning = false;

            _logger.LogInformation("Telemetry timer stopped.");
        }

        private async void SendTelemetry(object state)
        {
            var currentLocation = HiveInMemoryState.CurrentLocation;

            try
            {
                var request = new HiveTelemetryRequest
                {
                    HiveID = _communicationConfigurationOptions.HiveID,
                    Location = HiveInMemoryState.CurrentLocation ?? default,
                    // TODO: MOCKED FOR NOW
                    Height = 5,
                    Speed = 15,
                    State = Shared.Enums.State.Move
                };

                var connectResult = await _httpClient.SendCommunicationControlTelemetryAsync(_communicationConfigurationOptions.RequestSchema, 
                    _communicationConfigurationOptions.CommunicationControlIP, _communicationConfigurationOptions.CommunicationControlPort, 
                    _communicationConfigurationOptions.CommunicationControlPath, request);

                _logger.LogInformation($"Telemetry sent for HiveID: {request.HiveID}: {connectResult}");

                if (connectResult != null)
                {
                    // TODO: Store timestamp
                    var hiveConnectResponse = JsonSerializer.Deserialize<HiveTelemetryResponse>(connectResult);
                }
                else
                {
                    _logger.LogError($"Unable to send Hive telemetry for HiveID: {request.HiveID}.");
                    throw new Exception($"Failed to send telemetry for HiveID: {request.HiveID}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error sending telemetry: {Message}", ex.Message);
            }
        }
        #endregion
    }
}
