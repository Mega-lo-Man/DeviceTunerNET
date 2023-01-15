using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
        private readonly INetworkUtils _networkUtils;
        private readonly ISender _telnetSender;
        private readonly ISender _sshSender;
        private readonly ITftpServerManager _tftpServer;
        private readonly IConfigParser _configParser;
        private EthernetSwitch _ethernetSwitch;
        private int repeatNumer = 5;
        private Dictionary<string, string> _sDict;
        
        private string _tftpSharedDirectory = @"C:\Temp\";
        private string _resourcePath = "Resources\\Files\\";
        private string configTemplateFileName = @"EltexTemplateConfig.txt";
        private string configOutputFileName = @"config.txt";

        public string DefaultIpAddress { get; set; } = "192.168.1.239";
        public ushort DefaultTelnetPort { get; set; } = 23;
        public ushort DefaultSshPort { get; set; } = 22;
        public string DefaultUsername { get; set; } = "admin";
        public string DefaultPassword { get; set; } = "admin";
        public string RsaKeyFile { get; set; } = "id_rsa.key";

        
        public Eltex(INetworkUtils networkUtils,
                     IEnumerable<ISender> senders,
                     ITftpServerManager tftpServer,
                     IConfigParser configParser,
                     IEventAggregator eventAggregator)
        {
            _networkUtils = networkUtils;
            _telnetSender = senders.ElementAt(0);//telnetSender;
            _sshSender = senders.ElementAt(1);//sshSender;
            _tftpServer = tftpServer;
            _configParser = configParser;
            _ea = eventAggregator;

            // Start TFTP Server
            _tftpServer.Start(_tftpSharedDirectory);
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

            var result = _configParser.Parse(settingsDict, _resourcePath + configTemplateFileName, _tftpSharedDirectory + configOutputFileName);
            Debug.WriteLine("Parse result: " + result);
            var State = 0;
            var IsSendComplete = false;

            while (State < 7 && !token.IsCancellationRequested)
            {
                switch (State)
                {
                    case 0:
                        // Пингуем в цикле коммутатор по дефолтному адресу пока коммутатор не ответит на пинг
                        MessageForUser("Ожидание" + "\r\n" + "коммутатора");
                        if (_networkUtils.SendMultiplePing(_sDict["%%DEFAULT_IP_ADDRESS%%"], repeatNumer))
                            State = 1;
                        break;
                    case 1:
                        // Пытаемся в цикле подключиться по Telnet (сервер Telnet загружается через некоторое время после успешного пинга)
                        if (_telnetSender.CreateConnection(_sDict["%%DEFAULT_IP_ADDRESS%%"],
                                                           DefaultTelnetPort, _sDict["%%DEFAULT_ADMIN_LOGIN%%"],
                                                           _sDict["%%DEFAULT_ADMIN_PASSWORD%%"],
                                                           null))
                            State = 2;
                        break;
                    case 2:
                        // Заливаем первую часть конфига в коммутатор по Telnet
                        // copy tftp://192.168.1.254/config.txt running-config
                        MessageToConsole("Заливаем первую часть конфига в коммутатор по Telnet.");

                        _telnetSender.Send(_ethernetSwitch, _sDict);
                        // Закрываем Telnet соединение
                        _telnetSender.CloseConnection();
                        State = 3;
                        break;
                    case 3:
                        // Пытаемся в цикле подключиться к SSH-серверу
                        if (_sshSender.CreateConnection(_ethernetSwitch.AddressIP,
                                                        DefaultSshPort, _sDict["%%NEW_ADMIN_LOGIN%%"],
                                                        _sDict["%%NEW_ADMIN_PASSWORD%%"],
                                                        _resourcePath + RsaKeyFile))
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
    }
}
