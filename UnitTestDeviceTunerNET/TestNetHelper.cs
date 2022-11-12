using DeviceTunerNET.Services;
using DeviceTunerNET.SharedDataModel.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestDeviceTunerNET
{
    [TestClass]
    public class TestNetHelper
    {
        [TestMethod]
        public void TestGetBytesFromMac()
        {
            const string macColon = "01:02:03:AA:BB:F3";
            const string macDash = "01-02-03-AA-BB-F3";
            const string macEmpty = "";

            var expectedArray1 = new byte[6] { 0x01, 0x02, 0x03, 0xAA, 0xBB, 0xF3 };

            var result1 = NetHelper.GetBytesFromMacAddress(macColon);
            CollectionAssert.AreEqual(expectedArray1, result1);

            var result2 = NetHelper.GetBytesFromMacAddress(macDash);
            CollectionAssert.AreEqual(expectedArray1, result2);

            var expectedArray2 = new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            var result3 = NetHelper.GetBytesFromMacAddress(macEmpty);
            CollectionAssert.AreEqual(expectedArray2, result3);


        }
    }
}
