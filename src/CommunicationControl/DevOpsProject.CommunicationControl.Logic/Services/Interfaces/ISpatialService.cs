using DevOpsProject.Shared.Models;

namespace DevOpsProject.CommunicationControl.Logic.Services.Interfaces
{
    public interface ISpatialService
    {
        Task<HiveOperationalArea> GetHiveOperationalArea(HiveModel hiveModel);
    }
}
