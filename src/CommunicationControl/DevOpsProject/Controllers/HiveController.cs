using Asp.Versioning;
using DevOpsProject.CommunicationControl.Logic.Services.Interfaces;
using DevOpsProject.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsProject.CommunicationControl.API.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/hive")]
    public class HiveController : Controller
    {
        private readonly ICommunicationControlService _communicationControlService;

        public HiveController(ICommunicationControlService communicationControlService)
        {
            _communicationControlService = communicationControlService;
        }

        [HttpPost("connect")]
        public async Task<IActionResult> Connect(HiveConnectRequest request)
        {
            var hiveModel = new HiveModel
            {
                HiveID = request.HiveID,
                HiveIP = request.HiveIP,
                HivePort = request.HivePort,
            };

            var hiveOperationalArea = await _communicationControlService.ConnectHive(hiveModel);
            var connectResponse = new HiveConnectResponse
            {
                ConnectResult = true,
                OperationalArea = hiveOperationalArea,
            };

            return Ok(connectResponse);
        }

        [HttpPost("telemetry")]
        public async Task<IActionResult> Telemetry(HiveTelemetryRequest request)
        {
            var hiveTelemetryModel = new HiveTelemetryModel
            {
                HiveID = request.HiveID,
                Location = request.Location,
                Speed = request.Speed,
                Height = request.Height,
                State = request.State,
                Timestamp = DateTime.Now
            };

            var telemetryUpdateTimestamp = await _communicationControlService.AddTelemetry(hiveTelemetryModel);
            var telemetryResponse = new HiveTelemetryResponse
            {
                Timestamp = telemetryUpdateTimestamp
            };

            return Ok(telemetryResponse);
        }

    }
}
