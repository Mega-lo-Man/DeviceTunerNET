using DeviceTunerNET.SharedDataModel.ElectricModules;
using DeviceTunerNET.SharedDataModel.ElectricModules.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DeviceTunerNET.SharedDataModel.Devices.IOrionNetTimeouts;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000_2 : OrionDevice, IShleifs, IAccess
    {
        private readonly int inputsCount = 2;

        public new const int Code = 16;

        #region Properties
        
        public IEnumerable<Shleif> Shleifs { get; set; }
        public AccessController Access { get; set; }

        /// <summary>
        /// Выключать при открытии двери 1
        /// </summary>
        public bool TurnOffWhenOpening1 { get; set; }
        /// <summary>
        /// Выключать при открытии двери 2
        /// </summary>
        public bool TurnOffWhenOpening2 { get; set; }
        /// <summary>
        /// Выключать при закрытии двери 1
        /// </summary>
        public bool TurnOffWhenClosing1 { get; set; }
        /// <summary>
        /// Выключать при закрытии двери 2
        /// </summary>
        public bool TurnOffWhenClosing2 { get; set; }
        /// <summary>
        /// События о включении/выключении реле 1
        /// </summary>
        public bool ChangeRelayStateEvent1 { get; set; }
        /// <summary>
        /// События о включении/выключении реле 1
        /// </summary>
        public bool ChangeRelayStateEvent2 { get; set; }


        #endregion Properties


        public C2000_2(IPort port) : base (port)
        {
            Model = "С2000-2";
            SupportedModels = new List<string>
            {
                Model,
            };

            var inputs = new List<Shleif>();
            for (byte i = 0; i < inputsCount; i++)
            {
                inputs.Add(new Shleif(this, i));
            }
            Shleifs = inputs;

            Access = new AccessController(this);
        }

        public byte[] Transaction(byte address, byte[] sendArray)
        {
            return AddressTransaction(address, sendArray, Timeouts.ethernetConfig);
        }

        public override void WriteBaseConfig(Action<int> progressStatus)
        {
            CheckDeviceType();
            var progress = 0.0;

            foreach (var command in GetBaseConfig())
            {
                if (Transaction((byte)AddressRS485, command) == null)
                    throw new Exception("Transaction was false!");

                progressStatus(Convert.ToInt32(progress));
                progress += 50.0;
            }

            progressStatus(100);
        }

        private IEnumerable<byte[]> GetBaseConfig()
        {
            var config = new List<byte[]>
            {
                GetRelay1Config(),
                GetRelay2Config(),
            };

            return config;
        }

        private byte[] GetRelay1Config()
        {
            var controlByte = new byte[] { 0xFF };

            var bitArray = new BitArray(controlByte);
            bitArray.Set(0, TurnOffWhenOpening1);
            bitArray.Set(7, TurnOffWhenClosing1);
            bitArray.Set(5, ChangeRelayStateEvent1);
            bitArray.CopyTo(controlByte, 0);

            return
            [
                (byte)OrionCommands.WriteToDeviceMemoryMap,
                0x16,
                0x00,
                0x00,
                controlByte[0]
            ];
        }

        private byte[] GetRelay2Config()
        {
            var controlByte = new byte[] { 0xFF };

            var bitArray = new BitArray(controlByte);
            bitArray.Set(0, TurnOffWhenOpening1);
            bitArray.Set(7, TurnOffWhenClosing1);
            bitArray.Set(5, ChangeRelayStateEvent1);
            bitArray.CopyTo(controlByte, 0);

            return
            [
                (byte)OrionCommands.WriteToDeviceMemoryMap,
                0x17,
                0x00,
                0x00,
                controlByte[0]
            ];
        }

        private void CheckDeviceType()
        {
            var result = GetModelCode((byte)AddressRS485, out var deviceCode);
            if (!result)
                throw new Exception("Device doesn't respond!");
            if (deviceCode != ModelCode)
                throw new Exception("Wrong model!");

        }
    }
}
