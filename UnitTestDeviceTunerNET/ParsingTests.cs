using DeviceTunerNET.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DeviceTunerNET.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestDeviceTunerNET
{
    [TestClass]
    public class ParsingTests
    {
        private Dictionary<string, string> variables = new Dictionary<string, string>()
            {
                { "%%HOST_NAME%%", "SW_1" },
                { "%%DEFAULT_ADDRESS%%", "192.168.1.239" },
                { "%%NEW_ADDRESS%%", "192.168.3.11" },
                { "%%DEFAULT_ADMIN%%", "admin" },
                { "%%DEFAULT_ADMINPASSWORD%%", "admin" },
                { "%%NEW_ADMIN%%", "admin" },
                { "%%NEW_ADMINPASSWORD%%", "admin123" }

            };

        private const string templateConfig = @"C:\Temp\TempConfig.txt";
        private const string outputConfig = @"C:\Temp\OutputConfig.txt";

        [TestMethod]
        public void TestConfigFileExists()
        {
            

            var parser = new ConfigParser();

            Assert.AreEqual(IConfigParser.Errors.FileNotFound, parser.Parse(variables, templateConfig, outputConfig));
        }

        [TestMethod]
        public void TestConfigParse()
        {

            var parser = new ConfigParser();

            Assert.AreEqual(IConfigParser.Errors.Ok, parser.Parse(variables, templateConfig, outputConfig));
        }
    }
}
