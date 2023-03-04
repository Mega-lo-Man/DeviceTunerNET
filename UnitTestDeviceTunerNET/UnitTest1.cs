using DeviceTunerNET.Services;
using DeviceTunerNET.SharedDataModel.Devices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prism.Events;
using System.IO.Ports;
using System.Linq;
using static DeviceTunerNET.SharedDataModel.ElectricModules.Shleif;

namespace UnitTestDeviceTunerNET
{
    [TestClass]
    public class UnitTest1
    {
        private byte newAddress = 0x02;
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
            const string comPort = "COM3";
            
            var serialPort = new SerialPort {PortName = comPort};
            var testService = new SerialSender(new EventAggregator());
            var result = testService.IsDeviceOnline(serialPort, 127);

            Assert.IsTrue(result);
            //Assert.AreEqual(result, "С2000-Ethernet");
        }

        [TestMethod]
        public void TestSetDeviceAddress()
        {
            const string comPort = "COM3";
            
            var serialPort = new SerialPort { PortName = comPort };
            var testDevice = new Signal20P
            {
                AddressRS485 = 3,
                Port = serialPort
            };
            testDevice.Port.Open();

            var result = testDevice.SetAddress();

            testDevice.Port.Close();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestAdcShleif()
        {
            const string comPort = "COM3";

            var serialPort = new SerialPort { PortName = comPort };
            var testDevice = new Signal20P
            {
                AddressRS485 = 3,
                Port = serialPort
            };
            testDevice.Port.Open();

            testDevice.Port = serialPort;
            var result = testDevice.Shleifs.ElementAt(0).GetShleifAdcValue();

            testDevice.Port.Close();

            Assert.AreEqual(0xff, (byte)result);
        }

        [TestMethod]
        public void TestStateShleif()
        {
            const string comPort = "COM3";

            var serialPort = new SerialPort { PortName = comPort };
            var testDevice = new Signal20P
            {
                AddressRS485 = 3,
                Port = serialPort
            };
            testDevice.Port.Open();
            testDevice.Port = serialPort;

            var result = testDevice.Shleifs.ElementAt(0).GetShleifState();

            testDevice.Port.Close();

            Assert.AreEqual(States.RemovedGuard, result);
        }

        [TestMethod]
        public void TestRelayOn()
        {
            const string comPort = "COM3";

            var serialPort = new SerialPort { PortName = comPort };
            var testDevice = new Signal20P
            {
                AddressRS485 = 3,
                Port = serialPort
            };
            testDevice.Port.Open();
            testDevice.Port = serialPort;

            var result = testDevice.Relays.ElementAt(1).TurnOn();

            testDevice.Port.Close();

            Assert.IsTrue(result);
        }


        [TestMethod]
        public void TestRelayOff()
        {
            const string comPort = "COM3";

            var serialPort = new SerialPort { PortName = comPort };
            var testDevice = new Signal20P
            {
                AddressRS485 = 3,
                Port = serialPort
            };
            testDevice.Port.Open();
            testDevice.Port = serialPort;

            var result = testDevice.Relays.ElementAt(1).TurnOff();

            testDevice.Port.Close();

            Assert.IsTrue(result);
        }
        
        [TestMethod]
        public void TestSendC2000EthernetConfig()
        {
            const string comPort = "COM3";
            var masterDevice = new C2000Ethernet()
            {
                AddressIP = "192.168.2.22",
                NetworkMode = C2000Ethernet.Mode.master,
                NetName = "MASTER",
                AddressRS485 = 5,
                MACaddress = "AA:BB:CC:DD:EE:FF"
            };
            var slaveDevice = new C2000Ethernet()
            {
                AddressIP = "192.168.2.33",
                NetName = "SLAVE",
                NetworkMode = C2000Ethernet.Mode.slave,
                RemoteDevicesList = new System.Collections.Generic.List<C2000Ethernet> { masterDevice },
                AddressRS485= 127,
                MACaddress = "AA-BB-CC-DD-EE-FF"
            };

            var serialPort = new SerialPort {PortName = comPort};
            serialPort.Open();
            var target = 1;
            slaveDevice.WriteConfig(serialPort, null);
            serialPort.Close();
            //Assert.IsTrue(result);
        }
        
    }
}
