using DeviceTunerNET.Core;
using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;

namespace DeviceTunerNET.Services
{
    public class SerialSender : ISerialSender
    {
        private readonly int _packetLengthIndex = 1; //индекс байта в посылаемом пакете отвечающего за общую длину пакета
        private const int maxRepetitions = 15; // Максимальное количество повторов посылок пакетов
        private bool portReceive;
        private string receiveBuffer = null;
        private readonly SerialPort _serialPort;
        private readonly IEventAggregator _ea;

        private const int ADDRESS_CHANGE_TIMEOUT = 400; // Сингал-20П V3.10 после смены адреса подтверждает через 400 мс (остальные быстрее)
        private const int READ_MODEL_TIMEOUT = 50; // Чтение типа прибора занимает не более 50 мс

        /// <summary>
        /// Болидовская таблица CRC
        /// </summary>
        private readonly byte[] _crc8Table = {
            0x00,0x5E,0xBC,0xE2,0x61,0x3F,0xDD,0x83,0xC2,0x9C,0x7E,0x20,0xA3,0xFD,0x1F,0x41,
            0x9D,0xC3,0x21,0x7F,0xFC,0xA2,0x40,0x1E,0x5F,0x01,0xE3,0xBD,0x3E,0x60,0x82,0xDC,
            0x23,0x7D,0x9F,0xC1,0x42,0x1C,0xFE,0xA0,0xE1,0xBF,0x5D,0x03,0x80,0xDE,0x3C,0x62,
            0xBE,0xE0,0x02,0x5C,0xDF,0x81,0x63,0x3D,0x7C,0x22,0xC0,0x9E,0x1D,0x43,0xA1,0xFF,
            0x46,0x18,0xFA,0xA4,0x27,0x79,0x9B,0xC5,0x84,0xDA,0x38,0x66,0xE5,0xBB,0x59,0x07,
            0xDB,0x85,0x67,0x39,0xBA,0xE4,0x06,0x58,0x19,0x47,0xA5,0xFB,0x78,0x26,0xC4,0x9A,
            0x65,0x3B,0xD9,0x87,0x04,0x5A,0xB8,0xE6,0xA7,0xF9,0x1B,0x45,0xC6,0x98,0x7A,0x24,
            0xF8,0xA6,0x44,0x1A,0x99,0xC7,0x25,0x7B,0x3A,0x64,0x86,0xD8,0x5B,0x05,0xE7,0xB9,
            0x8C,0xD2,0x30,0x6E,0xED,0xB3,0x51,0x0F,0x4E,0x10,0xF2,0xAC,0x2F,0x71,0x93,0xCD,
            0x11,0x4F,0xAD,0xF3,0x70,0x2E,0xCC,0x92,0xD3,0x8D,0x6F,0x31,0xB2,0xEC,0x0E,0x50,
            0xAF,0xF1,0x13,0x4D,0xCE,0x90,0x72,0x2C,0x6D,0x33,0xD1,0x8F,0x0C,0x52,0xB0,0xEE,
            0x32,0x6C,0x8E,0xD0,0x53,0x0D,0xEF,0xB1,0xF0,0xAE,0x4C,0x12,0x91,0xCF,0x2D,0x73,
            0xCA,0x94,0x76,0x28,0xAB,0xF5,0x17,0x49,0x08,0x56,0xB4,0xEA,0x69,0x37,0xD5,0x8B,
            0x57,0x09,0xEB,0xB5,0x36,0x68,0x8A,0xD4,0x95,0xCB,0x29,0x77,0xF4,0xAA,0x48,0x16,
            0xE9,0xB7,0x55,0x0B,0x88,0xD6,0x34,0x6A,0x2B,0x75,0x97,0xC9,0x4A,0x14,0xF6,0xA8,
            0x74,0x2A,0xC8,0x96,0x15,0x4B,0xA9,0xF7,0xB6,0xFC,0x0A,0x54,0xD7,0x89,0x6B,0x35
        };



