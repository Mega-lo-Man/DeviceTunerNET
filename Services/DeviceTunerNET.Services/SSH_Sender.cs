using DeviceTunerNET.Core;
using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using Prism.Events;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DeviceTunerNET.Services
{
    public class SSH_Sender : ISender
    {
        private SshClient _sshClient;
        private EventAggregator _ea;
        private Dictionary<string, string> _sDict;

        public SSH_Sender(EventAggregator ea)
        {
            _ea = ea;
        }

        public bool CloseConnection()
        {
            _sshClient.Disconnect();
            return true;
        }

        public bool CreateConnection(string IPaddress, ushort Port, string Username, string Password, string KeyFile)
        {
            try
            {
                // => Dependency injection!
                ConnectionInfo ConnNfo = new ConnectionInfo(IPaddress, Username,
                    new AuthenticationMethod[] {
                                //Password based Authentication
                                new PasswordAuthenticationMethod(Username, Password),
                                //Key Based Authentication (using keys in OpneSSH format)
                                new PrivateKeyAuthenticationMethod(Username, new PrivateKeyFile[]
                                {
                                    new PrivateKeyFile(KeyFile/*@"id_rsa.key"*/, "testrsa")
                                }),
                    });

                _sshClient = new SshClient(ConnNfo);
                _sshClient.Connect();
            }
            catch (Exception ex)
            {
                Debug.Print("Fault SSH connect." + ex.ToString());
                return false;
            }
            return true;
        }

        public EthernetSwitch Send(EthernetSwitch ethernetDevice, Dictionary<string, string> SettingsDict)
        {
            _sDict = SettingsDict;

            ShellStream stream = _sshClient.CreateShellStream("", 0, 0, 0, 0, 0);

            stream.WriteLine("sh system id");

            GetIDoverSSH(GetDeviceResponse(stream), ethernetDevice);

            stream.WriteLine("en");
            stream.WriteLine(_sDict["NewAdminPassword"]);
            stream.WriteLine("conf t");
            stream.WriteLine("no ip telnet server");
            stream.WriteLine("exit");
            stream.WriteLine("wr mem");
            stream.WriteLine("Y");

            GetDeviceResponse(stream);

            stream.Close();

            return ethernetDevice;
        }

        private void GetIDoverSSH(string strForParse, EthernetSwitch ethernetDevice)
        {
            string answer = strForParse;

            string MACaddress;
            string HardwareVersion;
            string SerialNumber;

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
                int LastWordIndex = answer.LastIndexOf(' ') + 1;
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
            ethernetDevice.MACaddress = MACaddress;
            ethernetDevice.HardwareVersion = HardwareVersion;
            ethernetDevice.Serial = SerialNumber;
            ethernetDevice.Username = _sDict["NewAdminLogin"];
            ethernetDevice.Password = _sDict["NewAdminPassword"];
        }

        // Получение строки ответов на комманды от коммутатора
        private string GetDeviceResponse(ShellStream stream)
        {
            string line;
            var result = "";
            // Сократим начало выражения "_ea.GetEvent<MessageSentEvent>()" обозвав его "ev"
            var ev = _ea.GetEvent<MessageSentEvent>();

            while ((line = stream.ReadLine(TimeSpan.FromSeconds(2))) != null)
            {
                ev.Publish(new Message
                {
                    ActionCode = MessageSentEvent.StringToConsole,
                    MessageString = line
                });//Tuple.Create(MessageSentEvent.StringToConsole, line));
                result += line;
            }
            return result;
        }
    }
}
