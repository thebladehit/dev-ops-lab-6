using DevOpsProject.Shared.Models;

namespace DevOpsProject.CommunicationControl.API.DTO.Hive.Response
{
    public class HiveConnectResponse
    {
        public bool ConnectResult { get; set; }
        public HiveOperationalArea OperationalArea { get;set; }
    }
}
