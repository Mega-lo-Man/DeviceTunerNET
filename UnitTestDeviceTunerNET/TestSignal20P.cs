using DeviceTunerNET.SharedDataModel.Devices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestDeviceTunerNET
{
    [TestClass]
    public class TestSignal20P
    {
        [TestMethod]
        public void TestGetConfig()
        {
            var device = new Signal20P
            {
                AddressRS485 = 127
            };

            var config = device.GetConfig();
            Assert.IsNotNull(config);
        }

        [TestMethod]
        public void TestWriteConfig()
        {
            var ComPort = new SerialPort();
            ComPort.PortName = "COM3";

            ComPort.Open();

            var device = new Signal20P
            {
                AddressRS485 = 127
            };

            device.WriteConfig(ComPort, null);
            ComPort.Close();
            
            //Assert.IsNotNull(config);
        }
    }
}
