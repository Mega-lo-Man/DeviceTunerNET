using DeviceTunerNET.SharedDataModel.Devices;
using DeviceTunerNET.SharedDataModel.Ports;
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
    public class TestOrionDevicesCommon
    {
        [TestMethod]
        public void TestIsDeviceOnline()
        {
            var port = new SerialPort
            {
                PortName = "COM3"
            };

            port.Open();

            var device = new OrionDevice(new ComPort() { SerialPort = port })
            {
                AddressRS485 = 4
            };
            var result = device.IsDeviceOnline();
            port.Close();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestIsChangeAddress()
        {
            var port = new SerialPort
            {
                PortName = "COM3"
            };

            port.Open();

            var device = new OrionDevice(new ComPort() { SerialPort = port })
            {
                AddressRS485 = 127
            };
            var result = device.ChangeDeviceAddress(4);


            port.Close();

            Assert.IsTrue(result);
        }
    }
}
