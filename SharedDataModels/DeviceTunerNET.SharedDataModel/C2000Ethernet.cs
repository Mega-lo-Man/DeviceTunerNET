using System;
using System.Collections.Generic;

namespace DeviceTunerNET.SharedDataModel
{
    public class C2000Ethernet : RS232device
    {
        public enum Duplex
        {
            half = 0, 
            full = 1
        }

        public enum Mode
        {
            transparent = 0, 
            slave = 1, 
            master = 2
        }

        public enum ProtocolType
        {
            RS485 = 0, 
            RS232 = 1
        }

        public enum Speed
        {
            b1200 = 0, 
            b2400 = 1, 
            b4800 = 2, 
            b9600 = 3, 
            b19200 = 4, 
            b38400 = 5, 
            b57600 = 6, 
            b115200 = 7
        }

        public enum DataParityStop
        {
            data8Stop1 = 0b0000_0000,
            data8Stop1Parity1 = 0b0000_0001,
            data8Stop1Parity0 = 0b0000_0010,
            data9Stop1 = 0b0000_0011,
            data8Stop2 = 0b0000_0100,
            data8Stop2Parity1 = 0b0000_0101,
            data8Stop2Parity0 = 0b0000_0110,
            data9Stop2 = 0b0000_0111,
            data7Stop1Parity1 = 0b0100_0000,
            data7Stop1Parity0 = 0b1000_0000
        }

        public enum UdpType
        {
            dynamicUdp = 0x00,
            staticUdp = 0x01
        }

        public enum TransparentProtocolType
        {
            other = 0x00,
            S2000 = 0x01,
            vCom = 0x02
        }

        private string _remoteDefaultFirstIP = "192.168.0.1";

        private List<byte[]> _configLineList;

        #region Properties

        public string NetName { get; set; } = "ABCDEFG";

        public string FirstDns { get; set; } = "0.0.0.0";

        public string SecondDns { get; set; } = "0.0.0.0";

        public Mode NetworkMode { get; set; }

        public ProtocolType InterfaceType { get; set; }

        /// <summary>
        /// Бодовая скорость работы «C2000-Ethernet» по интерфейсу RS-232/RS-485. 
        /// </summary>
        public Speed ConnectionSpeed { get; set; }

        /// <summary>
        /// Количество бит данных и стоповых бит.
        /// </summary>
        public DataParityStop FrameFormat { get; set; }

        public bool TimeoutSign { get; set; }

        public ushort Timeout { get; set; }

        public bool PauseSign { get; set; }

        /// <summary>
        /// Пауза между пакетами (при передаче в RS данных).
        /// </summary>
        public ushort Pause { get; set; }

        /// <summary>
        /// Режим  с  оптимизацией  данных.
        /// </summary>
        public bool Optimization { get; set; }

        /// <summary>
        /// Признак формирования уведомлений по доступу.
        /// </summary>
        public bool AccessNotifySign { get; set; }

        /// <summary>
        /// Пауза перед ответом по RS. (1 шаг = 0,125 сек)
        /// </summary>
        public ushort PauseBeforeResponseRs { get; set; }

        //public List<int> RemoteUDPList { get; set; } = new List<int>();

        public List<C2000Ethernet> RemoteDevicesList { get; set; } = new List<C2000Ethernet>();

        public string RemoteIpTrasparentMode { get; set; }

        public Duplex DuplexMode { get; set; }

        //public int UDPSender { get; set; }

        public ushort DestinationUdp { get; set; }

        public ushort MasterSlaveUdp { get; set; }

        public ushort FreeConnectionUdp { get; set; }

        public int WaitingTimeout { get; set; }

        public bool UseSingleReadWriteUDP { get; set; }

        public bool Dhcp { get; set; } = false;

        public UdpType UdpPortType { get; set; }

        public string CryptoKey { get; set; }

        public byte ConfirmationTimeout { get; set; }

        public byte ConnectionTimeout { get; set; }

        public bool AllowFreeConnection { get; set; }

        public UdpType FreeConnectionUdpType { get; set; }

        public ushort TransparentUdp { get; set; }

        public bool TransparentCrypto { get; set; }

        public TransparentProtocolType TransparentProtocol { get; set; }

        #endregion Properties

