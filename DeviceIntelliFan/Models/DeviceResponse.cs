using Microsoft.Azure.Devices.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceIntelliFan.Models
{
    internal class DeviceResponse
    {
        public string Message { get; set; }
        public string ConnectionString { get; set; }
        public string IotHubName { get; set; }
        public Twin DeviceTwin { get; set; }
    }
}
