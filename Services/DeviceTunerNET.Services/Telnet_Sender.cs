using DeviceTunerNET.Core;
using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using MinimalisticTelnet;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace DeviceTunerNET.Services
{
    public class Telnet_Sender : ISender
    {
        private TelnetConnection _tc;
        private EventAggregator _ea;
        private Dictionary<string, string> _sDict;
        private EthernetSwitch _ethernetDevice;
        

        public Telnet_Sender(EventAggregator ea, TelnetConnection tc)
        {
            _ea = ea;
            _tc = tc;
        }

        public bool CloseConnection()
        {
            _tc.ConnectionClose();
            return true;
        }

        public bool CreateConnection(string IPaddress, ushort Port, string Username, string Password, string KeyFile)
        {
            if (_tc.CreateConnection(IPaddress, Port))
            {
                var ev = _ea.GetEvent<MessageSentEvent>();
                string returnStrFromConsole = _tc.Login(Username, Password, 1000);
                ev.Publish(new Message {
                    ActionCode = MessageSentEvent.StringToConsole,
                    MessageString = returnStrFromConsole
                });
                // server output should end with "$" or ">" or "#", otherwise the connection failed
                string prompt = returnStrFromConsole.TrimEnd();
                prompt = returnStrFromConsole.Substring(prompt.Length - 1, 1);
                if (prompt != "$" && prompt != ">" && prompt != "#")
                    return false;
                return true;
            }
            else
                return false;
            
            
        }

        public EthernetSwitch Send(EthernetSwitch ethernetDevice, Dictionary<string, string> SettingsDict)
        {
            _sDict = SettingsDict;
            _ethernetDevice = ethernetDevice;
            
            PacketSendToTelnet(); // Передаём настройки по Telnet-протоколу
            _tc.ConnectionClose(); // Закрываем Telnet-соединение
            
            return ethernetDevice; // Возвращаем объект с заполненными свойствами полученными из коммутатора
        }

        private string SendMessage(string command)
        {
            string commandResult = _tc.WriteRead(command); //Комманда в коммутатор

            // Сообщаем всем, что получена строка-ответ от коммутатора которую нужно вывести в консоль
            _ea.GetEvent<MessageSentEvent>().Publish(new Message {
                ActionCode = MessageSentEvent.StringToConsole,
                MessageString = commandResult
            });
            return commandResult;
        }


        private bool PacketSendToTelnet()
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
