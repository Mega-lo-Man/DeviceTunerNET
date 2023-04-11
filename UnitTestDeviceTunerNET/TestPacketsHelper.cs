using DeviceTunerNET.SharedDataModel.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace UnitTestDeviceTunerNET
{
    [TestClass]
    public class TestPacketsHelper
    {
        [TestMethod]
        public void TestGetPackets()
        {
            // Arrange
            var testData = new byte[] { 0x7F, 0x05, 0x10, 0x04, 0x04, 0x93, 0x04, 0x05, 0x10, 0x04, 0x04, 0x8E };
            var expected = new List<byte[]>
            {
                new byte[] { 0x7F, 0x05, 0x10, 0x04, 0x04, 0x93 },
                new byte[] { 0x04, 0x05, 0x10, 0x04, 0x04, 0x8E },
            };

            // Act
            var actual = PacketsHelper.GetPackets(testData).ToList();

            // Assert
            for (int i = 0; i < expected.Count; i++)
            {
                CollectionAssert.AreEqual(expected[i], actual[i]);
            }
        }
    }
}
