using DeviceTunerNET.Core;
using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

namespace DeviceTunerNET.Modules.ModuleSwitch.Models
{
    public class NetworkTasks : INetworkTasks
    {
        private ushort _telnetPort = 23;
        private ushort _sshPort = 22;
        private int repitNumer = 5;

        private IEventAggregator _ea;
        private ISender _telnetSender = null;
        private ISender _sshSender = null;

        private string RSAfile = "id_rsa.key";

        public NetworkTasks(IEventAggregator ea, IEnumerable<ISender> senders)
        {
            _ea = ea;
            _telnetSender = senders.ElementAt(0);
            _sshSender = senders.ElementAt(1);
        }

        private void MessageForUser(string message)
        {
            //Сообщаем об обновлении данных в репозитории
            _ea.GetEvent<MessageSentEvent>().Publish(new Message
            {
                ActionCode = MessageSentEvent.NeedOfUserAction,
                MessageString = message
            });
        }

        private void MessageToConsole(string message)
        {
            //Сообщаем об обновлении данных в репозитории
            _ea.GetEvent<MessageSentEvent>().Publish(new Message
            {
                ActionCode = MessageSentEvent.StringToConsole,
                MessageString = message
            });
        }

        public bool SendMultiplePing(string NewIPAddr, int NumberOfRepetitions)
        {
            string _newIPAddr = NewIPAddr;
            int _numberOfRepetitions = NumberOfRepetitions;

            int counterGoodPing = 0;

            for (int i = 0; i < _numberOfRepetitions; i++)
            {
                if (SendPing(_newIPAddr))
                {
                    counterGoodPing++;
                }
                Thread.Sleep(50);
            }
            if (counterGoodPing >= _numberOfRepetitions / 2) return true;
            else return false;
        }

        public bool SendPing(string NewIPAddress)
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions
            {
                // Use the default Ttl value which is 128,
                // but change the fragmentation behavior.
                DontFragment = true
            };

            // Create a buffer of 32 bytes of data to be transmitted.
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 120;
            PingReply reply;
            try
            {
                reply = pingSender.Send(NewIPAddress, timeout, buffer, options);
                if (reply.Status == IPStatus.Success) return true;
                else return false;
            }
            catch (Exception ex)
            {
                Debug.Print("Ping exception: " + ex.Message);
                return false;
            }
        }

        public bool UploadConfigStateMachine(EthernetSwitch switchDevice,
                                             Dictionary<string, string> settings,
                                             CancellationToken token)
        {
            MessageToConsole("Waiting device...");
            Dictionary<string, string> _sDict = settings;
            int State = 0;
            bool IsSendComplete = false;
            while (State < 6 && !token.IsCancellationRequested)
            {
                switch (State)
                {
                    case 0:
                        // Пингуем в цикле коммутатор по дефолтному адресу пока коммутатор не ответит на пинг
                        MessageForUser("Ожидание" + "\r\n" + "коммутатора");
                        if (SendMultiplePing(_sDict["DefaultIPAddress"], repitNumer)) State = 1;
                        break;
                    case 1:
                        // Пытаемся в цикле подключиться по Telnet (сервер Telnet загружается через некоторое время после успешного пинга)
                        if (_telnetSender.CreateConnection(_sDict["DefaultIPAddress"],
                                                           _telnetPort, _sDict["DefaultAdminLogin"],
                                                           _sDict["DefaultAdminPassword"],
                                                           null))
                            State = 2;
                        break;
                    case 2:
                        // Заливаем первую часть конфига в коммутатор по Telnet
                        MessageToConsole("Заливаем первую часть конфига в коммутатор по Telnet.");
                        _telnetSender.Send(switchDevice, _sDict);
                        // Закрываем Telnet соединение
                        _telnetSender.CloseConnection();
                        State = 3;
                        break;
                    case 3:
                        // Пытаемся в цикле подключиться к SSH-серверу
                        if (_sshSender.CreateConnection(switchDevice.AddressIP,
                                                        _sshPort, _sDict["NewAdminLogin"],
                                                        _sDict["NewAdminPassword"],
                                                        RSAfile))
                            State = 4;
                        break;
                    case 4:
                        // Заливаем вторую часть конфига по SSH-протоколу
                        MessageToConsole("Заливаем вторую часть конфига по SSH-протоколу.");
                        _sshSender.Send(switchDevice, _sDict);
                        // Закрываем SSH-соединение
                        _sshSender.CloseConnection();

                        MessageToConsole("Заливка конфига в коммутатор завершена.");
                        State = 5;
                        break;
                    case 5:
                        // Пингуем в цикле коммутатор по новому IP-адресу (как только пинг пропал - коммутатор отключили)
                        MessageForUser("Замени" + "\r\n" + "коммутатор!");
                        if (!SendMultiplePing(switchDevice.AddressIP, repitNumer)) State = 6;
                        break;
                    case 6:
                        IsSendComplete = true;
                        break;
                    default:
                        break;
                }
                Thread.Sleep(100); // Слишком часто коммутатор лучше не долбить (может воспринять как атаку)
                // Go to state 0
            }
            return IsSendComplete;
        }
    }
}
