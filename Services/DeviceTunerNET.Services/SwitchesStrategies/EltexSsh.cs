using DeviceTunerNET.Core;
using DeviceTunerNET.SharedDataModel;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services.SwitchesStrategies
{
    public class EltexSsh : SshAbstract
    {
        public EltexSsh(EventAggregator ea) : base(ea)
        {
        }

        protected override bool GetIdOverSsh()
        {
            var answer = GetDeviceResponse();

            string MACaddress;
            string HardwareVersion;
            string SerialNumber;
            try
            {
                if (answer.Contains("MAC address :"))
                {
                    answer = answer.Remove(0, answer.IndexOf("MAC address :"));
                    answer = answer.Replace(" ", "");
                    //"MACaddress:e0:d9:e3:3d:ca:80Hardwareversion:01.03.01Serialnumber:ES50004388"
                    MACaddress = answer.Substring(answer.IndexOf(":") + 1, 17);
                    answer = answer.Remove(0, answer.IndexOf("Hardwareversion:"));
                    //"Hardwareversion:01.03.01Serialnumber:ES50004388"
                    HardwareVersion = answer.Substring(answer.IndexOf(":") + 1, answer.IndexOf("Serialnumber:") - (answer.IndexOf(":") + 1));
                    answer = answer.Remove(0, answer.IndexOf("Serialnumber:"));
                    //"Serialnumber:ES50004388"
                    SerialNumber = answer.Remove(0, answer.IndexOf(":") + 1);
                }
                else
                {
                    answer = answer.Trim();
                    //"\rSWITCH_1_2>sh system id\rUnit    MAC address    Hardware version Serial number ---- ----------------- ---------------- -------------  1   e8:28:c1:5d:5f:60     01.02.01      ES5E004602"
                    var LastWordIndex = answer.LastIndexOf(' ') + 1;
                    SerialNumber = answer.Substring(LastWordIndex, answer.Length - LastWordIndex);
                    answer = answer.Remove(LastWordIndex);
                    answer = answer.Trim();
                    // //"\rSWITCH_1_2>sh system id\rUnit    MAC address    Hardware version Serial number ---- ----------------- ---------------- -------------  1   e8:28:c1:5d:5f:60     01.02.01"
                    LastWordIndex = answer.LastIndexOf(' ') + 1;
                    HardwareVersion = answer.Substring(LastWordIndex, answer.Length - LastWordIndex);
                    answer = answer.Remove(LastWordIndex);
                    answer = answer.Trim();
                    // //"\rSWITCH_1_2>sh system id\rUnit    MAC address    Hardware version Serial number ---- ----------------- ---------------- -------------  1   e8:28:c1:5d:5f:60"
                    LastWordIndex = answer.LastIndexOf(' ') + 1;
                    MACaddress = answer.Substring(LastWordIndex, answer.Length - LastWordIndex);
                }
            }
            catch
            {
                return false;
            }
            NetworkSwitch.MACaddress = MACaddress;
            NetworkSwitch.HardwareVersion = HardwareVersion;
            NetworkSwitch.Serial = SerialNumber;
            NetworkSwitch.Username = SettingsDict["NewAdminLogin"];
            NetworkSwitch.Password = SettingsDict["NewAdminPassword"];
            return true;
        }

        protected override void SendPacket()
        {
            Stream.WriteLine("sh system id");

            GetIdOverSsh();

            Stream.WriteLine("en");
            Stream.WriteLine(SettingsDict["NewAdminPassword"]);
            Stream.WriteLine("conf t");



            Stream.WriteLine("loopback-detection enable");
            Stream.WriteLine("spanning-tree");
            Stream.WriteLine("spanning-tree mode rstp");
            Stream.WriteLine("spanning-tree priority 16384");
            Stream.WriteLine("spanning-tree forward-time 20");
            Stream.WriteLine("spanning-tree hello-time 5");
            Stream.WriteLine("spanning-tree max-age 38");

            Stream.WriteLine("clock source sntp");
            Stream.WriteLine("no clock timezone");
            //Stream.WriteLine("clock dhcp timezone");
            Stream.WriteLine("sntp client poll timer 60");
            Stream.WriteLine("sntp unicast client enable");
            Stream.WriteLine("sntp unicast client poll");
            Stream.WriteLine("sntp server 192.168.0.1 poll");

            Stream.WriteLine("no ip telnet server");
            Stream.WriteLine("exit");
            Stream.WriteLine("wr mem");
            Stream.WriteLine("Y");
        }        
    }
}
