using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using DeviceTunerNET.Services;
using DeviceTunerNET.SharedDataModel;
using Prism.Events;

namespace UnitTestDeviceTunerNET
{
    [TestClass]
    public class UnitTest1
    {
        private byte newAddress = 0x02;

        [TestMethod]
        public void TestIsDeviceOnline()
        {
            const string comPort = "COM6";
            
            var serialPort = new SerialPort {PortName = comPort};
            var testService = new SerialSender(new EventAggregator());
            var result = testService.IsDeviceOnline(serialPort, 127);

            Assert.IsTrue(result);
            //Assert.AreEqual(result, "С2000-Ethernet");
        }

        [TestMethod]
        public void TestChangeDeviceAddress()
        {
            const string comPort = "COM6";
            
            var serialPort = new SerialPort { PortName = comPort };
            var testService = new SerialSender(new EventAggregator());

            var result = testService.SetDeviceRS485Address(serialPort, 127, newAddress);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestSendC2000EthernetConfig()
        {
            const string comPort = "COM6";
            var testDevice = new C2000Ethernet
            {
                AddressRS485 = 2,
                NetName = "DEADBEAF",
                AddressIP = "1.2.3.4", 
                Netmask = "5.6.7.8",
                DefaultGateway = "7.8.9.10",
                Dhcp = true, 
                FirstDns = "11.12.13.14", 
                SecondDns = "15.16.17.18",
                NetworkMode = C2000Ethernet.Mode.master,
                InterfaceType = C2000Ethernet.ProtocolType.RS485,
                ConnectionSpeed = C2000Ethernet.Speed.b9600,
                FrameFormat = C2000Ethernet.DataParityStop.data9Stop2,
                TimeoutSign = false,
                Timeout = 65534,
                PauseSign = true,
                Pause = 65533,
                Optimization = false,
                AccessNotifySign = true,
                PauseBeforeResponseRs = 0x10,
                RemoteDevicesList = new List<C2000Ethernet>()
                {
                    new C2000Ethernet()
                    {
                        AddressIP = "192.1.1.1",
                        DestinationUdp = 54789,
                        UdpPortType = C2000Ethernet.UdpType.dynamicUdp,
                        CryptoKey = System.Text.Encoding.Default.GetString(new byte[]{0x3c, 0x3e, 0x15, 0xff, 0x8c, 0x89, 0x94, 0xd3, 0xaa, 0x9e, 0x9c, 0x2e, 0xcc, 0xdd, 0x2a, 0x06})
                    },
                    new C2000Ethernet()
                    {
                        AddressIP = "192.1.2.2",
                        DestinationUdp = 54789
                    },
                    new C2000Ethernet()
                    {
                        AddressIP = "192.1.3.3",
                        DestinationUdp = 54789,
                        UdpPortType = C2000Ethernet.UdpType.dynamicUdp
                    },
                    new C2000Ethernet()
                    {
                        AddressIP = "192.168.3.4",
                        DestinationUdp = 54789
                    },
                    new C2000Ethernet()
                    {
                        AddressIP = "192.168.3.5",
                        DestinationUdp = 54789,
                        UdpPortType = C2000Ethernet.UdpType.dynamicUdp
                    },
                    new C2000Ethernet()
                    {
                        AddressIP = "192.168.3.6",
                        DestinationUdp = 54789
                    },
                    new C2000Ethernet()
                    {
                        AddressIP = "192.168.3.7",
                        DestinationUdp = 54789,
                        UdpPortType = C2000Ethernet.UdpType.dynamicUdp
                    },
                    new C2000Ethernet()
                    {
                        AddressIP = "192.168.3.8",
                        DestinationUdp = 54789
                    },
                    new C2000Ethernet()
                    {
                        AddressIP = "192.168.3.9",
                        DestinationUdp = 54789,
                        UdpPortType = C2000Ethernet.UdpType.dynamicUdp
                    },
                    new C2000Ethernet()
                    {
                        AddressIP = "192.168.3.10",
                        DestinationUdp = 54789
                    },
                    new C2000Ethernet()
                    {
                        AddressIP = "192.168.3.11",
                        DestinationUdp = 54789
                    },
                    new C2000Ethernet()
                    {
                        AddressIP = "192.168.3.12",
                        DestinationUdp = 54789
                    },
                    new C2000Ethernet()
                    {
                        AddressIP = "192.168.3.13",
                        DestinationUdp = 54789
                    },
                    new C2000Ethernet()
                    {
                        AddressIP = "192.168.3.14",
                        DestinationUdp = 54789
                    },
                    new C2000Ethernet()
                    {
                        AddressIP = "192.168.3.15",
                        DestinationUdp = 54789,
                        UdpPortType = C2000Ethernet.UdpType.dynamicUdp
                    }
                },
                MasterSlaveUdp = 12345,
                ConfirmationTimeout = 0xFD,
                ConnectionTimeout = 0xFE,

                FreeConnectionUdpType = C2000Ethernet.UdpType.staticUdp,
                AllowFreeConnection = false,

                TransparentUdp = 5554,
                TransparentProtocol = C2000Ethernet.TransparentProtocolType.S2000,
                TransparentCrypto = true
            };

            var serialPort = new SerialPort {PortName = comPort};
            var testService = new SerialSender(new EventAggregator());
            var result = testService.SetC2000EthernetConfig(serialPort, newAddress, testDevice);

            Assert.IsTrue(result);
        }

    }
}