        private readonly Dictionary<byte, string> _bolidDict = new Dictionary<byte, string>()
        {
            { 1, "Сигнал-20" },
            { 2, "Сигнал-20П" },
            { 3, "С2000-СП1" },
            { 4, "С2000-4" },
            { 7, "С2000-К" },
            { 8, "С2000-ИТ" },
            { 9, "С2000-КДЛ" },
            { 10, "С2000-БИ/БКИ" },
            { 11, "Сигнал-20(вер. 02)" },
            { 13, "С2000-КС" },
            { 14, "С2000-АСПТ" },
            { 15, "С2000-КПБ" },
            { 16, "С2000-2" },
            { 19, "УО-ОРИОН" },
            { 20, "Рупор" },
            { 21, "Рупор-Диспетчер исп.01" },
            { 22, "С2000-ПТ" },
            { 24, "УО-4С" },
            { 25, "Поток-3Н" },
            { 26, "Сигнал-20М" },
            { 28, "С2000-БИ-01" },
            { 29, "С2000-Ethernet" },
            { 30, "Рупор-01" },
            { 31, "С2000-Adem" },
            { 33, "РИП-12 исп.50, РИП-12 исп.51, РИП-12 без исполнения" },
            { 34, "Сигнал-10" },
            { 36, "С2000-ПП" },
            { 38, "РИП-12 исп.54" },
            { 39, "РИП-24 исп.50, РИП-24 исп.51" },
            { 41, "С2000-КДЛ-2И" },
            { 43, "С2000-PGE" },
            { 44, "С2000-БКИ" },
            { 45, "Поток-БКИ" },
            { 46, "Рупор-200" },
            { 47, "С2000-Периметр" },
            { 48, "МИП-12" },
            { 49, "МИП-24" },
            { 53, "РИП-48 исп.01" },
            { 54, "РИП-12 исп.56" },
            { 55, "РИП-24 исп.56" },
            { 59, "Рупор исп.02" },
            { 61, "С2000-КДЛ-Modbus" },
            { 66, "Рупор исп.03" },
            { 67, "Рупор-300" },
        };

        private SerialSender()
        {
        }

        public SerialSender(SerialPort serialPort, IEventAggregator ea)
        {
            _serialPort = serialPort;
            _ea = ea;
        }

        public bool SetDeviceRS485Address(string comPortName, byte deviceAddress, byte newDeviceAddress)
        {
            if (_serialPort.IsOpen)
                return false;

            _serialPort.PortName = comPortName;

            _serialPort.Open();

            // формируем команду на отправку
            var cmdString = new byte[] { deviceAddress, 0xB0, 0x0F, newDeviceAddress, newDeviceAddress };

            var result = Transaction(cmdString, ADDRESS_CHANGE_TIMEOUT);

            _serialPort.Close();

            if (result.Length <= 1)
                return false;

            return result[4] == newDeviceAddress;
        }

        public string GetDeviceModel(string comPortName, byte deviceAddress)
        {
            // формируем команду на отправку
            var cmdString = new byte[] { deviceAddress, 0xBF/*0x92*/, 0x0D, 0x00, 0x00 };

            if (!_serialPort.IsOpen)
            {
                _serialPort.PortName = comPortName;
                try
                {
                    _serialPort.Open();
                }
                catch
                {
                    MessageBox.Show("Не удалось открыть порт!");
                    return "";
                }

                var deviceModel = Transaction(cmdString, READ_MODEL_TIMEOUT);

                _serialPort.Close();

                if (!(deviceModel?.Length > 1))
                    return "";

                var devType = deviceModel[3];
                return _bolidDict[devType];
            }
            else
            {
                var deviceModel = Transaction(cmdString, READ_MODEL_TIMEOUT);

                if (!(deviceModel?.Length > 1)) 
                    return "";

                var devType = deviceModel[3];

                return _bolidDict[devType];
            }
        }

        public bool IsDeviceOnline(string comPortName, byte deviceAddress)
        {
            // формируем команду на отправку
            var cmdString = new byte[] { deviceAddress, 0x92, 0x01, 0x00, 0x00 };
            if (!_serialPort.IsOpen)
            {
                _serialPort.PortName = comPortName;
                try
                {
                    _serialPort.Open();
                }
                catch
                {
                    MessageBox.Show("Не удалось открыть порт!");
                    return false;
                }

                var deviceModel = Transaction(cmdString, READ_MODEL_TIMEOUT);

                _serialPort.Close();

                if (!(deviceModel?.Length > 1))
                    return false;

                return true;
            }
            else
            {
                var deviceModel = Transaction(cmdString, READ_MODEL_TIMEOUT);

                
                return true;
            }
        }

