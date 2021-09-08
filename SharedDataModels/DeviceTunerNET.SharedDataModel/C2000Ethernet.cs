using System;
using System.Collections.Generic;

namespace DeviceTunerNET.SharedDataModel
{
    public class C2000Ethernet : RS232device
    {
        private enum _duplex { half = 0, full = 1 }
        private enum _mode { transparent = 0, master = 1, slave = 2 }

        private string _remoteDefaultFirstIP = "192.168.2.1";

        private List<byte[]> _configLineList;

        #region Properties

        public string NetName { get; set; } = "ABCDEFG";

        public string FirstDns { get; set; } = "0.0.0.0";

        public string SecondDns { get; set; } = "0.0.0.0";

        private _mode _networkMode;
        public int NetworkMode
        {
            get => (int)_networkMode;
            set => _networkMode = (_mode)value;
        }

        public List<int> RemoteUDPList { get; set; } = new List<int>();

        public List<C2000Ethernet> ListOfRemoteDevices { get; set; } = new List<C2000Ethernet>();

        public string RemoteIpTrasparentMode { get; set; }

        public int DuplexMode { get; set; }

        public int UDPSender { get; set; }

        public int UDPRemote { get; set; }

        public int FreeUDPRemote { get; set; }

        public int WaitingTimeout { get; set; }

        public bool UseSingleReadWriteUDP { get; set; }

        #endregion Properties

        public C2000Ethernet()
        {
            //------------------------------------------------
            for (var i = 0; i < 9; i++)
            {
                RemoteUDPList.Add(40000);
            }
            //------------------------------------------------
            AddressIP = "192.168.2.11";
            Netmask = "255.255.252.0";
            DefaultGateway = "0.0.0.0";
            Netmask = "255.255.252.0";
            DuplexMode = (int)_duplex.half;
            UDPSender = 40000;
            UDPRemote = 40001;
            FreeUDPRemote = 40001;
            UseSingleReadWriteUDP = true;
            WaitingTimeout = 80;
            AddressRS232 = 127;
            AddressRS485 = 127;
            //------------------------------------------------
            _configLineList = new List<byte[]>();
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
            SendNetmaskAndDevicesUDP(addr, Netmask, RemoteUDPList, UDPSender, FreeUDPRemote);

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
