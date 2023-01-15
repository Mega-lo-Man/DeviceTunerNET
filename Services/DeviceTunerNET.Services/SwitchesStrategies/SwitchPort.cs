using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services.SwitchesStrategies
{
    public class SwitchPort
    {
        public string Port { get; set; } = "";
        public string Type { get; set; } = "";
        public string Duplex { get; set; } = "";
        public int? Speed { get; set; }
        public string Neg { get; set; } = "";
        public string FlowControl { get; set; } = "";
        public string LinkState { get; set; } = "";
        public string Uptime { get; set; } = "";
        public string BackPressure { get; set; } = "";
        public string MdixMode { get; set; } = "";
        public string PortMode { get; set; } = "";
        public int? VLAN { get; set; }

    }
}