        public Dictionary<byte, string> SearchOnlineDevices(string comPortName)
        {
            if (!_serialPort.IsOpen)
            {
                _serialPort.PortName = comPortName;
                _serialPort.Open();
            }
            var result = new Dictionary<byte, string>();
            for (byte devAddr = 1; devAddr <= 127; devAddr++)
            {
                var OnlineDevicesModel = GetDeviceModel(comPortName, devAddr);
                if (OnlineDevicesModel != string.Empty)
                {
                    result.Add(devAddr, OnlineDevicesModel);
                }
                _ea.GetEvent<MessageSentEvent>().Publish(new Message
                {
                    ActionCode = MessageSentEvent.UpdateRS485SearchProgressBar,
                    AttachedObject = devAddr
                });
            }
            _serialPort.Close();
            return result;
        }

        private byte[] Transaction(byte[] sendArray, int timeout)
        {

            // make DataReceived event handler
            _serialPort.DataReceived += sp_DataReceived;

            for (var i = 0; i < maxRepetitions; i++)
            {
                SendPacket(sendArray);
                while (portReceive) { }
                Thread.Sleep(timeout);
                if (receiveBuffer == null) 
                    continue;
                break;
            }

            if (receiveBuffer == null)
                return new byte[1] {0x00};

            var result = receiveBuffer;
            receiveBuffer = null;
            return Encoding.ASCII.GetBytes(result);

        }

        private void SendPacket(byte[] sendArray)
        {
            byte bytesCounter = 1; //сразу начнём считать с единицы, т.к. всё равно придётся добавить один байт(сам байт длины команды)
            var lst = new List<byte>();
            foreach (var bt in sendArray)
            {
                lst.Add(bt);
                bytesCounter++;
            }

            lst.Insert(1, bytesCounter); //вставляем вторым байтом в пакет длину всего пакета + байт длины пакета

            _serialPort.Write(lst.ToArray(), 0, bytesCounter);
            _serialPort.Write(CRC8(lst.ToArray()), 0, 1);
        }

        private byte[] CRC8(byte[] bytes)
        {
            byte crc = 0;
            for (byte i = 0; i < bytes.Length; ++i)
                crc = _crc8Table[crc ^ bytes[i]];

            var chr = new byte[1];
            chr[0] = crc;
            return chr;
        }

        public ObservableCollection<string> GetAvailableCOMPorts()
        {
            var ports = SerialPort.GetPortNames();
            var portsList = new ObservableCollection<string>();
            foreach (var port in ports)
            {
                portsList.Add(port);
            }
            return portsList;
        }

        public SerialPort GetSerialPortObjectRef()
        {
            return _serialPort;
        }

        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            portReceive = true;
            //Console.WriteLine("I am in event handler");
            var sPort = (SerialPort)sender;
            var data = sPort.ReadExisting();
            foreach (var ch in data)
            {
                var value = Convert.ToInt32(ch);
                //Console.Write(value.ToString("X") + " ");
            }
            receiveBuffer += data;
            Debug.WriteLine("ReceiveBuffer: " + receiveBuffer + "  Length: " + receiveBuffer.Length);
            portReceive = false;
        }

        public bool SetC2000EthernetConfig(string ComPortName, byte deviceAddress, C2000Ethernet device)
        {
            Debug.WriteLine("-----------------------");
            var result = false;
            if (_serialPort.IsOpen)
                return result;

            _serialPort.PortName = ComPortName;

            _serialPort.Open();
            // make DataReceived event handler
            _serialPort.DataReceived += sp_DataReceived;

            if (!GetDeviceModel(ComPortName, deviceAddress).Equals("С2000-Ethernet"))
                return false;

            

            result = SendPromoter();
            result = SendDeviceNetName(device.NetName);
            result = SendEthernetTune(device.AddressIP, device.Netmask, device.DefaultGateway, device.FirstDns, device.SecondDns);
            result = SendDhcpStatus(device.Dhcp);
            result = SendMasterSlaveTransparent(device.NetworkMode);
            result = SendInterfaceType(device.InterfaceType);
            result = SendConnectionSpeed(device.ConnectionSpeed);
            result = SendParityStopTimeoutSign(device.FrameFormat, device.TimeoutSign, device.PauseSign, device.Optimization);
            result = SendTimeout(device.Timeout);
            result = SendPause(device.Pause);
            result = SendAccessNotifySign(device.AccessNotifySign);
            result = SendPauseBeforeResponseRs(device.PauseBeforeResponseRs);
            
            result = SendMasterSlaveUdp(device.MasterSlaveUdp);
            result = SendConfirmationTimeout(device.ConfirmationTimeout);
            result = SendConnectionTimeout(device.ConnectionTimeout);
            result = SendFreeConnectionTune(device.FreeConnectionUdpType, device.AllowFreeConnection);
            result = SendFreeConnectionUdp(device.FreeConnectionUdp);
            result = SendTransparentTune(device.TransparentUdp, device.TransparentProtocol, device.TransparentCrypto);

            result = SendRemoteDevices(device.RemoteDevicesList);
            //result = SendSuffix();
            _serialPort.Close();
            return result;
        }

