using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DeviceTunerNET.Core;
using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using Prism.Events;

namespace DeviceTunerNET.Services.SwitchesStrategies
{
    public class Eltex : ISwitchConfigUploader
    {
        private readonly IEventAggregator _ea;
        private readonly NetworkUtils _networkUtils;
        private readonly Telnet_Sender _telnetSender;
        private readonly SSH_Sender _sshSender;

        private EthernetSwitch _ethernetSwitch;
        private int repeatNumer = 5;
        private Dictionary<string, string> _sDict;

        public string DefaultIpAddress { get; set; } = "192.168.1.239";
        public ushort DefaultTelnetPort { get; set; } = 23;
        public ushort DefaultSshPort { get; set; } = 22;
        public string DefaultUsername { get; set; } = "admin";
        public string DefaultPassword { get; set; } = "admin";
        public string RsaKeyFile { get; set; } = "Resources\\Files\\id_rsa.key";

        public Eltex(NetworkUtils networkUtils, Telnet_Sender telnetSender, SSH_Sender sshSender, IEventAggregator eventAggregator)
        {
            _networkUtils = networkUtils;
            _telnetSender = telnetSender;
            _sshSender = sshSender;
            _ea = eventAggregator;
        }

        public string GetSwitchModel()
        {
            throw new NotImplementedException();
        }

        public EthernetSwitch SendConfig(EthernetSwitch ethernetSwitch, Dictionary<string, string> settingsDict, CancellationToken token)
        {
            _ethernetSwitch = ethernetSwitch;
            MessageToConsole("Waiting device...");
            _sDict = settingsDict;
            var State = 0;
            var IsSendComplete = false;

            while (State < 7 && !token.IsCancellationRequested)
            {
                switch (State)
                {
                    case 0:
                        // Пингуем в цикле коммутатор по дефолтному адресу пока коммутатор не ответит на пинг
                        MessageForUser("Ожидание" + "\r\n" + "коммутатора");
                        if (_networkUtils.SendMultiplePing(_sDict["DefaultIPAddress"], repeatNumer))
                            State = 1;
                        break;
                    case 1:
                        // Пытаемся в цикле подключиться по Telnet (сервер Telnet загружается через некоторое время после успешного пинга)
                        if (_telnetSender.CreateConnection(_sDict["DefaultIPAddress"],
                                                           DefaultTelnetPort, _sDict["DefaultAdminLogin"],
                                                           _sDict["DefaultAdminPassword"],
                                                           null))
                            State = 2;
                        break;
                    case 2:
                        // Заливаем первую часть конфига в коммутатор по Telnet
                        MessageToConsole("Заливаем первую часть конфига в коммутатор по Telnet.");

                        _telnetSender.Send(_ethernetSwitch, _sDict);
                        // Закрываем Telnet соединение
                        _telnetSender.CloseConnection();
                        State = 3;
                        break;
                    case 3:
                        // Пытаемся в цикле подключиться к SSH-серверу
                        if (_sshSender.CreateConnection(_ethernetSwitch.AddressIP,
                                                        DefaultSshPort, _sDict["NewAdminLogin"],
                                                        _sDict["NewAdminPassword"],
                                                        RsaKeyFile))
                            State = 4;
                        break;
                    case 4:
                        // Заливаем вторую часть конфига по SSH-протоколу
                        MessageToConsole("Заливаем вторую часть конфига по SSH-протоколу.");
                        _sshSender.Send(_ethernetSwitch, _sDict);
                        // Закрываем SSH-соединение
                        _sshSender.CloseConnection();

                        MessageToConsole("Заливка конфига в коммутатор завершена.");
                        State = 5;
                        break;
                    case 5:
                        // Пингуем в цикле коммутатор по новому IP-адресу (как только пинг пропал - коммутатор отключили)
                        MessageForUser("Замени" + "\r\n" + "коммутатор!");
                        if (!_networkUtils.SendMultiplePing(_ethernetSwitch.AddressIP, repeatNumer)) State = 6;
                        break;
                    case 6:
                        IsSendComplete = true;
                        State = 7;
                        break;
                }
                Thread.Sleep(100); // Слишком часто коммутатор лучше не долбить (может воспринять как атаку)
                                   // Go to state 0
            }
            if (IsSendComplete)
                return _ethernetSwitch;
            return null;
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

        private void MessageForUser(string message)
        {
            //Сообщаем об обновлении данных в репозитории
            _ea.GetEvent<MessageSentEvent>().Publish(new Message
            {
                ActionCode = MessageSentEvent.NeedOfUserAction,
                MessageString = message
            });
        }

        private void TelnetCommandPacket()
        {
            _telnetSender.SendMessage("conf t");
            _telnetSender.SendMessage("hostname " + _ethernetSwitch.Designation);
            _telnetSender.SendMessage("aaa authentication login default line");
            _telnetSender.SendMessage("aaa authentication enable default line");

            _telnetSender.SendMessage("line console");
            _telnetSender.SendMessage("login authentication default");
            _telnetSender.SendMessage("enable authentication default");
            _telnetSender.SendMessage("password " + _sDict["NewAdminPassword"]);
            _telnetSender.SendMessage("exit");

            _telnetSender.SendMessage("ip ssh server");
            _telnetSender.SendMessage("line ssh");
            _telnetSender.SendMessage("login authentication default");
            _telnetSender.SendMessage("enable authentication default");
            _telnetSender.SendMessage("password " + _sDict["NewAdminPassword"]);
            _telnetSender.SendMessage("exit");

            _telnetSender.SendMessage("username " + _sDict["NewAdminLogin"] + " privilege 15 " + "password " + _sDict["NewAdminPassword"]);

            _telnetSender.SendMessage("interface vlan 1");
            _telnetSender.SendMessage("ip address " + _ethernetSwitch.AddressIP + " /" + _sDict["IPmask"]);
        }
    }
}
