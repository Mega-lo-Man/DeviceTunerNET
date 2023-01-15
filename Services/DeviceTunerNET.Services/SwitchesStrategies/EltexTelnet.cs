using DeviceTunerNET.SharedDataModel;
using MinimalisticTelnet;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services.SwitchesStrategies
{
    public class EltexTelnet : TelnetAbstract
    {
        public EltexTelnet(EventAggregator ea, TelnetConnection tc) : base(ea, tc)
        {
        }

        public override bool SendPacket()
        {
            var tftpServerIp = NetUtils.GetLocalIpAddress();
            SendMessage("copy tftp://" + tftpServerIp + "/config.txt running-config");
            
            return true;
        }
    }
}
