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
            SendMessage("terminal datadump");
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
            SendMessage("vlan 101 name SERVERS");
            SendMessage("vlan 102 name DEVICES");
            SendMessage("vlan 103 name SWITCHES");
            SendMessage("vlan 104 name CAMERAS");

            SendMessage("exit");


            Debug.WriteLine(SendMessage("interface vlan 1"));
            Debug.WriteLine(SendMessage("ip address 192.168.1.239 /24"));

            Debug.WriteLine(SendMessage("interface vlan 103"));
            Debug.WriteLine(SendMessage("ip address " + _ethernetDevice.AddressIP + " /" + _sDict["IPmask"]));

            Debug.WriteLine(SendMessage("exit"));
            Debug.WriteLine(SendMessage("exit"));

            /*
            var interfaceStatus = SendMessage("show interface status | i ^gi|^te").Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            interfaceStatus.RemoveAt(0); // удаляем саму команду
            
            Debug.WriteLine("interfaceStatus = " + interfaceStatus.Count);
            foreach(var str in interfaceStatus)
                Debug.WriteLine(str);

            Debug.WriteLine(SendMessage(Environment.NewLine));
            */
            Debug.WriteLine(SendMessage("conf t"));
            Debug.WriteLine(SendMessage("interface gi1/0/9"));
            Debug.WriteLine(SendMessage("switchport mode trunk"));
            Debug.WriteLine(SendMessage("switchport trunk native vlan 103"));
            Debug.WriteLine(SendMessage("switchport trunk allowed vlan add 101-104"));
            //SendMessage("switchport forbidden default-vlan");
            Debug.WriteLine(SendMessage("exit"));

            SendMessage("interface gi1/0/10");
            SendMessage("switchport mode trunk");
            SendMessage("switchport trunk allowed vlan add 101-104");
            //SendMessage("switchport forbidden default-vlan");
            SendMessage("exit");

            

            return true;
        }
    }
}
