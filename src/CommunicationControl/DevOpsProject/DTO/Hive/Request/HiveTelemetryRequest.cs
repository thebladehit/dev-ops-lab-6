using DevOpsProject.Shared.Enums;
using DevOpsProject.Shared.Models;

namespace DevOpsProject.CommunicationControl.API.DTO.Hive.Request
{
    public class HiveTelemetryRequest
    {
        public string HiveID { get; set; }
        public Location Location { get; set; }
        public float Speed { get; set; }
        public float Height { get; set; }
        public State State { get; set; }
    }
}
