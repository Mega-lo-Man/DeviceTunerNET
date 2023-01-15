using DeviceTunerNET.Core;
using DeviceTunerNET.Services.Interfaces;
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
    public abstract class TelnetAbstract : ISender
    {
        private readonly EventAggregator _ea;
        private readonly TelnetConnection _tc;
        protected Dictionary<string, string> _sDict;
        protected EthernetSwitch _ethernetDevice;

        protected TelnetAbstract(EventAggregator ea, TelnetConnection tc)
        {
            _ea = ea;
            _tc = tc;
        }

        public bool CloseConnection()
        {
            try
            {
                _tc.ConnectionClose();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool CreateConnection(string IPaddress, ushort Port, string Username, string Password, string KeyFile)
        {
            if (_tc.CreateConnection(IPaddress, Port))
            {
                var ev = _ea.GetEvent<MessageSentEvent>();
                var returnStrFromConsole = _tc.Login(Username, Password, 1000);
                ev.Publish(new Message
                {
                    ActionCode = MessageSentEvent.StringToConsole,
                    MessageString = returnStrFromConsole
                });
                // server output should end with "$" or ">" or "#", otherwise the connection failed
                var prompt = returnStrFromConsole.TrimEnd();
                prompt = returnStrFromConsole.Substring(prompt.Length - 1, 1);
                return prompt == "$" || prompt == ">" || prompt == "#";
            }

            return false;
        }

        public string SendMessage(string command)
        {
            var commandResult = _tc.WriteRead(command); //Комманда в коммутатор

            // Сообщаем всем, что получена строка-ответ от коммутатора которую нужно вывести в консоль
            _ea.GetEvent<MessageSentEvent>().Publish(new Message
            {
                ActionCode = MessageSentEvent.StringToConsole,
                MessageString = commandResult
            });
            return commandResult;
        }

        public virtual EthernetSwitch Send(EthernetSwitch ethernetSwitch, Dictionary<string, string> settingsDict)
        {
            _ethernetDevice = ethernetSwitch;
            _sDict = settingsDict;

            SendPacket(); // Передаём настройки по Telnet-протоколу
            _tc.ConnectionClose(); // Закрываем Telnet-соединение

            return _ethernetDevice; // Возвращаем объект с заполненными свойствами полученными из коммутатора
        }

        // Пакет команд для отправки на коммутатор
        public abstract bool SendPacket();
    }
}
