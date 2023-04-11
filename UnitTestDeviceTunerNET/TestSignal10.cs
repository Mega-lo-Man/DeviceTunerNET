using DeviceTunerNET.SharedDataModel.Devices;
using DeviceTunerNET.SharedDataModel.ElectricModules;
using DeviceTunerNET.SharedDataModel.Ports;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestDeviceTunerNET
{
    [TestClass]
    public class TestSignal10
    {
        [TestMethod]
        public void TestCheckDeviceModel()
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
            var result = device.GetModelCode(4);
            port.Close();

            Assert.AreEqual(34, result);
        }

        [TestMethod]
        public void TestBaseWriteConfig()
        {
            var port = new SerialPort
            {
                PortName = "COM3"
            };

            port.Open();

            var device = new Signal_10(new ComPort() { SerialPort = port })
            {
                AddressRS485 = 127
            };

            device.WriteBaseConfig(Progress);
            port.Close();

            //Assert.IsNotNull(config);
        }

        [TestMethod]
        public void TestWriteConfig()
        {
            var port = new SerialPort
            {
                PortName = "COM3"
            };

            port.Open();

            var device = new Signal_10(new ComPort() { SerialPort = port })
            {
                AddressRS485 = 127
            };

            for(byte i = 0; i < 10; i++)
            {
                device.Shleifs.ElementAt(i).InputType = Shleif.InputTypes.Intrusion;
                device.Shleifs.ElementAt(i).AlarmDelay = i;
                device.Shleifs.ElementAt(i).ArmingDelay = (byte)(i + 10);
                device.Shleifs.ElementAt(i).InputAnalysisDelayAfterReset = (byte)(i + 20);
                var relayControlDict = new Dictionary<IRelay, byte>
                {
                    { device.Relays.ElementAt(0), 0x10 }
                };
                device.Shleifs.ElementAt(i).RelayControlActivations = relayControlDict;
            }
            for (var i = 0; i < 2; i++)
            {
                device.Relays.ElementAt(i).ControlProgram = OptoRelay.ControlPrograms.NoControl;
                device.Relays.ElementAt(i).ControlTime = 0x1FF;
                device.Relays.ElementAt(i).Events = true;
            }

            for (var i = 0; i < 2; i++)
            {
                device.SupervisedRelays.ElementAt(i).ControlProgram = OptoRelay.ControlPrograms.NoControl;
                device.SupervisedRelays.ElementAt(i).ControlTime = 0xFFF;
                device.SupervisedRelays.ElementAt(i).Events = false;
                device.SupervisedRelays.ElementAt(i).Monitoring = SupervisedRelay.MonitorFor.OpenFaults;
            }

            device.WriteConfig(Progress);
            port.Close();

            //Assert.IsNotNull(config);
        }

        private void Progress(int progress)
        {
            Console.Write(progress + ", ");
        }
    }
}
