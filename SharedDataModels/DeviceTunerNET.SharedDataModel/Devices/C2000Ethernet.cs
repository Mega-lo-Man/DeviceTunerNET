using DeviceTunerNET.SharedDataModel.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using static DeviceTunerNET.SharedDataModel.Devices.IOrionNetTimeouts;
using static System.String;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000Ethernet : EthernetOrionDevice, ITransaction, IOrionNetTimeouts
    {
        private const int MacAddressLength = 6;

        public new const int ModelCode = 29;
        public new const int Code = 29;

        #region Enums
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
        #endregion Enums

        #region Properties
        /// <summary>
        /// Сетевое имя
        /// </summary>
        public string NetName { get; set; } = "ABCDEFG";

        /// <summary>
        /// DHCP client enable
        /// </summary>
        public bool DhcpEnable { get; set; }


        /// <summary>
        /// Адрес прибора на линии RS-232 ("24")
        /// </summary>
        private int? _address_RS232;
        public int? AddressRS232
        {
            get => _address_RS232;
            set
            {
                if (value > 0 && value <= 127) _address_RS232 = value;
            }
        }

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

        #region Constructors
        public C2000Ethernet(IPort port) : base(port)
        {
            Model = "С2000-Ethernet";
            SupportedModels = new List<string>
            {
                Model,
            };

            AddressIP = "192.168.2.11";
            Netmask = "255.255.254.0";
            DefaultGateway = "0.0.0.0";
            DuplexMode = (int)Duplex.half;
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
            MasterSlaveUdp = 40001;
            ConfirmationTimeout = 0x02;
            ConnectionTimeout = 0x1E;

            AllowFreeConnection = true;
            FreeConnectionUdpType = UdpType.staticUdp;

            TransparentUdp = 40000;
            TransparentProtocol = TransparentProtocolType.S2000;
            TransparentCrypto = false;

            UdpPortType = UdpType.staticUdp;
        }
        #endregion Constructors

        public byte[] Transaction(byte address, byte[] sendArray)
        {
            return AddressTransaction(address, sendArray, Timeouts.ethernetConfig);
        }

        //We must override default implementaition of the Setup() because in C2000-Ethernet we need upload config and after that change address
        public override bool Setup(Action<int> progressStatus, int modelCode = 0)
        {
            var result = GetModelCode((byte)defaultAddress, out var currentDeviceCode);
            
            if(!result)
                return false;

            if (currentDeviceCode != ModelCode)
            {
                return false;
            }

            WriteBaseConfig(progressStatus);

            SetAddress();

            return true;
        }

        public override void WriteConfig(Action<int> progressStatus)
        {
            UploadConfig(1.66, defaultAddress, GetConfig(), progressStatus);
        }

        public override void WriteBaseConfig(Action<int> progressStatus)
        {
            UploadConfig(3.0, defaultAddress, GetBaseConfig(), progressStatus);
        }

        private void UploadConfig(double progressStep, uint address, IEnumerable<byte[]> config, Action<int> progressStatus)
        {
            var progress = 1.0;
            progressStatus(Convert.ToInt32(progress));
            //Reboot();

            //Thread.Sleep((int)Timeouts.restartC2000Ethernet);

            foreach (var command in config)
            {
                if (Transaction((byte)address, command) == null)
                    throw new Exception("Transaction false!");

                progressStatus(Convert.ToInt32(progress));
                progress += progressStep;
                Debug.WriteLine(progress.ToString());
            }

            Reboot();

            Thread.Sleep((int)Timeouts.restartC2000Ethernet);

            progressStatus(100);
        }

        private IEnumerable<byte[]> GetBaseConfig()
        {
            var config = new List<byte[]>
            {
                ReadDeviceNetworkSettings(),
                GetDeviceNetName(NetName),
                GetEthernetTune(AddressIP, Netmask, DefaultGateway, FirstDns, SecondDns),
                GetDhcpStatus(DhcpEnable),
                GetDhcpStatus(Dhcp),
                GetMasterSlaveTransparent(NetworkMode),
                GetInterfaceType(InterfaceType),

                GetMasterSlaveUdp(MasterSlaveUdp),

                GetFreeConnectionTune(FreeConnectionUdpType, AllowFreeConnection),
                GetFreeConnectionUdp(FreeConnectionUdp),
                GetTransparentTune(TransparentUdp, TransparentProtocol, TransparentCrypto),
                GetSecondPowerControl(),
            };
            config.AddRange(GetRemoteDevices(RemoteDevicesList));

            return config;
        }

        public IEnumerable<byte[]> GetConfig()
        {
            var config = new List<byte[]>
            {
                ReadDeviceNetworkSettings(),
                GetDeviceNetName(NetName),
                GetEthernetTune(AddressIP, Netmask, DefaultGateway, FirstDns, SecondDns),
                GetDhcpStatus(DhcpEnable),
                GetDhcpStatus(Dhcp),
                GetMasterSlaveTransparent(NetworkMode),
                GetInterfaceType(InterfaceType),
                GetConnectionSpeed(ConnectionSpeed),
                GetParityStopTimeoutSign(FrameFormat, TimeoutSign, PauseSign, Optimization),
                GetTimeout(Timeout),
                GetPause(Pause),
                GetAccessNotifySign(AccessNotifySign),
                GetPauseBeforeResponseRs(PauseBeforeResponseRs),
                GetMasterSlaveUdp(MasterSlaveUdp),
                GetConfirmationTimeout(ConfirmationTimeout),
                GetConnectionTimeout(ConnectionTimeout),
                GetFreeConnectionTune(FreeConnectionUdpType, AllowFreeConnection),
                GetFreeConnectionUdp(FreeConnectionUdp),
                GetTransparentTune(TransparentUdp, TransparentProtocol, TransparentCrypto),
                GetSecondPowerControl(),
            };
            config.AddRange(GetRemoteDevices(RemoteDevicesList));

            return config;
        }
        private string CidrToString(int cidr)
        {
            var range = 0xFFFFFFFF;
            range <<= 32 - cidr;
            var fourBytes = new[] { "0", "0", "0", "0" };

            for (var i = 3; i >= 0; i--)
            {
                var x = range & 255;
                fourBytes[i] = x.ToString();
                range >>= 8;

            }
            return Join(".", fourBytes);
        }

        private int ConvertToCidr(string address)
        {
            var addr = address;
            var range = (uint)ConvertStringToRange(addr);
            var bitsCounter = 0;

            while (range > 0)
            {
                if ((range & 1) >= 0)
                    ++bitsCounter;
                range <<= 1;
            }
            return bitsCounter;
        }

        private int ConvertStringToRange(string addrStr)
        {
            var textAddress = addrStr;
            var result = 0;
            var bytesArray = textAddress.Split(new char[] { '.' });
            for (var i = 0; i < 4; i++)
            {
                result <<= 8;
                result |= int.Parse(bytesArray[i]);

            }
            return result;
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

        private static byte[] IpToByteArray(string ipAddress)
        {
            var ipWithoutDots = ipAddress.Split(new char[] { '.' });
            var ipBytes = new byte[] { 0, 0, 0, 0 };
            var numberOfBytesIpV4 = 3;

            for (var counter = 0; counter <= numberOfBytesIpV4; counter++)
            {
                var numbInt32 = int.Parse(ipWithoutDots[numberOfBytesIpV4 - counter]);
                var numbByte = Convert.ToByte(numbInt32);
                ipBytes[counter] = numbByte;
            }
            return ipBytes;
        }

        private byte[] GetDeviceNetName(string name)
        {
            var deviceName = Encoding.ASCII.GetBytes(name);
            var header = new byte[] { 0x41, 0x00, 0x00, 0x00 };
            var resultCmd = CombineArrays(header, deviceName);
            return resultCmd;
        }

        private byte[] GetEthernetTune(string ip, string netmask, string gateway, string firstDNS, string secondDNS)
        {
            var _ip = IpToByteArray(ip);
            var _netmask = IpToByteArray(netmask);
            var _gateway = IpToByteArray(gateway);
            var _firstDNS = IpToByteArray(firstDNS);
            var _secondDNS = IpToByteArray(secondDNS);
            var _header = new byte[] { 0x41, 0x20, 0x00, 0x00 };
            var cmd = CombineArrays(_header, _ip, _netmask, _gateway, _firstDNS, _secondDNS);

            return cmd;
        }

        private byte[] GetDhcpStatus(bool dhcpEnable)
        {
            var dhcp = Convert.ToByte(dhcpEnable);
            var cmd = new byte[] { 0x41, 0x3A, 0x00, 0x00, dhcp }; //7f 08 0f 41 61 00 00 01

            return cmd;
        }

        private byte[] GetMasterSlaveTransparent(Mode mode)
        {
            var cmd = new byte[] { 0x41, 0x61, 0x00, 0x00, (byte)mode }; // 7f 08 8b 41 61 00 00 00
            return cmd;
        }

        private byte[] GetInterfaceType(ProtocolType rsType)
        {
            var cmd = new byte[] { 0x41, 0x60, 0x00, 0x00, (byte)rsType }; // 7f 08 50 41 60 00 00 01
            return cmd;
        }

        private byte[] GetConnectionSpeed(Speed speed)
        {
            var cmd = new byte[] { 0x41, 0xC0, 0x00, 0x00, (byte)speed }; // 7f 08 d6 41 c0 00 00 03
            return cmd;
        }

        private byte[] GetParityStopTimeoutSign(
            DataParityStop dps,
            bool timeoutSign,
            bool pauseSign,
            bool optimization)
        {
            var timeoutSignByte = Convert.ToByte(timeoutSign);
            var timeoutSignMask = (byte)(timeoutSignByte << 4); // 0001_0000 if timeoutSign = true

            var pauseSignByte = Convert.ToByte(pauseSign);
            var pauseSignMask = (byte)(pauseSignByte << 5); // 0010_0000 if pauseSign = true

            var optimizationByte = Convert.ToByte(optimization);
            var optimizationMask = (byte)(optimizationByte << 3); // 0000_1000 if optimization = true

            var dpsts = (byte)dps; // Name: DataParityStop + timeoutSign = DPSTS

            // Our target: dpsts = YYZX_OYYY
            dpsts |= optimizationMask; // YYYY_OYYY (O - optimization, Y - dps)
            dpsts |= timeoutSignMask; // YYYX_YYYY (X - timeoutSign, Y - dps)
            dpsts |= pauseSignMask; // YYZY_YYYY (Z - pauseSign, Y - dps)

            var cmd = new byte[] { 0x41, 0xC1, 0x00, 0x00, dpsts }; // 
            return cmd;
        }

        private byte[] GetTimeout(ushort timeout)
        {
            var bytes = BitConverter.GetBytes(timeout);
            var cmd = new byte[] { 0x41, 0xC2, 0x00, 0x00, bytes[0], bytes[1] }; // 
            return cmd;
        }

        private byte[] GetPause(ushort pause)
        {
            var bytes = BitConverter.GetBytes(pause);
            var cmd = new byte[] { 0x41, 0xC4, 0x00, 0x00, bytes[0], bytes[1] };
            return cmd;
        }

        private byte[] GetAccessNotifySign(bool accessNotifySign)
        {
            var ans = Convert.ToByte(accessNotifySign);
            var cmd = new byte[] { 0x41, 0x9F, 0x00, 0x00, ans }; // 7f 08 11 41 9f 00 00 01 d3

            return cmd;
        }

        private byte[] GetPauseBeforeResponseRs(ushort pause)
        {
            var bytes = BitConverter.GetBytes(pause);
            var cmd = new byte[] { 0x41, 0xB0, 0x00, 0x00, bytes[0], bytes[1] };//7f 08 b5 41 b0 00 00 10 7a
            return cmd;
        }

        private IEnumerable<byte[]> GetRemoteDevices(IEnumerable<C2000Ethernet> devices)
        {
            if (devices.Count() > 15)
                return Enumerable.Empty<byte[]>();

            var cmd = new List<byte[]>();

            ushort addressOffset = 0x0100;
            foreach (var device in devices)
            {
                var offsetBytes = BitConverter.GetBytes(addressOffset);
                cmd.Add(GetRemoteDeviceIp(device.AddressIP, addressOffset));

                cmd.Add(GetRemoteDeviceUdp(device.DestinationUdp, (ushort)(addressOffset + 0x20)));

                cmd.Add(GetRemoteDeviceUdpType((byte)device.UdpPortType, (ushort)(addressOffset + 0x38)));

                //В версии прошивки V3.15 перестало работать
                //cmd.Add(GetRemoteDeviceMac(device.MACaddress, (ushort)(addressOffset + 0x32)));

                /*
             
                   // После расшифровки алгоритма шифрации раскомментировать
                   var cryptoKeyOffset = (ushort)(addressOffset + 0x22);
                   cmd.Add(GetRemoteDeviceCryptoKey(device.CryptoKey, cryptoKeyOffset));
                */
                addressOffset += 0x40;
            }
            //7f 13 06 41 00 01 00 36 33 2e 36 34 2e 36 35 2e 36 36 00 21
            return cmd;
        }

        private byte[] GetRemoteDeviceIp(string sendStr, ushort addressOffset)
        {
            if (IsNullOrEmpty(sendStr))
                return null;

            var byteArray = Encoding.Default.GetBytes(sendStr);
            var offsetBytes = BitConverter.GetBytes(addressOffset);
            var header = new byte[] { 0x41, offsetBytes[0], offsetBytes[1], 0x00 };
            var stringEnd = new byte[] {  };

            var cmd = CombineArrays(header, byteArray, stringEnd);

            return cmd;
        }

        private IEnumerable<byte[]> GetRemoteDeviceIpByByte(string sendStr, ushort addressOffset)
        {
            if (IsNullOrEmpty(sendStr))
                return Enumerable.Empty<byte[]>();

            var cmd = new List<byte[]>();

            byte[] dot = { 0x2E };
            var ipDigits =  sendStr.Split('.');
            const ushort offset = 0x0004;
            var currentOffset = addressOffset;

            var offsetBytes = BitConverter.GetBytes(currentOffset);
            var headerFirst = new byte[] { 0x41, offsetBytes[0], offsetBytes[1], 0x00 };

            cmd.Add(CombineArrays(headerFirst, Encoding.Default.GetBytes(ipDigits[0]), dot));

            var secondOffset = BitConverter.GetBytes(currentOffset + offset);

            var headerSecond = new byte[] { 0x41, secondOffset[0], secondOffset[1], 0x00 };
            cmd.Add(CombineArrays(headerSecond,
                Encoding.Default.GetBytes(ipDigits[1]), dot,
                Encoding.Default.GetBytes(ipDigits[2]), dot,
                Encoding.Default.GetBytes(ipDigits[3])));

            return cmd;
        }

        private byte[] GetRemoteDeviceUdp(ushort udp, ushort addressOffset)
        {
            if (udp == 0)
                return Array.Empty<byte>();

            var bytes = BitConverter.GetBytes(udp);
            var offsetBytes = BitConverter.GetBytes(addressOffset);
            var header = new byte[] { 0x41, offsetBytes[0], offsetBytes[1], 0x00 };

            var cmd = CombineArrays(header, bytes);

            return cmd;
        }

        private byte[] GetRemoteDeviceUdpType(byte udpType, ushort addressOffset)
        {
            var bytes = BitConverter.GetBytes(udpType);
            var offsetBytes = BitConverter.GetBytes(addressOffset);
            var header = new byte[] { 0x41, offsetBytes[0], offsetBytes[1], 0x00 };

            var cmd = CombineArrays(header, bytes);

            return cmd;
        }

        private byte[] GetRemoteDeviceCryptoKey(string cryptoKey, ushort addressOffset)
        {
            // stub method
            /*
            if (string.IsNullOrEmpty(cryptoKey))
                return true;

            var bytes = Encoding.ASCII.GetBytes(cryptoKey);
            var offsetBytes = BitConverter.GetBytes(addressOffset);
            var header = new byte[] { 0x41, offsetBytes[0], offsetBytes[1], 0x00 };

            var cmd = CombineArrays(header, bytes);
            CommandSend(cmd);*/
            return null;
        }

        private byte[] GetRemoteDeviceMac(string macString, ushort addressOffset)
        {
            //stub method
            //7f 0d e8 41 32 01 00 99 99 78 56 34 12 a1
            //7f 0d b0 41 32 01 00 66 55 44 33 22 11
            var macBytes = NetHelper.GetBytesFromMacAddress(macString).Reverse().ToArray();
            var header = new byte[] { 0x41, 0x32, 0x01, 0x00 };
            var cmd = CombineArrays(header, macBytes);
            return cmd;
        }

        private byte[] GetMasterSlaveUdp(ushort udp)
        {
            var bytes = BitConverter.GetBytes(udp);
            var cmd = new byte[] { 0x41, 0x40, 0x00, 0x00, bytes[0], bytes[1] };
            return cmd;
        }

        private byte[] GetConfirmationTimeout(byte timeout)
        {
            var cmd = new byte[] { 0x41, 0x42, 0x00, 0x00, timeout };
            //7f 08 80 41 42 00 00 ff a2
            return cmd;
        }

        private byte[] GetConnectionTimeout(byte timeout)
        {
            var cmd = new byte[] { 0x41, 0x43, 0x00, 0x00, timeout };
            return cmd;
        }

        private byte[] GetFreeConnectionTune(C2000Ethernet.UdpType udpType, bool allowFreeConnection)
        {
            var freeConnection = Convert.ToByte(allowFreeConnection);// 0000_0001 if allowFreeConnection = true

            var udpPortType = Convert.ToByte(udpType);
            freeConnection |= (byte)(udpPortType << 1); // 0000_0010 if udpType = static

            var cmd = new byte[] { 0x41, 0x56, 0x00, 0x00, freeConnection };
            return cmd;
        }

        private byte[] GetFreeConnectionUdp(ushort udp)
        {
            var bytes = BitConverter.GetBytes(udp);
            // 7f 09 26 41 44 00 00 39 30 9a
            var cmd = new byte[] { 0x41, 0x44, 0x00, 0x00, bytes[0], bytes[1] };
            return cmd;
        }

        private byte[] GetTransparentTune(ushort udp, C2000Ethernet.TransparentProtocolType protocol, bool crypto)
        {
            var transparentMode = Convert.ToByte(crypto);

            transparentMode |= (byte)((byte)protocol << 1);

            var bytes = BitConverter.GetBytes(udp);
            //7f 0a 62 41 d0 00 00 67 2b 02 57
            var cmd = new byte[] { 0x41, 0xd0, 0x00, 0x00, bytes[0], bytes[1], transparentMode };

            return cmd;
        }

        private byte[] ReadDeviceNetworkSettings()
        {
            return new byte[] { 0x43, 0x00, 0x00, 0x00, 0x40 }; // 7f 08 6e 43 00 00 00 40
        }

        private byte[] GetSecondPowerControl()
        {
            var cmd = new byte[] { 0x41, 0xF0, 0x00, 0x00, 0x00 };// 7f 08 3a 41 f0 00 00 01
            return cmd;
        }
    }
}
