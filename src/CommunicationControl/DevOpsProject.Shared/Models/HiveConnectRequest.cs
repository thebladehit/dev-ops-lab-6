using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevOpsProject.Shared.Models
{
    public class HiveConnectRequest
    {
        public string HiveIP { get; set; }
        public int HivePort { get; set; }
        public string HiveID { get; set; }
    }
}
