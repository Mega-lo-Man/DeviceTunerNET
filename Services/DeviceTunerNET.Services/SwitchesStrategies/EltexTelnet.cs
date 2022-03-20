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

            SendMessage("vlan database");
            SendMessage("vlan 101 name V101");
            SendMessage("vlan 102 name V102");
            SendMessage("vlan 103 name V103");
            SendMessage("vlan 104 name V104");
            SendMessage("vlan 105 name V105");
            SendMessage("vlan 106 name V106");
            SendMessage("vlan 107 name V107");
            SendMessage("vlan 108 name V108");

            SendMessage("interface gi1/0/9");
            SendMessage("switchport mode trunk");
            SendMessage("switchport trunk allowed vlan add 101-108");
            //SendMessage("switchport forbidden default-vlan");
            SendMessage("exit");

            SendMessage("interface gi1/0/10");
            SendMessage("switchport mode trunk");
            SendMessage("switchport trunk allowed vlan add 101-108");
            //SendMessage("switchport forbidden default-vlan");
            SendMessage("exit");

            SendMessage("interface vlan 1");
            SendMessage("ip address " + _ethernetDevice.AddressIP + " /" + _sDict["IPmask"]);

            return true;
        }
    }
}