        public C2000Ethernet()
        {
            //------------------------------------------------
            /*for (var i = 0; i < 9; i++)
            {
                RemoteUDPList.Add(40000);
            }*/
            //------------------------------------------------
            AddressIP = "192.168.2.11";
            Netmask = "255.255.252.0";
            DefaultGateway = "0.0.0.0";
            Netmask = "255.255.252.0";
            DuplexMode = (int)Duplex.half;
            //UDPSender = 40000;
            DestinationUdp = 40001;
            FreeConnectionUdp = 40001;
            UseSingleReadWriteUDP = true;
            WaitingTimeout = 80;
            AddressRS232 = 127;
            AddressRS485 = 127;
            NetworkMode = Mode.transparent;
            InterfaceType = ProtocolType.RS485;
            ConnectionSpeed = Speed.b9600;
            FrameFormat = DataParityStop.data8Stop1;
            TimeoutSign = false;
            Timeout = 0;
            PauseSign = true;
            Pause = 6;
            Optimization = true;
            AccessNotifySign = true;
            PauseBeforeResponseRs = 0x10;
            /*RemoteDevicesList = new List<C2000Ethernet>()
            {
                new C2000Ethernet()
                {
                    AddressIP = "192.168.1.127"
                }
            };*/
            DestinationUdp = 40000;
            MasterSlaveUdp = 40001;
            ConfirmationTimeout = 0x02;
            ConnectionTimeout = 0x1E;
            
            AllowFreeConnection = true;
            FreeConnectionUdpType = UdpType.staticUdp;

            TransparentUdp = 40000;
            TransparentProtocol = TransparentProtocolType.S2000;
            TransparentCrypto = false;
            //------------------------------------------------
            _configLineList = new List<byte[]>();
            UdpPortType = UdpType.staticUdp;
        }


        public IList<byte[]> GetConfigCommandLine(byte RS232address)
        {
            var addr = RS232address;
            _configLineList.Clear();

            SendPromoter(addr);
            SendGatewayAndUDP(addr, DefaultGateway);
            SendPromoter2(addr);
            //SendOtherDevicesIP(addr, RemoteIpList, AddressIP);
            SendPromoter3(addr);
            //SendNetmaskAndDevicesUDP(addr, Netmask, RemoteUDPList, UDPSender, FreeRemoteUdp);

            return _configLineList;
        }


        private bool SendPromoter(byte address)
        {
            _configLineList.Add(new byte[] { address, 0xca, 0x0D, 0x00, 0x00 }); // 7f 06 ca 0d 00 00
            _configLineList.Add(new byte[] { address, 0x01, 0x43, 0x00, 0x00, 0x00, 0x10 }); // 7f 08 01 43 00 00 00 10
            _configLineList.Add(new byte[] { address, 0x1F, 0x43, 0x00, 0x00, 0x00, 0x10 }); // 7f 08 1f 43 00 00 00 10
            return true;
        }

        private bool SendGatewayAndUDP(byte address, string ip)
        {
            var ipAddr = IpToByteArray(ip);
            _configLineList.Add(new byte[] { address, 0xA1, 0x41, 0x00, 0x1C, 0x00, ipAddr[0], ipAddr[1], ipAddr[2], ipAddr[3], 0x01, 0x02, 0x41, 0x9C, 0x01, 0x01 });
            return true;
        }

        private bool SendPromoter2(byte address)
        {
            _configLineList.Add(new byte[] { address, 0xB5, 0x41, 0x07, 0x1D, 0x00, 0x01 }); // 7f 08 b5 41 07 1d 00 01
            _configLineList.Add(new byte[] { address, 0xF4, 0x41, 0x22, 0x1D, 0x00, 0x02, 0x01, 0x00 });
            return true;
        }

        private bool SendPromoter3(byte address)
        {
            _configLineList.Add(new byte[] { address, 0xC8, 0x41, 0x60, 0x1D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }); // 7f 17 c8 41 60 1d 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
            _configLineList.Add(new byte[] { address, 0xBE, 0x41, 0x70, 0x1D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }); // 7f 17 be 41 70 1d 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
            return true;
        }

