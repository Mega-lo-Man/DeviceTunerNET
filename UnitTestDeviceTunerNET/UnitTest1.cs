using DeviceTunerNET.Services;
using DeviceTunerNET.SharedDataModel.Devices;
using DeviceTunerNET.SharedDataModel.Ports;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prism.Events;
using System.IO.Ports;
using System.Linq;
using System.Web.Services.Description;
using static DeviceTunerNET.SharedDataModel.ElectricModules.Shleif;

namespace UnitTestDeviceTunerNET
{
    [TestClass]
    public class UnitTest1
    {
        private byte newAddress = 0x02;
        const string comPort = "COM3";
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
            var masterDevice = new C2000Ethernet(new ComPort() { SerialPort = serialPort })
            {
                AddressIP = "192.168.2.22",
                NetworkMode = C2000Ethernet.Mode.master,
                NetName = "MASTER",
                AddressRS485 = 5,
                MACaddress = "AA:BB:CC:DD:EE:FF"
            };
            var slaveDevice = new C2000Ethernet(new ComPort() { SerialPort = serialPort })
            {
                AddressIP = "192.168.2.33",
                NetName = "SLAVE",
                NetworkMode = C2000Ethernet.Mode.slave,
                RemoteDevicesList = new System.Collections.Generic.List<C2000Ethernet> { masterDevice },
                AddressRS485= 127,
                MACaddress = "AA-BB-CC-DD-EE-FF"
            };

            serialPort.Open();
            var target = 1;
            slaveDevice.WriteConfig(null);
            serialPort.Close();
            //Assert.IsTrue(result);
        }

        private void Progress(int progress)
        {

        }
        
    }
}
