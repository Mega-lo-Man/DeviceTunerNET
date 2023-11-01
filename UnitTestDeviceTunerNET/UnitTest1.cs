using DeviceTunerNET.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DeviceTunerNET.SharedDataModel.Devices;
using DeviceTunerNET.SharedDataModel.Ports;
using System.Linq;
using System.Net;
using static DeviceTunerNET.SharedDataModel.ElectricModules.Shleif;
using System.IO.Ports;
using Prism.Events;

namespace UnitTestDeviceTunerNET
{
    [TestClass]
    public class UnitTest1
    {
        private byte newAddress = 0x02;
        const string comPort = "COM4";
        const uint deviceAddress = 127;
        /*
        [TestMethod]
        public void TestTryParsePortSwitchResponse()
        {
            const string respornse = @"-------- ------------ ------  ----- -------- ---- ----------- ------------- -------- ------- ------------------------
gi1/0/1  1G-Copper      --      --     --     --  Down (nc)         --         --     --     Access (1)
gi1/0/2  1G-Copper      --      --     --     --  Down (nc)         --         --     --     Access (1)
gi1/0/3  1G-Copper      --      --     --     --  Down (nc)         --         --     --     Access (1)
gi1/0/4  1G-Copper      --      --     --     --  Down (nc)         --         --     --     Access (1)
gi1/0/5  1G-Copper      --      --     --     --  Down (nc)         --         --     --     Access (1)
gi1/0/6  1G-Copper      --      --     --     --  Down (nc)         --         --     --     Access (1)
gi1/0/7  1G-Copper      --      --     --     --  Down (nc)         --         --     --     Access (1)
gi1/0/8  1G-Copper      --      --     --     --  Down (nc)         --         --     --     Access (1)
gi1/0/9  1G-Combo-C   Full    1000  Enabled  Off  Up          00,03:42:45   Disabled On      Trunk
gi1/0/10 1G-Combo-C     --      --     --     --  Down (nc)         --         --     --     Access (1)
-------- ------- ------  -----  -------- -------  ------------- ------------------------
";

            
        }

        [TestMethod]
        public void TestTryParseFirstStringOfSwitchResponse()
        {
            const string respornse = @"-------- ------------ ------  ----- -------- ---- ----------- ------------- -------- ------- ------------------------"; 
            var portFactory = new PortFactory();

            var result = portFactory.GetColumnsShifting(respornse);

            Assert.AreEqual(11, result.Count());
        }
        */



        [TestMethod]
        public void TestIsDeviceOnline()
        {
            
            
            var serialPort = new SerialPort {PortName = comPort};
            var testService = new SerialSender(new EventAggregator());
            var result = testService.IsDeviceOnline(serialPort, 127);

            Assert.IsTrue(result);
            //Assert.AreEqual(result, "С2000-Ethernet");
        }

        [TestMethod]
        public void TestIsDeviceOnlineUdp()
        {
            var ip = IPAddress.Parse("10.10.10.1");

            var port = new BolidUdpClient(12000)
            {
                RemoteServerIp = ip,
                RemoteServerUdpPort = 12000,
                Timeout = 100,
            };

            var device = new Signal20P(port);

            var result = device.GetModelCode(127, out var deviceCode).ToString();

            
            Assert.AreEqual(deviceCode, device.ModelCode);
        }

