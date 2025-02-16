using Asp.Versioning;
using DevOpsProject.HiveMind.Logic.Services.Interfaces;
using DevOpsProject.Shared.Configuration;
using DevOpsProject.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DevOpsProject.HiveMind.API.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}")]
    public class HiveMindController : Controller
    {
        private readonly IHiveMindService _hiveMindService;
        private readonly IHiveMindMovingService _hiveMindMovingService;

        public HiveMindController(IHiveMindService hiveMindService, IHiveMindMovingService hiveMindMovingService)
        {
            _hiveMindService = hiveMindService;
            _hiveMindMovingService = hiveMindMovingService;
        }

        [HttpGet("ping")]
        public IActionResult Ping(IOptionsSnapshot<HiveCommunicationConfig> config)
        {
            return Ok(new
            {
                Timestamp = DateTime.Now,
                ID = config.Value.HiveID
            });
        }

        [HttpPost("connect")]
        public async Task<IActionResult> TriggerConnectHive()
        {
            await _hiveMindService.ConnectHive();
            return Ok();
        }

        [HttpPost("command")]
        public IActionResult MoveHideMind(MoveHiveMindCommand command)
        {
            _hiveMindMovingService.MoveToLocation(command.Location);
            return Ok();
        }

    }
}
