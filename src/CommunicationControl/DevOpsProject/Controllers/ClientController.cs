using DevOpsProject.CommunicationControl.API.DTO.Client.Request;
using DevOpsProject.CommunicationControl.Logic.Services.Interfaces;
using DevOpsProject.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DevOpsProject.CommunicationControl.API.Controllers
{

    [ApiController]
    [Route("api/client")]
    public class ClientController : Controller
    {
        private readonly ICommunicationControlService _communicationControlService;
        private readonly IOptionsMonitor<OperationalAreaConfig> _operationalAreaConfig;

        public ClientController(ICommunicationControlService communicationControlService, IOptionsMonitor<OperationalAreaConfig> operationalAreaConfig)
        {
            _communicationControlService = communicationControlService;
            _operationalAreaConfig = operationalAreaConfig;
        }

        [HttpGet("area")]
        public async Task<IActionResult> GetOperationalArea()
        {
            return Ok(_operationalAreaConfig.CurrentValue);
        }

        [HttpGet("hive/{hiveId}")]
        public async Task<IActionResult> GetHive(string hiveId)
        {
            var hiveModel = await _communicationControlService.GetHiveModel(hiveId);

            return Ok(hiveModel);
        }

        [HttpGet("hive")]
        public async Task<IActionResult> GetHives()
        {

            var hives = await _communicationControlService.GetAllHives();

            return Ok(hives);
        }

        [HttpDelete("hive/{hiveId}")]
        public async Task<IActionResult> DisconnectHive(string hiveId)
        {
            var disconnetResult = await _communicationControlService.DisconnectHive(hiveId);
            return Ok(disconnetResult);
        }

        [HttpPatch("hive")]
        public async Task<IActionResult> SendBulkHiveMovingSignal(MoveHivesRequest request)
        {
            if (request?.Hives == null || !request.Hives.Any())
                return BadRequest("No hive IDs provided.");

            foreach (var id in request.Hives)
            {
                Task.Run(async () => await _communicationControlService.SendHiveControlSignal(id, request.Destination));
            }

            return Accepted("Hives are being moved asynchronously.");
        }
    }
}
