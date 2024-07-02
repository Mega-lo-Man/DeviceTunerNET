using DeviceTunerNET.SharedDataModel.ElectricModules;
using DeviceTunerNET.SharedDataModel.ElectricModules.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using static DeviceTunerNET.SharedDataModel.Devices.IOrionNetTimeouts;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class Signal20P : OrionDevice, IShleifs, IRelays
    {
        private const int sirenTime = 0x03C0;
        private readonly int inputsCount = 20;
        private readonly int relayNumber = 3;
        //private readonly int supervisedRelayNumber = 2;

        public new const int ModelCode = 2;
        public new const int Code = 2;

        #region Enums
        /// <summary>
        /// Acccording with columns names in the Uprog table (Inputs)
        /// </summary>
        private enum InputPropertiesOffsets : byte
        {
            InputType = 0x00,
            Zone = 0x01,
            AlarmDelay = 0x02,
            ArmingDelay = 0x03,    
            InputAnalysisDelayAfterReset = 0x04,
            ShuntTime = 0x05,
            ActivationDelay1 = 0x06,
            ActivationDelay2 = 0x07,
            ActivationDelay3 = 0x08,
            ActivationDelay4 = 0x09,
            RelayActivationDelay5 = 0x0A,
            NeverDisarm = 0x0B, // 0000_000X
            RearmingIfArmingFailed = 0x0B, // 0000_00X0
            RearmingAfterAlarm = 0x0B, // 0000_0X00
            DisarmedInputMonitoring = 0x0B, // 0000_X000
            InhibitFireInputMonitoring = 0x0B, // 000X_0000
            DebounceTime = 0x0B, // 00X0_0000
            IgnoreLobbyInputDeviation = 0x0B, // 0X00_0000
            Relay1 = 0x0C, // 0000_000X
            Relay2 = 0x0C, // 0000_00X0
            Relay3 = 0x0C, // 0000_0X00
            Relay4 = 0x0C, // 0000_X000
            Relay5 = 0x0C  // 000X_0000
        }

        private enum RelayPropertiesOffsets : byte
        {
            OutputType = 0x00,
            ControlProgram = 0x01,
            ControlTime = 0x02,
            Events = 0x0B,

        }

        private enum SupervisedRelayPropertiesOffsets : byte
        {
            ActivatedMonitoring = 0x04,
            DeactivatedMonitoring = 0x05,
            MonitoringModes = 0x06,
            ShortCircuit = 0x07,
            BreakTreshold = 0x09
        }
        #endregion Enums

        #region Properties
        public bool TwoPowerInputsMonitor { get; set; }
        public IEnumerable<Shleif> Shleifs { get; set; }
        public IEnumerable<Relay> Relays { get; set; }
        public IEnumerable<SupervisedRelay> SupervisedRelays { get; set; }
        #endregion Properties

        #region Constructor
        public Signal20P(IPort port) : base(port)
        {
            Model = "Сигнал-20П";
            SupportedModels = SupportedModels = new List<string> 
            {
                Model,
                "Сигнал-20П исп.01"
            };  

            var inputs = new List<Shleif>();
            for(byte i = 0; i < inputsCount; i++)
            {
                inputs.Add(new Shleif(this, i));
            }
            Shleifs = inputs;

            var relays = new List<Relay>();

            for(byte i = 0; i < relayNumber; i++)
            {
                relays.Add(new Relay(this, i));
            }
            
            Relays = relays;

            var supervisedRelays = new List<SupervisedRelay>
            {
                new SupervisedRelay(this, 3),
                new SupervisedRelay(this, 4)
                {
                    ControlTime = sirenTime
                }
            };


            SupervisedRelays = supervisedRelays;
        }
        #endregion Constructor

        public byte[] Transaction(byte address, byte[] sendArray)
        {
            return AddressTransaction(address, sendArray, Timeouts.ethernetConfig);
        }

        public override void WriteConfig(Action<int> progressStatus)
        {
            CheckDeviceType();
            var progress = 0.0;

            foreach (var command in GetConfig())
            {
                if (Transaction((byte)AddressRS485, command) == null)
                    throw new Exception("Transaction was false!");

                progressStatus(Convert.ToInt32(progress));
                progress += 0.364;
            }
            Reboot();
            progressStatus(100);
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
                progress += 1.587;
            }
            Reboot();
            progressStatus(100);
        }

        private IEnumerable<byte[]> GetBaseConfig()
        {
            var config = new List<byte[]>
            {
                GetPromoter(),
                GetPromoter(),
                GetTwoPowerInputsMonitor(),
            };
            config.AddRange(GetInputsProps(GetBaseInputsProps));

            return config;
        }

        public IEnumerable<byte[]> GetConfig()
        {
            var config = new List<byte[]>
            {
                GetPromoter(),
                GetPromoter(),
                GetTwoPowerInputsMonitor(),
            };
            config.AddRange(GetInputsProps(GetInputProperties));
            config.AddRange(GetRelaysProps());

            return config;
        }

        private IEnumerable<byte[]> GetRelaysProps()
        {
            var bytesList = new List<byte[]>();

            foreach (var relay in Relays)
            {
                bytesList.AddRange(GetRelaysConfigs(relay));
            }

            foreach (var supervisedRelay in SupervisedRelays)
            {
                bytesList.AddRange(GetRelaysConfigs(supervisedRelay));
            }

            return bytesList;
        }

        private IEnumerable<byte[]> GetRelaysConfigs<T>(T relay) where T : Relay
        {
            var result = new List<byte[]>();

            ushort offset = (ushort)((relay.RelayIndex - 1) * 0x000C + 0x01D8);

            result.AddRange(GetRelayConfig(relay, offset));

            if (relay is SupervisedRelay supervisedRelay)
            {
                result.AddRange(GetSupervisedRelayConfig(offset, supervisedRelay));
            }

            return result;
        }

        private IEnumerable<byte[]> GetRelayConfig<T>(T relay, ushort offset) where T : Relay
        {
            var result = new List<byte[]>();

            byte[] bytes = BitConverter.GetBytes((ushort)(offset + (ushort)RelayPropertiesOffsets.OutputType));
            result.Add(new byte[] { (byte)OrionCommands.WriteToDeviceMemoryMap, bytes[0], bytes[1], 0x00, (byte)relay.OutputType });

            bytes = BitConverter.GetBytes((ushort)(offset + (ushort)RelayPropertiesOffsets.ControlProgram));
            result.Add(new byte[] { (byte)OrionCommands.WriteToDeviceMemoryMap, bytes[0], bytes[1], 0x00, (byte)relay.ControlProgram });

            bytes = BitConverter.GetBytes((ushort)(offset + (ushort)RelayPropertiesOffsets.ControlTime));
            var ControlTimeBytes = BitConverter.GetBytes(relay.ControlTime);
            result.Add(new byte[]
            {
                (byte)OrionCommands.WriteToDeviceMemoryMap,
                bytes[0],
                bytes[1],
                0x00,
                ControlTimeBytes[0],
                ControlTimeBytes[1]
            });

            bytes = BitConverter.GetBytes((ushort)(offset + (ushort)RelayPropertiesOffsets.Events));
            result.Add(new byte[]
            {
                (byte)OrionCommands.WriteToDeviceMemoryMap,
                bytes[0],
                bytes[1],
                0x00,
                (byte)(relay.Events ? 0x01 : 0x02)
            });

            return result;
        }

        private IEnumerable<byte[]> GetSupervisedRelayConfig(ushort offset, SupervisedRelay supervisedRelay)
        {
            var result = new List<byte[]>();

            byte[] bytes = BitConverter.GetBytes((ushort)(offset + (ushort)SupervisedRelayPropertiesOffsets.ActivatedMonitoring));
            result.Add(new byte[] { (byte)OrionCommands.WriteToDeviceMemoryMap, bytes[0], bytes[1], 0x00, (byte)supervisedRelay.ActivatedOutputType });

            bytes = BitConverter.GetBytes((ushort)(offset + (ushort)SupervisedRelayPropertiesOffsets.DeactivatedMonitoring));
            result.Add(new byte[] { (byte)OrionCommands.WriteToDeviceMemoryMap, bytes[0], bytes[1], 0x00, (byte)supervisedRelay.DeactivatedOutputType });

            bytes = BitConverter.GetBytes((ushort)(offset + (ushort)SupervisedRelayPropertiesOffsets.MonitoringModes));
            result.Add(new byte[] { (byte)OrionCommands.WriteToDeviceMemoryMap, bytes[0], bytes[1], 0x00, (byte)supervisedRelay.ActivatedOutputMode });

            bytes = BitConverter.GetBytes((ushort)(offset + (ushort)SupervisedRelayPropertiesOffsets.ShortCircuit));
            var tresholdA = BitConverter.GetBytes(supervisedRelay.ShortCircuitTreshold);
            result.Add(new byte[] { (byte)OrionCommands.WriteToDeviceMemoryMap, bytes[0], bytes[1], 0x00, tresholdA[0], tresholdA[1] });

            bytes = BitConverter.GetBytes((ushort)(offset + (ushort)SupervisedRelayPropertiesOffsets.BreakTreshold));
            var tresholdB = BitConverter.GetBytes(supervisedRelay.BreakTreshold);
            result.Add(new byte[] { (byte)OrionCommands.WriteToDeviceMemoryMap, bytes[0], bytes[1], 0x00, tresholdB[0], tresholdB[1] });

            return result;
        }

        private byte[] GetTwoPowerInputsMonitor()
        {
            var controlByte = new byte[] { 0xFF };
            
            var bitArray = new BitArray(controlByte);
            bitArray.Set(7, TwoPowerInputsMonitor);
            bitArray.CopyTo(controlByte, 0);

            return new byte[] 
            { 
                (byte)OrionCommands.WriteToDeviceMemoryMap, 
                0x14, 
                0x02, 
                0x00,
                controlByte[0]
            };
        }

        private IEnumerable<byte[]> GetInputsProps(Func<Shleif, IEnumerable<byte[]>> inputParameters)
        {
            var bytesList = new List<byte[]>();

            foreach(var shleif in Shleifs)
            {
                bytesList.AddRange(inputParameters(shleif)/*GetInputProperties(shleif)*/);
            }
            return bytesList;
        }

        private IEnumerable<byte[]>GetBaseInputsProps(Shleif shleif)
        {
            var result = new List<byte[]>();

            ushort offset = (ushort)((shleif.ShleifIndex - 1) * 0x0016);

            var bytes = new byte[2];

            bytes = BitConverter.GetBytes((ushort)(offset + (ushort)InputPropertiesOffsets.InputType));
            result.Add(new byte[] { (byte)OrionCommands.WriteToDeviceMemoryMap, bytes[0], bytes[1], 0x00, (byte)shleif.InputType });

            result.AddRange(GetBitsShleifTune(shleif, offset));
            result.AddRange(GetBitsShleifRelayTune(shleif, offset));

            return result;
        }
            
        private IEnumerable<byte[]> GetInputProperties(Shleif shleif)
        {
            var result = new List<byte[]>();

            ushort offset = (ushort)((shleif.ShleifIndex - 1) * 0x0016);

            var bytes = new byte[2];

            bytes = BitConverter.GetBytes((ushort)(offset + (ushort)InputPropertiesOffsets.InputType));
            result.Add(new byte[] { (byte)OrionCommands.WriteToDeviceMemoryMap, bytes[0], bytes[1], 0x00, (byte)shleif.InputType });

            bytes = BitConverter.GetBytes((ushort)(offset + (ushort)InputPropertiesOffsets.Zone));
            result.Add(new byte[] { (byte)OrionCommands.WriteToDeviceMemoryMap, bytes[0], bytes[1], 0x00, shleif.Zone });

            bytes = BitConverter.GetBytes((ushort)(offset + (ushort)InputPropertiesOffsets.ArmingDelay));
            result.Add(new byte[] { (byte)OrionCommands.WriteToDeviceMemoryMap, bytes[0], bytes[1], 0x00, shleif.ArmingDelay });

            bytes = BitConverter.GetBytes((ushort)(offset + (ushort)InputPropertiesOffsets.InputAnalysisDelayAfterReset));
            result.Add(new byte[] { (byte)OrionCommands.WriteToDeviceMemoryMap, bytes[0], bytes[1], 0x00, shleif.InputAnalysisDelayAfterReset });

            bytes = BitConverter.GetBytes((ushort)(offset + (ushort)InputPropertiesOffsets.ShuntTime));
            result.Add(new byte[] { (byte)OrionCommands.WriteToDeviceMemoryMap, bytes[0], bytes[1], 0x00, shleif.ShuntTime });

            bytes = BitConverter.GetBytes((ushort)(offset + (ushort)InputPropertiesOffsets.ActivationDelay1));
            result.Add(new byte[] { (byte)OrionCommands.WriteToDeviceMemoryMap, bytes[0], bytes[1], 0x00, shleif.ActivationDelay1 });

            bytes = BitConverter.GetBytes((ushort)(offset + (ushort)InputPropertiesOffsets.ActivationDelay2));
            result.Add(new byte[] { (byte)OrionCommands.WriteToDeviceMemoryMap, bytes[0], bytes[1], 0x00, shleif.ActivationDelay2 });

            bytes = BitConverter.GetBytes((ushort)(offset + (ushort)InputPropertiesOffsets.ActivationDelay3));
            result.Add(new byte[] { (byte)OrionCommands.WriteToDeviceMemoryMap, bytes[0], bytes[1], 0x00, shleif.ActivationDelay3 });

            bytes = BitConverter.GetBytes((ushort)(offset + (ushort)InputPropertiesOffsets.ActivationDelay4));
            result.Add(new byte[] { (byte)OrionCommands.WriteToDeviceMemoryMap, bytes[0], bytes[1], 0x00, shleif.ActivationDelay4 });

            bytes = BitConverter.GetBytes((ushort)(offset + (ushort)InputPropertiesOffsets.RelayActivationDelay5));
            result.Add(new byte[] { (byte)OrionCommands.WriteToDeviceMemoryMap, bytes[0], bytes[1], 0x00, shleif.RelayActivationDelay5 });

            result.AddRange(GetBitsShleifTune(shleif, offset));
            result.AddRange(GetBitsShleifRelayTune(shleif, offset));

            return result;
        }

        private IEnumerable<byte[]> GetBitsShleifRelayTune(Shleif shleif, ushort offset)
        {
            var controlByteB = new byte[1] { 0x00 };
            var bits = new BitArray(controlByteB);
            bits.Set(0, shleif.Relay1);
            bits.Set(1, shleif.Relay2);
            bits.Set(2, shleif.Relay3);
            bits.Set(3, shleif.Relay4);
            bits.Set(4, shleif.Relay5);

            bits.CopyTo(controlByteB, 0);

            var _ = BitConverter.GetBytes((ushort)(offset + (ushort)InputPropertiesOffsets.Relay1));
            var cmd = new List<byte[]> 
            { 
                new byte[] { (byte)OrionCommands.WriteToDeviceMemoryMap, _[0], _[1], 0x00, controlByteB[0] } 
            };
            return cmd;
        }

        private IEnumerable<byte[]> GetBitsShleifTune(Shleif shleif, ushort offset)
        {
            var controlByte = new byte[1] { 0x00 };
            var bits = new BitArray(controlByte);
            bits.Set(0, shleif.NeverDisarm);
            bits.Set(1, shleif.RearmingIfArmingFailed);
            bits.Set(2, shleif.RearmingAfterAlarm);
            bits.Set(3, shleif.DisarmedInputMonitoring);
            bits.Set(4, shleif.InhibitFireInputMonitoring);
            bits.Set(5, shleif.DebounceTime);
            bits.Set(6, shleif.IgnoreLobbyInputDeviation);
            bits.CopyTo(controlByte, 0);

            var _ = BitConverter.GetBytes((ushort)(offset + (ushort)InputPropertiesOffsets.NeverDisarm));
            var cmd = new List<byte[]> 
            { 
                new byte[] { (byte)OrionCommands.WriteToDeviceMemoryMap, _[0], _[1], 0x00, controlByte[0] }
            };
            return cmd;
        }

        private byte[] GetPromoter()
        {
            return new byte[] { 0x43, 0x00, 0x00, 0x00, 0x40 }; // Unrecognized pattern
        }

        private void CheckDeviceType()
        {
            var result = GetModelCode((byte)AddressRS485, out var deviceCode);
            if(!result)
                throw new Exception("Device doesn't respond!");
            if (deviceCode != ModelCode)
                throw new Exception("Wrong model!");

        }

        public override bool Setup(Action<int> updateProgressBar, int modelCode = 0)
        {
            return base.Setup(updateProgressBar, Code);
        }
    }
}