        [TestMethod]
        public void TestSetDeviceAddress()
        {
            
            
            var serialPort = new SerialPort { PortName = comPort };
            var testDevice = new Signal20P(new ComPort() { SerialPort = serialPort })
            {
                AddressRS485 = deviceAddress,
            };
            serialPort.Open();

            var result = testDevice.SetAddress();

            serialPort.Close();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestAdcShleif()
        {
            

            var serialPort = new SerialPort { PortName = comPort };
            var testDevice = new Signal20P(new ComPort() { SerialPort = serialPort })
            {
                AddressRS485 = deviceAddress,
            };
            serialPort.Open();

            var result = testDevice.Shleifs.ElementAt(0).GetShleifAdcValue();

            serialPort.Close();

            Assert.AreEqual(0x00, (byte)result);
        }

        [TestMethod]
        public void TestStateShleif()
        {
            
            var serialPort = new SerialPort { PortName = comPort };
            
            var testDevice = new Signal20P(new ComPort() { SerialPort = serialPort })
            {
                AddressRS485 = deviceAddress,
            };
            serialPort.Open();

            var result = testDevice.Shleifs.ElementAt(0).GetShleifState();

            serialPort.Close();

            Assert.AreEqual(States.RemovedGuard, result);
        }

        [TestMethod]
        public void TestRelayOn()
        {
           

            var serialPort = new SerialPort { PortName = comPort };
            var testDevice = new Signal20P(new ComPort() { SerialPort = serialPort })
            {
                AddressRS485 = deviceAddress,
            };
            serialPort.Open();

            var result = testDevice.Relays.ElementAt(1).TurnOn();

            serialPort.Close();

            Assert.IsTrue(result);
        }


        [TestMethod]
        public void TestRelayOff()
        {
           

            var serialPort = new SerialPort { PortName = comPort };
            var testDevice = new Signal20P(new ComPort() { SerialPort = serialPort })
            {
                AddressRS485 = deviceAddress,
            };
            serialPort.Open();

            var result = testDevice.Relays.ElementAt(1).TurnOff();

            serialPort.Close();

            Assert.IsTrue(result);
        }
        
        [TestMethod]
        public void TestSendC2000EthernetConfig()
        {

            var serialPort = new SerialPort { PortName = comPort };
            var masterDevice1 = new C2000Ethernet(new ComPort() { SerialPort = serialPort })
            {
                AddressIP = "192.168.2.12",
                NetworkMode = C2000Ethernet.Mode.master,
                NetName = "MASTER1",
                AddressRS485 = 5,
                MACaddress = "AA:BB:CC:DD:EE:FF"
            };
            var masterDevice2 = new C2000Ethernet(new ComPort() { SerialPort = serialPort })
            {
                AddressIP = "192.168.2.13",
                NetworkMode = C2000Ethernet.Mode.master,
                NetName = "МАСТЕР2",
                AddressRS485 = 6,
                MACaddress = "AA:BB:CC:DD:EE:FE"
            };
            var masterDevice3 = new C2000Ethernet(new ComPort() { SerialPort = serialPort })
            {
                AddressIP = "192.168.2.14",
                NetworkMode = C2000Ethernet.Mode.master,
                NetName = "МАСТЕР2",
                AddressRS485 = 6,
                MACaddress = "AA:BB:CC:DD:EE:FD"
            };
            var masterDevice4 = new C2000Ethernet(new ComPort() { SerialPort = serialPort })
            {
                AddressIP = "192.168.2.15",
                NetworkMode = C2000Ethernet.Mode.master,
                NetName = "МАСТЕР3",
                AddressRS485 = 6,
                MACaddress = "AA:BB:CC:DD:EE:FC"
            };
            var masterDevice5 = new C2000Ethernet(new ComPort() { SerialPort = serialPort })
            {
                AddressIP = "192.168.2.16",
                NetworkMode = C2000Ethernet.Mode.master,
                NetName = "МАСТЕР3",
                AddressRS485 = 6,
                MACaddress = "AA:BB:CC:DD:EE:FB"
            };
            var slaveDevice = new C2000Ethernet(new ComPort() { SerialPort = serialPort })
            {
                AddressIP = "192.168.2.33",
                NetName = "РАБ",
                NetworkMode = C2000Ethernet.Mode.slave,
                RemoteDevicesList = new System.Collections.Generic.List<C2000Ethernet> 
                { 
                    masterDevice1, 
                    masterDevice2, 
                    masterDevice3, 
                    masterDevice4, 
                    masterDevice5, 
                    masterDevice1, 
                    masterDevice2, 
                    masterDevice3, 
                    masterDevice4, 
                    masterDevice5, 
                    masterDevice1, 
                    masterDevice2, 
                    masterDevice3, 
                    masterDevice4, 
                    masterDevice5/**/
                },
                AddressRS485= 127,
                MACaddress = "AA-BB-CC-DD-EE-A1"
            };


            serialPort.Open();
            var target = 1;
            slaveDevice.WriteConfig(Progress);
            serialPort.Close();
            //Assert.IsTrue(result);
        }

        private void Progress(int progress)
        {

        }
        
    }
}