        private bool SendNetmaskAndDevicesUDP(byte address, string netmask, List<int> UDPlist, int UdpSender, int FreeRemoteDeviceUdp)
        {
            var udp1 = IntToByteArray(UDPlist[0]);
            var udp2 = IntToByteArray(UDPlist[1]);
            var udp3 = IntToByteArray(UDPlist[2]);
            var udp4 = IntToByteArray(UDPlist[3]);
            var udp5 = IntToByteArray(UDPlist[4]);
            var udp6 = IntToByteArray(UDPlist[5]);
            var udp7 = IntToByteArray(UDPlist[6]);
            var udp8 = IntToByteArray(UDPlist[7]);
            var udp9 = IntToByteArray(UDPlist[8]);

            var mask = IpToByteArray(netmask);
            var udpSender = IntToByteArray(UdpSender);
            var freeRemoteUDP = IntToByteArray(FreeRemoteDeviceUdp);

            _configLineList.Add(new byte[] { address, 0x1C, 0x41, 0x80, 0x1D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, mask[0], mask[1], mask[2], mask[3], udpSender[0], udpSender[1], udp1[0], udp1[1], udp2[0], udp2[1] }); // 7f 17 1c 41 80 1d 00 00 00 00 00 00 00 ff ff ff 00 40 9c 40 9c 40 9c
            _configLineList.Add(new byte[] { address, 0x2D, 0x41, 0x90, 0x1D, 0x00, udp3[0], udp3[1], udp4[0], udp4[1], udp5[0], udp5[1], udp6[0], udp6[1], udp7[0], udp7[1], udp8[0], udp8[1], udp9[0], udp9[1], freeRemoteUDP[0], freeRemoteUDP[1] }); // 7f 17 2d 41 90 1d 00 40 9c 40 9c 40 9c 40 9c 40 9c 40 9c 40 9c 41 9c
            return true;
        }

        private bool SendOtherDevicesIP(byte address, List<string> otherDevicesIpList, string deviceIp)
        {

            var ipAddr1 = IpToByteArray(otherDevicesIpList[0]);
            var ipAddr2 = IpToByteArray(otherDevicesIpList[1]);
            var ipAddr3 = IpToByteArray(otherDevicesIpList[2]);
            var ipAddr4 = IpToByteArray(otherDevicesIpList[3]);
            var ipAddr5 = IpToByteArray(otherDevicesIpList[4]);
            var ipAddr6 = IpToByteArray(otherDevicesIpList[5]);
            var ipAddr7 = IpToByteArray(otherDevicesIpList[6]);
            var ipAddr8 = IpToByteArray(otherDevicesIpList[7]);
            var ipAddr9 = IpToByteArray(otherDevicesIpList[8]);

            var ipSelf = IpToByteArray(deviceIp);

            _configLineList.Add(new byte[] { address, 0x9A, 0x41, 0x26, 0x1D, 0x00, 0x7F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x50, 0x00, ipAddr1[0], ipAddr1[1], ipAddr1[2], ipAddr1[3] });
            _configLineList.Add(new byte[] { address, 0x7B, 0x41, 0x36, 0x1D, 0x00, ipAddr2[0], ipAddr2[1], ipAddr2[2], ipAddr2[3], ipAddr3[0], ipAddr3[1], ipAddr3[2], ipAddr3[3], ipAddr4[0], ipAddr4[1] });
            _configLineList.Add(new byte[] { address, 0x8F, 0x41, 0x40, 0x1D, 0x00, ipAddr4[2], ipAddr4[3], ipAddr5[0], ipAddr5[1], ipAddr5[2], ipAddr5[3], ipAddr6[0], ipAddr6[1], ipAddr6[2], ipAddr6[3], ipAddr7[0], ipAddr7[1], ipAddr7[2], ipAddr7[3], ipAddr8[0], ipAddr8[1] });
            _configLineList.Add(new byte[] { address, 0x44, 0x41, 0x50, 0x1D, 0x00, ipAddr8[2], ipAddr8[3], ipAddr9[0], ipAddr9[1], ipAddr9[2], ipAddr9[3], 0x00, 0x00, 0x00, 0x00, ipSelf[0], ipSelf[1], ipSelf[2], ipSelf[3], 0x00, 0x00 });
            return true;
        }

        private byte[] IntToByteArray(int intValue)
        {
            var intVal16 = (ushort)intValue;
            var result = BitConverter.GetBytes(intVal16);
            //Array.Reverse(result);
            return result;
        }

        private byte[] IpToByteArray(string ipAddress)
        {
            var ipWithoutDots = ipAddress.Split(new char[] { '.' });
            var ipBytes = new byte[] { 0, 0, 0, 0 };

            for (var counter = 0; counter < 4; counter++)
            {
                var numbInt32 = int.Parse(ipWithoutDots[counter]);
                var numbByte = Convert.ToByte(numbInt32);
                ipBytes[counter] = numbByte;
            }
            return ipBytes;
        }
    }
}
