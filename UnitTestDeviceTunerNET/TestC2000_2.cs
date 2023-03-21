using DeviceTunerNET.SharedDataModel.Devices;
using DeviceTunerNET.SharedDataModel.Ports;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DeviceTunerNET.SharedDataModel.ElectricModules.Shleif;
using System.Web.Services.Description;

namespace UnitTestDeviceTunerNET
{
    [TestClass]
    public class TestC2000_2
    {

        private const string portName = "COM3";
        private readonly uint deviceAddress = 127;

        [TestMethod]
        public void TestShleifAdc()
        {
            var port = new ComPort
            {
                SerialPort = new SerialPort()
                {
                    PortName = portName,
                }
            };
            port.SerialPort.Open();

            var testDevice = new C2000_2(port)
            {
                AddressRS485 = deviceAddress
            };

            var result = testDevice.Shleifs.ElementAt(0).GetShleifAdcValue();

            port.SerialPort.Close();

            Assert.AreEqual(0x00, (byte)result);
        }

        [TestMethod]
        public void TestStateShleif()
        {

            var serialPort = new SerialPort { PortName = portName };

            var testDevice = new Signal20P(new ComPort() { SerialPort = serialPort })
            {
                AddressRS485 = deviceAddress,
            };
            serialPort.Open();

            var result = testDevice.Shleifs.ElementAt(0).GetShleifState();

            serialPort.Close();

            Assert.AreEqual(States.RemovedGuard, result);
        }
    }
}