        private bool SendPromoter()
        {
            //CommandSend(new byte[] { 0x7F, 0x70, 0x0D, 0x00, 0x00 }); // 7f 06 70 0d 00 00

            CommandSend(new byte[] { 0x7F, 0x6E, 0x43, 0x00, 0x00, 0x00, 0x40 }); // 7f 08 6e 43 00 00 00 40
            CommandSend(new byte[] { 0x7F, 0x3D, 0x43, 0x00, 0x00, 0x00, 0x40 }); // 7f 08 3d 43 00 00 00 40
            return true;
        }

        private bool SendDeviceNetName(string name)
        {
            var deviceName = StringToByteArray(name);
            var header = new byte[] { 0x7F, 0x7B, 0x41, 0x00, 0x00, 0x00 };
            var resultCmd = CombineArrays(header, deviceName);

            CommandSend(resultCmd);
            return true;
        }

        private bool SendEthernetTune(string ip, string netmask, string gateway, string firstDNS, string secondDNS)
        {
            var _ip = IpToByteArray(ip);
            var _netmask = IpToByteArray(netmask);
            var _gateway = IpToByteArray(gateway);
            var _firstDNS = IpToByteArray(firstDNS);
            var _secondDNS = IpToByteArray(secondDNS);
            var _header = new byte[] { 0x7F, 0xC9, 0x41, 0x20, 0x00, 0x00 };
            var cmd = CombineArrays(_header, _ip, _netmask, _gateway, _firstDNS, _secondDNS);
            CommandSend(cmd);
            return true;
        }

        private bool SendDhcpStatus(bool dhcpEnable)
        {
            var dhcp = Convert.ToByte(dhcpEnable);
            var cmd = new byte[] { 0x7F, 0x0F, 0x41, 0x3A, 0x00, 0x00, dhcp }; //7f 08 0f 41 61 00 00 01
            CommandSend(cmd);
            return true;
        }

        private bool SendMasterSlaveTransparent(C2000Ethernet.Mode mode)
        {
            CommandSend(new byte[] { 0x7F, 0x8B, 0x41, 0x61, 0x00, 0x00, (byte)mode }); // 7f 08 8b 41 61 00 00 00
            return true;
        }

        private bool SendInterfaceType(C2000Ethernet.ProtocolType rsType)
        {
            CommandSend(new byte[] { 0x7F, 0x50, 0x41, 0x60, 0x00, 0x00, (byte)rsType }); // 7f 08 50 41 60 00 00 01
            return true;
        }

        private bool SendConnectionSpeed(C2000Ethernet.Speed speed)
        {
            CommandSend(new byte[] { 0x7F, 0xD6, 0x41, 0xC0, 0x00, 0x00, (byte)speed }); // 7f 08 d6 41 c0 00 00 03
            return true;
        }

        private bool SendParityStopTimeoutSign(
            C2000Ethernet.DataParityStop dps, 
            bool timeoutSign,
            bool pauseSign,
            bool optimization)
        {
            var timeoutSignByte = Convert.ToByte(timeoutSign);
            var timeoutSignMask = (byte)(timeoutSignByte << 4); // 0001_0000 if timeoutSign = true

            var pauseSignByte = Convert.ToByte(pauseSign);
            var pauseSignMask = (byte) (pauseSignByte << 5); // 0010_0000 if pauseSign = true

            var optimizationByte = Convert.ToByte(optimization);
            var optimizationMask = (byte) (optimizationByte << 3); // 0000_1000 if optimization = true

            var dpsts = (byte)dps; // Name: DataParityStop + timeoutSign = DPSTS

            // Our target: dpsts = YYZX_OYYY
            dpsts |= optimizationMask; // YYYY_OYYY (O - optimization, Y - dps)
            dpsts |= timeoutSignMask; // YYYX_YYYY (X - timeoutSign, Y - dps)
            dpsts |= pauseSignMask; // YYZY_YYYY (Z - pauseSign, Y - dps)

            CommandSend(new byte[] { 0x7F, 0xD6, 0x41, 0xC1, 0x00, 0x00, dpsts }); // 
            return true;
        }

