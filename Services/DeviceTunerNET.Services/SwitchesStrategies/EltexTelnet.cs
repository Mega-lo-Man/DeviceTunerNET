using DeviceTunerNET.SharedDataModel;
using MinimalisticTelnet;
using Prism.Events;
using System;
using System.Collections.Generic;
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
            SendMessage("conf t");
            SendMessage("hostname " + _ethernetDevice.Designation);
            SendMessage("aaa authentication login default line");
            SendMessage("aaa authentication enable default line");

            SendMessage("line console");
            SendMessage("login authentication default");
            SendMessage("enable authentication default");
            SendMessage("password " + _sDict["NewAdminPassword"]);
            SendMessage("exit");

            SendMessage("ip ssh server");
            SendMessage("line ssh");
            SendMessage("login authentication default");
            SendMessage("enable authentication default");
            SendMessage("password " + _sDict["NewAdminPassword"]);
            SendMessage("exit");

            SendMessage("username " + _sDict["NewAdminLogin"] + " privilege 15 " + "password " + _sDict["NewAdminPassword"]);

            SendMessage("interface vlan 1");
            SendMessage("ip address " + _ethernetDevice.AddressIP + " /" + _sDict["IPmask"]);

            return true;
        }
    }
}
