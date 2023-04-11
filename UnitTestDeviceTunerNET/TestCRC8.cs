using DeviceTunerNET.SharedDataModel.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MS.WindowsAPICodePack.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestDeviceTunerNET
{
    [TestClass]
    public class TestCRC8
    {
        [TestMethod]
        public void TestCheckOrionCRC8()
        {
            var data = new List<byte[]>()
            { 
                new byte[] { 0x7F, 0x05, 0x10, 0x04, 0x04, 0x93 },
                new byte[] { 0x02, 0x05, 0x02, 0x24, 0x00, 0xb7 },
                new byte[] { 0x04, 0x05, 0x10, 0x7F, 0x7F, 0xC3 },               
            };

            for (var i = 0; i < data.Count; i++)
            {
                var result = OrionCRC.IsCrcValid(data[i]);
                Assert.IsTrue(result);
            }

        }
    }
}
