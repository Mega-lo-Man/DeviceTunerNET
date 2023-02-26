using DeviceTunerNET.Services;
using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using DeviceTunerNET.SharedDataModel.Devices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestDeviceTunerNET
{
    [TestClass]
    public  class TestDeviceGenerator
    {
        [TestMethod]
        public void TestGetDevice()
        {

            var testDeviceName = "Сигнал-20П";

            var generator = new DeviceGenerator();

            generator.TryGetDevice(testDeviceName, out var obj);

            Assert.AreEqual(obj.GetType(), typeof(Signal20P));
        }
    }
}