        private bool SendTimeout(ushort timeout)
        {
            var bytes = BitConverter.GetBytes(timeout);
            CommandSend(new byte[] { 0x7F, 0xD6, 0x41, 0xC2, 0x00, 0x00, bytes[0], bytes[1] }); // 
            return true;
        }

        private bool SendPause(ushort pause)
        {
            var bytes = BitConverter.GetBytes(pause);
            CommandSend(new byte[] { 0x7F, 0xD6, 0x41, 0xC4, 0x00, 0x00, bytes[0], bytes[1] });
            return true;
        }

        private bool SendAccessNotifySign(bool accessNotifySign)
        {
            var ans = Convert.ToByte(accessNotifySign);
            var cmd = new byte[] { 0x7F, 0x11, 0x41, 0x9F, 0x00, 0x00, ans }; // 7f 08 11 41 9f 00 00 01 d3
            CommandSend(cmd);
            return true;
        }

        private bool SendPauseBeforeResponseRs(ushort pause)
        {
            var bytes = BitConverter.GetBytes(pause);
            CommandSend(new byte[] { 0x7F, 0xB5, 0x41, 0xB0, 0x00, 0x00, bytes[0], bytes[1] });//7f 08 b5 41 b0 00 00 10 7a
            return true;
        }
        
        public bool SendRemoteDevices(IEnumerable<C2000Ethernet> devices)
        {
            if (devices.Count() > 15)
                return false;

            ushort addressOffset = 0x0100;

            foreach (var device in devices)
            {
                SendRemoteDeviceIp(device.AddressIP, addressOffset);
                
                var udpOffset = (ushort)(addressOffset + 0x20);
                SendRemoteDeviceUdp(device.DestinationUdp, udpOffset);

                var udpTypeOffset = (ushort) (addressOffset + 0x38);
                SendRemoteDeviceUdpType((byte)device.UdpPortType, udpTypeOffset);

                var cryptoKeyOffset = (ushort)(addressOffset + 0x22);
                SendRemoteDeviceCryptoKey(device.CryptoKey, cryptoKeyOffset);

                var macOffset = (ushort) (addressOffset + 0x32);
                SendRemoteDeviceMac(device.MACaddress, macOffset);

                addressOffset += 0x40;
            }
            
            //7f 13 06 41 00 01 00 36 33 2e 36 34 2e 36 35 2e 36 36 00 21
            return true;
        }

        private bool SendRemoteDeviceIp(string sendStr, ushort addressOffset)
        {
            if (string.IsNullOrEmpty(sendStr))
                return true;

            var byteArray = Encoding.Default.GetBytes(sendStr);
            var offsetBytes = BitConverter.GetBytes(addressOffset);
            var header = new byte[] {0x7F, 0x06, 0x41, offsetBytes[0], offsetBytes[1], 0x00};
            var stringEnd = new byte[] { 0x00 };

            var cmd = CombineArrays(header, byteArray, stringEnd);
            CommandSend(cmd);
            return true;
        }

        private bool SendRemoteDeviceUdp(ushort udp, ushort addressOffset)
        {
            if (udp == 0)
                return true;

            var bytes = BitConverter.GetBytes(udp);
            var offsetBytes = BitConverter.GetBytes(addressOffset);
            var header = new byte[] { 0x7F, 0x06, 0x41, offsetBytes[0], offsetBytes[1], 0x00 };
            
            var cmd = CombineArrays(header, bytes);
            CommandSend(cmd);
            return true;
        }

        private bool SendRemoteDeviceUdpType(byte udpType, ushort addressOffset)
        {
            var bytes = BitConverter.GetBytes(udpType);
            var offsetBytes = BitConverter.GetBytes(addressOffset);
            var header = new byte[] { 0x7F, 0x06, 0x41, offsetBytes[0], offsetBytes[1], 0x00 };

            var cmd = CombineArrays(header, bytes);
            CommandSend(cmd);
            return true;
        }

        private bool SendRemoteDeviceCryptoKey(string cryptoKey, ushort addressOffset)
        {
            // stub method
            /*
            if (string.IsNullOrEmpty(cryptoKey))
                return true;

            var bytes = Encoding.ASCII.GetBytes(cryptoKey);
            var offsetBytes = BitConverter.GetBytes(addressOffset);
            var header = new byte[] { 0x7F, 0x06, 0x41, offsetBytes[0], offsetBytes[1], 0x00 };

            var cmd = CombineArrays(header, bytes);
            CommandSend(cmd);*/
            return true;
        }

