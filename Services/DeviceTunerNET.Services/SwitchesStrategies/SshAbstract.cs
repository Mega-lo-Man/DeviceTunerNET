using DeviceTunerNET.Core;
using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using Prism.Events;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services.SwitchesStrategies
{
    public abstract class SshAbstract : ISender
    {
        private SshClient _sshClient;
        private readonly EventAggregator _ea;
        protected EthernetSwitch NetworkSwitch;
        protected Dictionary<string, string> SettingsDict;
        protected ShellStream Stream;

        public SshAbstract(EventAggregator ea)
        {
            _ea = ea;
        }

        public bool CloseConnection()
        {
            try
            {
                _sshClient.Disconnect();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool CreateConnection(string ipAddress, ushort port, string username, string password, string rsaKeyFile)
        {
            try
            {
                // => Dependency injection!
                var ConnNfo = new ConnectionInfo(ipAddress, username,
                    new AuthenticationMethod[] {
                                //Password based Authentication
                                new PasswordAuthenticationMethod(username, password),
                                //Key Based Authentication (using keys in OpneSSH format)
                                new PrivateKeyAuthenticationMethod(username, new PrivateKeyFile[]
                                {
                                    new PrivateKeyFile(rsaKeyFile/*@"id_rsa.key"*/, "testrsa")
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

        public virtual EthernetSwitch Send(EthernetSwitch ethernetSwitch, Dictionary<string, string> settingsDict)
        {
            NetworkSwitch = ethernetSwitch;
            SettingsDict = settingsDict;

            Stream = _sshClient.CreateShellStream("", 0, 0, 0, 0, 0);

            SendPacket();

            Stream.Close();

            

            return NetworkSwitch;
        }

        protected abstract bool GetIdOverSsh();

        // Пакет команд для отправки на коммутатор
        protected abstract void SendPacket();

        // Получение строки ответов на комманды от коммутатора
        protected string GetDeviceResponse()
        {
            string line;
            var result = "";
            // Сократим начало выражения "_ea.GetEvent<MessageSentEvent>()" обозвав его "ev"
            var ev = _ea.GetEvent<MessageSentEvent>();

            while ((line = Stream.ReadLine(TimeSpan.FromSeconds(2))) != null)
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
