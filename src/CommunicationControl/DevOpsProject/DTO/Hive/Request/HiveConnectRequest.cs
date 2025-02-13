namespace DevOpsProject.CommunicationControl.API.DTO.Hive.Request
{
    public class HiveConnectRequest
    {
        public string HiveIP { get; set; }
        public int HivePort { get; set; }
        public string HiveID { get; set; }
    }
}