        private bool SendRemoteDeviceMac(string macString, ushort addressOffset)
        {
            //stub method
            //7f 0d e8 41 32 01 00 99 99 78 56 34 12 a1

            return true;
        }

        private bool SendMasterSlaveUdp(ushort udp)
        {
            var bytes = BitConverter.GetBytes(udp);
            CommandSend(new byte[] { 0x7F, 0xBA, 0x41, 0x40, 0x00, 0x00, bytes[0], bytes[1] });
            return true;
        }

        private bool SendConfirmationTimeout(byte timeout)
        {
            CommandSend(new byte[] { 0x7F, 0x80, 0x41, 0x42, 0x00, 0x00, timeout });
            //7f 08 80 41 42 00 00 ff a2
            return true;
        }

        private bool SendConnectionTimeout(byte timeout)
        {
            CommandSend(new byte[] { 0x7F, 0x80, 0x41, 0x43, 0x00, 0x00, timeout });
            return true;
        }

        private bool SendFreeConnectionTune(C2000Ethernet.UdpType udpType, bool allowFreeConnection)
        {
            var freeConnection = Convert.ToByte(allowFreeConnection);// 0000_0001 if allowFreeConnection = true

            var udpPortType = Convert.ToByte(udpType);
            freeConnection |= (byte)(udpPortType << 1); // 0000_0010 if udpType = static

            CommandSend(new byte[] { 0x7f, 0x0e, 0x41, 0x56, 0x00, 0x00, freeConnection });
            return true;
        }

        private bool SendFreeConnectionUdp(ushort udp)
        {
            var bytes = BitConverter.GetBytes(udp);
            // 7f 09 26 41 44 00 00 39 30 9a
            CommandSend(new byte[] { 0x7F, 0x26, 0x41, 0x44, 0x00, 0x00, bytes[0], bytes[1] });
            return true;
        }

        private bool SendTransparentTune(ushort udp, C2000Ethernet.TransparentProtocolType protocol, bool crypto)
        {
            var transparentMode = Convert.ToByte(crypto);

            transparentMode |= (byte) ((byte) protocol << 1);

            var bytes = BitConverter.GetBytes(udp);
            //7f 0a 62 41 d0 00 00 67 2b 02 57
            CommandSend(new byte[] {0x7f, 0x62, 0x41, 0xd0, 0x00, 0x00, bytes[0], bytes[1], transparentMode});
            
            return true;
        }

        private bool SendSuffix()
        {
            CommandSend(new byte[] { 0x7f, 0xa6, 0x17, 0x00, 0x00 });
            return true;
        }

        private string CommandSend(byte[] command)
        {
            for (var i = 0; i < maxRepetitions; i++)
            {
                SendPacket(command);
                while (portReceive == true) { }
                Thread.Sleep(300);
                if (!(receiveBuffer?.Length > 0)) 
                    continue;

                break;
            }
            var str = BitConverter.ToString(Encoding.ASCII.GetBytes(receiveBuffer));
            receiveBuffer = "";
            return str;
        }

        private static byte[] IntToByteArray(int intValue)
        {
            var intVal16 = (ushort)intValue;
            var result = BitConverter.GetBytes(intVal16);
            //Array.Reverse(result);
            return result;
        }

        private static byte[] IpToByteArray(string ipAddress)
        {
            var ipWithoutDots = ipAddress.Split(new char[] { '.' });
            var ipBytes = new byte[] { 0, 0, 0, 0 };
            var numberOfBytesIpV4 = 3;

            for (var counter = 0; counter <= numberOfBytesIpV4; counter++)
            {
                var numbInt32 = Int32.Parse(ipWithoutDots[numberOfBytesIpV4 - counter]);
                var numbByte = Convert.ToByte(numbInt32);
                ipBytes[counter] = numbByte;
            }
            return ipBytes;
        }

        private static byte[] CombineArrays(params byte[][] arrays)
        {
            var resultArray = new byte[arrays.Sum(a => a.Length)];
            var offset = 0;
            foreach (var item in arrays)
            {
                Buffer.BlockCopy(item, 0, resultArray, offset, item.Length);
                offset += item.Length;
            }
            return resultArray;
        }

        private static byte[] StringToByteArray(string str)
        {
            var barr = Encoding.ASCII.GetBytes(str);
            return barr;
        }
    }
}
