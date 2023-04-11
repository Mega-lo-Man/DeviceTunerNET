using DeviceTunerNET.SharedDataModel.Devices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeviceTunerNET.SharedDataModel.Ports;
using System.Web.Services.Description;

namespace UnitTestDeviceTunerNET
{
    [TestClass]
    public class TestSignal20P
    {
        [TestMethod]
        public void TestGetConfig()
        {
            var device = new Signal20P(new ComPort() { SerialPort = null })
            {
                AddressRS485 = 127
            };

            var config = device.GetConfig();
            Assert.IsNotNull(config);
        }

        [TestMethod]
        public void TestWriteConfig()
        {
            var port = new SerialPort
            {
                PortName = "COM3"
            };

            port.Open();

            var device = new Signal20P(new ComPort() { SerialPort = port } )
            {
                AddressRS485 = 127,
            };

            

            device.Shleifs.ElementAt(19).RelayControlActivations.Add(device.SupervisedRelays.ElementAt(0), 20);

            device.WriteConfig(Progress);
            port.Close();
            
            //Assert.IsNotNull(config);
        }

        [TestMethod]
        public void TestBaseWriteConfig()
        {
            var port = new SerialPort
            {
                PortName = "COM3"
            };

            port.Open();

            var device = new Signal20P(new ComPort() { SerialPort = port })
            {
                AddressRS485 = 127
            };

            device.WriteBaseConfig(Progress);
            port.Close();

            //Assert.IsNotNull(config);
        }

        private void Progress(int progress)
        {
            Console.Write(progress + ", ");
        }
    }
}
