using DeviceTunerNET.Services;
using DeviceTunerNET.SharedDataModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTestDeviceTunerNET
{
    [TestClass]
    public class UploadManagerTests
    {
        [TestMethod]
        public void TestUploadingConfig()
        {
            var ethernetSwitch = new EthernetSwitch(null);

            var manager = new UploadSwitchManager(new ConfigParser(), new TftpServerManager());
            var token = new CancellationToken();

            //Assert.AreEqual(true, manager.UploadConfig(ethernetSwitch, token));
        }
    }
}
