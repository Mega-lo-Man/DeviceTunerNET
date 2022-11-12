using DeviceTunerNET.Services;
using DeviceTunerNET.SharedDataModel.Devices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prism.Events;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Serialization;
using static DeviceTunerNET.SharedDataModel.ElectricModules.Shleif;

namespace UnitTestDeviceTunerNET
{
    [TestClass]
    public class UnitTest1
    {
        private byte newAddress = 0x02;

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
                ComPort = serialPort
            };
            testDevice.ComPort.Open();

            var result = testDevice.SetAddress();

            testDevice.ComPort.Close();

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
                ComPort = serialPort
            };
            testDevice.ComPort.Open();

            testDevice.ComPort = serialPort;
            var result = testDevice.Shleifs.ElementAt(0).GetShleifAdcValue();

            testDevice.ComPort.Close();

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
                ComPort = serialPort
            };
            testDevice.ComPort.Open();
            testDevice.ComPort = serialPort;

            var result = testDevice.Shleifs.ElementAt(0).GetShleifState();

            testDevice.ComPort.Close();

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
                ComPort = serialPort
            };
            testDevice.ComPort.Open();
            testDevice.ComPort = serialPort;

            var result = testDevice.Relays.ElementAt(1).TurnOn();

            testDevice.ComPort.Close();

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
                ComPort = serialPort
            };
            testDevice.ComPort.Open();
            testDevice.ComPort = serialPort;

            var result = testDevice.Relays.ElementAt(1).TurnOff();

            testDevice.ComPort.Close();

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
            var result = slaveDevice.WriteConfig(serialPort, null);
            serialPort.Close();
            Assert.IsTrue(result);
        }
        
    }
}
