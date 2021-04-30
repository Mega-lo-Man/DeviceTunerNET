using DeviceTunerNET.Core;
using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Dynamic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace DeviceTunerNET.Services
{
    public class SerialSender : ISerialSender
    {
        private readonly int _packetLengthIndex = 1; //индекс байта в посылаемом пакете отвечающего за общую длину пакета
        private bool portReceive;
        private string receiveBuffer = null;
        private readonly SerialPort _serialPort;
        private readonly IEventAggregator _ea;

        /// <summary>
        /// Болидовская таблица CRC
        /// </summary>
        private readonly byte[] crc8Table = new byte[]
        {
            0x00, 0x5E, 0x0BC, 0x0E2, 0x61, 0x3F, 0x0DD, 0x83, 0x0C2, 0x9C, 0x7E, 0x20, 0x0A3, 0x0FD, 0x1F, 0x41,
            0x9D, 0x0C3, 0x21, 0x7F, 0x0FC, 0x0A2, 0x40, 0x1E, 0x5F, 0x01, 0x0E3, 0x0BD, 0x3E, 0x60, 0x82, 0x0DC,
            0x23, 0x7D, 0x9F, 0x0C1, 0x42, 0x1C, 0x0FE, 0x0A0, 0x0E1, 0x0BF, 0x5D, 0x03, 0x80, 0x0DE, 0x3C, 0x62,
            0x0BE, 0x0E0, 0x02, 0x5C, 0x0DF, 0x81, 0x63, 0x3D, 0x7C, 0x22, 0x0C0, 0x9E, 0x1D, 0x43, 0x0A1, 0x0FF,
            0x46, 0x18, 0x0FA, 0x0A4, 0x27, 0x79, 0x9B, 0x0C5, 0x84, 0x0DA, 0x38, 0x66, 0x0E5, 0x0BB, 0x59, 0x07,
            0x0DB, 0x85, 0x67, 0x39, 0x0BA, 0x0E4, 0x06, 0x58, 0x19, 0x47, 0x0A5, 0x0FB, 0x78, 0x26, 0x0C4, 0x9A,
            0x65, 0x3B, 0x0D9, 0x87, 0x04, 0x5A, 0x0B8, 0x0E6, 0x0A7, 0x0F9, 0x1B, 0x45, 0x0C6, 0x98, 0x7A, 0x24,
            0x0F8, 0x0A6, 0x44, 0x1A, 0x99, 0x0C7, 0x25, 0x7B, 0x3A, 0x64, 0x86, 0x0D8, 0x5B, 0x05, 0x0E7, 0x0B9,
            0x8C, 0x0D2, 0x30, 0x6E, 0x0ED, 0x0B3, 0x51, 0x0F, 0x4E, 0x10, 0x0F2, 0x0AC, 0x2F, 0x71, 0x93, 0x0CD,
            0x11, 0x4F, 0x0AD, 0x0F3, 0x70, 0x2E, 0x0CC, 0x92, 0x0D3, 0x8D, 0x6F, 0x31, 0x0B2, 0x0EC, 0x0E, 0x50,
            0x0AF, 0x0F1, 0x13, 0x4D, 0x0CE, 0x90, 0x72, 0x2C, 0x6D, 0x33, 0x0D1, 0x8F, 0x0C, 0x52, 0x0B0, 0x0EE,
            0x32, 0x6C, 0x8E, 0x0D0, 0x53, 0x0D, 0x0EF, 0x0B1, 0x0F0, 0x0AE, 0x4C, 0x12, 0x91, 0x0CF, 0x2D, 0x73,
            0x0CA, 0x94, 0x76, 0x28, 0x0AB, 0x0F5, 0x17, 0x49, 0x08, 0x56, 0x0B4, 0x0EA, 0x69, 0x37, 0x0D5, 0x8B,
            0x57, 0x09, 0x0EB, 0x0B5, 0x36, 0x68, 0x8A, 0x0D4, 0x95, 0x0CB, 0x29, 0x77, 0x0F4, 0x0AA, 0x48, 0x16,
            0x0E9, 0x0B7, 0x55, 0x0B, 0x88, 0x0D6, 0x34, 0x6A, 0x2B, 0x75, 0x97, 0x0C9, 0x4A, 0x14, 0x0F6, 0x0A8,
            0x74, 0x2A, 0x0C8, 0x96, 0x15, 0x4B, 0x0A9, 0x0F7, 0x0B6, 0x0FC, 0x0A, 0x54, 0x0D7, 0x89, 0x6B, 0x35
        };

        

        private Dictionary<byte, string> BolidDict = new Dictionary<byte, string>()
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
            { 33, "\"РИП-12 исп.50, \" исп.51, \" без исполнения\"" },
            { 34, "Сигнал-10" },
            { 36, "С2000-ПП" },
            { 38, "РИП-12 исп.54" },
            { 39, "\"РИП-24 исп.50, \" исп.51\"" },
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
            if (!_serialPort.IsOpen)
            {
                _serialPort.PortName = comPortName;
                
                _serialPort.Open();


                byte[] result = Transaction(new byte[] { deviceAddress, 0xB0, 0x0F, newDeviceAddress, newDeviceAddress });
                
                _serialPort.Close();
                if(result.Length > 1)
                {
                    if (result[4] == newDeviceAddress)
                        return true;
                }
            }
            return false;
        }

        public string GetDeviceModel(string comPortName, byte deviceAddress)
        {
            
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

                byte[] deviceModel = Transaction(new byte[] { deviceAddress, 0x92, 0x0D, 0x00, 0x00 });
                
                _serialPort.Close();
                if (deviceModel?.Length > 1)
                {
                    byte devType = deviceModel[3];
                    return BolidDict[devType];
                }
            }
            else
            {
                byte[] deviceModel = Transaction(new byte[] { deviceAddress, 0x92, 0x0D, 0x00, 0x00 });

                if (deviceModel?.Length > 1)
                {
                    byte devType = deviceModel[3];
                    return BolidDict[devType];
                }
            }
            
            return "";
        }

        public bool IsDeviceOnline(string comPortName, byte deviceAddress)
        {
            throw new NotImplementedException();
        }

        public Dictionary<byte, string> SearchOnlineDevices(string comPortName)
        {
            if (!_serialPort.IsOpen)
            {
                _serialPort.PortName = comPortName;
                _serialPort.Open();
            }
            Dictionary<byte, string> result = new Dictionary<byte, string>();
            for (byte devAddr = 1; devAddr <= 127; devAddr++)
            {
                string OnlineDevicesModel = GetDeviceModel(comPortName, devAddr);
                if (OnlineDevicesModel != String.Empty)
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

        private byte[] Transaction(byte[] sendArray)
        {
            
            // make DataReceived event handler
            _serialPort.DataReceived += sp_DataReceived;

            for (int i = 0; i < 4; i++)
            {
                SendPacket(sendArray);
                while (portReceive == true) { }
                Thread.Sleep(250);
                if (receiveBuffer != null)
                    break;
            }
            if (receiveBuffer != null)
            {
                string result = receiveBuffer;
                receiveBuffer = null;
                return Encoding.ASCII.GetBytes(result);
            }
            else
            {
                return new byte[1] { 0x00 };
            }
                
        }

        private void SendPacket(byte[] sendArray)
        {
            byte bytesCounter = 1; //сразу начнём считать с единицы, т.к. всё равно придётся добавить один байт(сам байт длины команды)
            List<byte> lst = new List<byte>();
            foreach (byte bt in sendArray)
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
            for (var i = 0; i < bytes.Length; i++)
                crc = crc8Table[crc ^ bytes[i]];

            byte[] chr = new byte[1];
            chr[0] = crc;
            return chr;
        }

        public ObservableCollection<string> GetAvailableCOMPorts()
        {
            string[] ports = SerialPort.GetPortNames();
            ObservableCollection<string> portsList = new ObservableCollection<string>();
            foreach(string port in ports)
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
            SerialPort sPort = (SerialPort)sender;
            string data = sPort.ReadExisting();
            foreach (char ch in data)
            {
                int value = Convert.ToInt32(ch);
                //Console.Write(value.ToString("X") + " ");
            }
            receiveBuffer += data;
            Debug.WriteLine("ReceiveBuffer: " + receiveBuffer + "  Length: " + receiveBuffer.Length);
            portReceive = false;
        }

        public bool SetC2000EthernetConfig(string ComPortName, byte deviceAddress, C2000Ethernet device)
        {
            bool result = false;
            if (!_serialPort.IsOpen)
            {
                _serialPort.PortName = ComPortName;
                
                _serialPort.Open();
                // make DataReceived event handler
                _serialPort.DataReceived += sp_DataReceived;

                
                result = SendPromoter();
                result = SendDeviceNetName(device.NetName);
                result = SendEthernetTune(device.AddressIP, device.Netmask, device.DefaultGateway, device.FirstDns, device.SecondDns);
                //result = SendUnrecognized2();
                //result = SendOtherDevicesIP(device.RemoteIpList, device.AddressIP);
                //result = SendUnrecognized3();
                //result = SendNetmaskAndOtherDevicesUDP(device.Netmask, device.RemoteUDPList, device.UDPSender, device.UDPRemote);

                _serialPort.Close();
            }
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
            byte[] deviceName = StringToByteArray(name);
            byte[] header = new byte[] { 0x7F, 0x16, 0x7B, 0x41, 0x00, 0x00, 0x00 };
            byte[] resultCmd = CombineArrays(header, deviceName);

            CommandSend(resultCmd);
            return true;
        }

        private bool SendEthernetTune(string ip, string netmask, string gateway, string firstDNS, string secondDNS)
        {
            byte[] _ip = IpToByteArray(ip);
            byte[] _netmask = IpToByteArray(netmask);
            byte[] _gateway = IpToByteArray(gateway);
            byte[] _firstDNS = IpToByteArray(firstDNS);
            byte[] _secondDNS = IpToByteArray(secondDNS);
            byte[] _header = new byte[] { 0x7F, 0xC9, 0x41, 0x20, 0x00, 0x00 };
            byte[] _cmd = CombineArrays(_header, _ip, _netmask, _gateway, _firstDNS, _secondDNS);
            CommandSend(_cmd);
            return true;
        }

        private bool SendUnrecognized2()
        {
            CommandSend(new byte[] { 0x7F, 0xB5, 0x41, 0x07, 0x1D, 0x00, 0x01 }); // 7f 08 b5 41 07 1d 00 01
            CommandSend(new byte[] { 0x7F, 0xF4, 0x41, 0x22, 0x1D, 0x00, 0x02, 0x01, 0x00 });
            return true;
        }

        private bool SendUnrecognized3()
        {
            CommandSend(new byte[] { 0x7F, 0xC8, 0x41, 0x60, 0x1D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }); // 7f 17 c8 41 60 1d 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
            CommandSend(new byte[] { 0x7F, 0xBE, 0x41, 0x70, 0x1D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }); // 7f 17 be 41 70 1d 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
            return true;
        }

        private bool SendNetmaskAndOtherDevicesUDP(string netmask, List<int> UDPlist, int UDPsender, int FreeRemoteDeviceUDP)
        {
            byte[] udp1 = IntToByteArray(UDPlist[0]);
            byte[] udp2 = IntToByteArray(UDPlist[1]);
            byte[] udp3 = IntToByteArray(UDPlist[2]);
            byte[] udp4 = IntToByteArray(UDPlist[3]);
            byte[] udp5 = IntToByteArray(UDPlist[4]);
            byte[] udp6 = IntToByteArray(UDPlist[5]);
            byte[] udp7 = IntToByteArray(UDPlist[6]);
            byte[] udp8 = IntToByteArray(UDPlist[7]);
            byte[] udp9 = IntToByteArray(UDPlist[8]);

            byte[] mask = IpToByteArray(netmask);
            byte[] udpSender = IntToByteArray(UDPsender);
            byte[] freeRemoteUDP = IntToByteArray(FreeRemoteDeviceUDP);

            CommandSend(new byte[] { 0x7F, 0x1C, 0x41, 0x80, 0x1D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, mask[0], mask[1], mask[2], mask[3], udpSender[0], udpSender[1], udp1[0], udp1[1], udp2[0], udp2[1] }); // 7f 17 1c 41 80 1d 00 00 00 00 00 00 00 ff ff ff 00 40 9c 40 9c 40 9c
            CommandSend(new byte[] { 0x7F, 0x2D, 0x41, 0x90, 0x1D, 0x00, udp3[0], udp3[1], udp4[0], udp4[1], udp5[0], udp5[1], udp6[0], udp6[1], udp7[0], udp7[1], udp8[0], udp8[1], udp9[0], udp9[1], freeRemoteUDP[0], freeRemoteUDP[1] }); // 7f 17 2d 41 90 1d 00 40 9c 40 9c 40 9c 40 9c 40 9c 40 9c 40 9c 41 9c
            return true;
        }

        private bool SendOtherDevicesIP(List<string> otherDevicesIpList, string deviceIp)
        {

            byte[] ipAddr1 = IpToByteArray(otherDevicesIpList[0]);
            byte[] ipAddr2 = IpToByteArray(otherDevicesIpList[1]);
            byte[] ipAddr3 = IpToByteArray(otherDevicesIpList[2]);
            byte[] ipAddr4 = IpToByteArray(otherDevicesIpList[3]);
            byte[] ipAddr5 = IpToByteArray(otherDevicesIpList[4]);
            byte[] ipAddr6 = IpToByteArray(otherDevicesIpList[5]);
            byte[] ipAddr7 = IpToByteArray(otherDevicesIpList[6]);
            byte[] ipAddr8 = IpToByteArray(otherDevicesIpList[7]);
            byte[] ipAddr9 = IpToByteArray(otherDevicesIpList[8]);

            byte[] ipSelf = IpToByteArray(deviceIp);

            CommandSend(new byte[] { 0x7F, 0x9A, 0x41, 0x26, 0x1D, 0x00, 0x7F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x50, 0x00, ipAddr1[0], ipAddr1[1], ipAddr1[2], ipAddr1[3] });
            CommandSend(new byte[] { 0x7F, 0x7B, 0x41, 0x36, 0x1D, 0x00, ipAddr2[0], ipAddr2[1], ipAddr2[2], ipAddr2[3], ipAddr3[0], ipAddr3[1], ipAddr3[2], ipAddr3[3], ipAddr4[0], ipAddr4[1] });
            CommandSend(new byte[] { 0x7F, 0x8F, 0x41, 0x40, 0x1D, 0x00, ipAddr4[2], ipAddr4[3], ipAddr5[0], ipAddr5[1], ipAddr5[2], ipAddr5[3], ipAddr6[0], ipAddr6[1], ipAddr6[2], ipAddr6[3], ipAddr7[0], ipAddr7[1], ipAddr7[2], ipAddr7[3], ipAddr8[0], ipAddr8[1] });
            CommandSend(new byte[] { 0x7F, 0x44, 0x41, 0x50, 0x1D, 0x00, ipAddr8[2], ipAddr8[3], ipAddr9[0], ipAddr9[1], ipAddr9[2], ipAddr9[3], 0x00, 0x00, 0x00, 0x00, ipSelf[0], ipSelf[1], ipSelf[2], ipSelf[3], 0x00, 0x00 });
            return true;
        }

        private string CommandSend(byte[] command)
        {
            for (int i = 0; i < 3; i++)
            {
                SendPacket(command);
                while (portReceive == true) { }
                Thread.Sleep(200);
                if (receiveBuffer?.Length > 0)
                    break;
            }
            string str = BitConverter.ToString(Encoding.ASCII.GetBytes(receiveBuffer));
            receiveBuffer = "";
            return str;
        }

        private static byte[] IntToByteArray(int intValue)
        {
            UInt16 intVal16 = (UInt16)intValue;
            byte[] result = BitConverter.GetBytes(intVal16);
            //Array.Reverse(result);
            return result;
        }

        private static byte[] IpToByteArray(string ipAddress)
        {
            string[] ipWithoutDots = ipAddress.Split(new char[] { '.' });
            byte[] ipBytes = new byte[] { 0, 0, 0, 0 };
            int numberOfBytesIpV4 = 3;

            for (int counter = 0; counter <= numberOfBytesIpV4; counter++)
            {
                int numbInt32 = Int32.Parse(ipWithoutDots[numberOfBytesIpV4 - counter]);
                byte numbByte = Convert.ToByte(numbInt32);
                ipBytes[counter] = numbByte;
            }
            return ipBytes;
        }

        private static byte[] CombineArrays(params byte[][] arrays)
        {
            byte[] resultArray = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach(byte[] item in arrays)
            {
                System.Buffer.BlockCopy(item, 0, resultArray, offset, item.Length);
                offset += item.Length;
            }
            return resultArray;
        }

        private static byte[] StringToByteArray(string str)
        {
            byte[] barr = Encoding.ASCII.GetBytes(str);
            return barr;
        }
    }
}
