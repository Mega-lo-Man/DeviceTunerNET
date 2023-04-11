using DeviceTunerNET.SharedDataModel.ElectricModules;
using DeviceTunerNET.SharedDataModel.ElectricModules.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using static DeviceTunerNET.SharedDataModel.Devices.IOrionNetTimeouts;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class Signal_10 : OrionDevice
    {
        private const int sirenTime = 0x03C0;
        private const int shleifSettingsOffset = 0x0014;
        private const byte supervisedRealysMonitorOffset = 0xD4;
        private readonly int inputsCount = 10;
        private readonly int relayNumber = 2;

        #region Enums
        /// <summary>
        /// Acccording with columns names in the Uprog table (Inputs)
        /// </summary>
        private enum InputPropertiesOffsets : byte
        {
            InputType = 0x00,
            Zone = 0x08,
            AlarmDelay = 0x01,
            ArmingDelay = 0x02,
            InputAnalysisDelayAfterReset = 0x03,
            ActivationDelay1 = 0x04,
            NeverDisarm = 0x09, // 0000_000X
            RearmingIfArmingFailed = 0x09, // 0000_00X0
            RearmingAfterAlarm = 0x09, // 0000_0X00
            DisarmedInputMonitoring = 0x09, // 0000_X000
            InhibitFireInputMonitoring = 0x09, // 000X_0000
            DebounceTime = 0x09, // 00X0_0000
            IgnoreLobbyInputDeviation = 0x09, // 0X00_0000
            ActivateRelayOffset = 0x0A, // 0000_XXXX
        }

        private enum RelayPropertiesOffsets : byte
        {
            ControlProgram = 0x00,
            ControlTime = 0x01,
            Events = 0xD,
        }

        private enum SupervisedRelayProperties : byte
        {

        }

        #endregion Enums

        #region Properties
        public bool TwoPowerInputsMonitor { get; set; }
        public bool En54 { get; set; }
        public bool Buzzer { get; set; }
        public IEnumerable<Shleif> Shleifs { get; set; }
        public IEnumerable<OptoRelay> Relays { get; set; }
        public IEnumerable<SupervisedRelay> SupervisedRelays { get; set; }
        
        #endregion Properties

        public Signal_10(IPort port) : base(port)
        {
            ModelCode = 34;
            SupportedModels = SupportedModels = new List<string>
            {
                "Сигнал - 10",
            };

            var relays = new List<OptoRelay>();

            for (byte i = 0; i < relayNumber; i++)
            {
                relays.Add(new OptoRelay(this, i));
            }

            Relays = relays;

            var supervisedRelays = new List<SupervisedRelay>
            {
                new SupervisedRelay(this, 2),
                new SupervisedRelay(this, 3)
                {
                    ControlTime = sirenTime
                }
            };

            SupervisedRelays = supervisedRelays;

            var inputs = new List<Shleif>();
            for (byte i = 0; i < inputsCount; i++)
            {
                inputs.Add(new Shleif(this, i));
            }
            Shleifs = inputs;
        }

        public byte[] Transaction(byte address, byte[] sendArray)
        {
            return AddressTransaction(address, sendArray, Timeouts.ethernetConfig);
        }

        public override void WriteConfig(Action<int> progressStatus)
        {
            UploadConfig(1.66, AddressRS485, GetConfig(), progressStatus);
        }

        public override void WriteBaseConfig(Action<int> progressStatus)
        {
            UploadConfig(3.0, AddressRS485, GetConfig(), progressStatus);
        }

        private void UploadConfig(double progressStep, uint address, IEnumerable<byte[]> config, Action<int> progressStatus)
        {
            var progress = 1.0;
            progressStatus(Convert.ToInt32(progress));

            foreach (var command in config)
            {
                if (Transaction((byte)address, command) == null)
                    throw new Exception("Transaction false!");

                progressStatus(Convert.ToInt32(progress));
                progress += progressStep;
                Debug.WriteLine(progress.ToString());
            }

            Reboot();

            progressStatus(100);
        }

        private IEnumerable<byte[]> GetBaseConfig()
        {
            var config = new List<byte[]>
            {
                GetTwoPowerEn54Buzzer(),
            };
            
            return config;
        }

        private IEnumerable<byte[]> GetInputsProps(Func<Shleif, IEnumerable<byte[]>> inputParameters)
        {
            var bytesList = new List<byte[]>();

            foreach (var shleif in Shleifs)
            {
                bytesList.AddRange(inputParameters(shleif));
            }
            return bytesList;
        }

        public IEnumerable<byte[]> GetConfig()
        {
            var config = new List<byte[]>
            {
                GetTwoPowerEn54Buzzer(),
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

            foreach (var relay in SupervisedRelays)
            {
                bytesList.AddRange(GetRelaysConfigs(relay));
            }

            bytesList.Add(GetMonitorForSupervisedRelays());

            return bytesList;
        }

        private byte[] GetMonitorForSupervisedRelays()
        {
            //7f 08 94 41 d4 00 00 01
            byte controlByte = (byte)((byte)SupervisedRelays.ElementAt(0).Monitoring |
                                     ((byte)SupervisedRelays.ElementAt(1).Monitoring << 2));
            return new byte[]
            {
                (byte)OrionCommands.WriteToDeviceMemoryMap,
                supervisedRealysMonitorOffset,
                0x00,
                0x00,
                controlByte,
            };

        }

        private IEnumerable<byte[]> GetRelaysConfigs<T>(T relay) where T : OptoRelay
        {
            var result = new List<byte[]>();

            ushort relaysOffset = (ushort)((relay.RelayIndex - 1) * 0x03 + 0xC8);

            result.AddRange(GetRelayConfig(relay, relaysOffset));

            result.Add(GetRelayEvents(relaysOffset));

            return result;
        }
        
        private IEnumerable<byte[]> GetRelayConfig<T>(T relay, uint offset) where T : OptoRelay
        {
            var result = new List<byte[]>();

            byte[] bytes = BitConverter.GetBytes((ushort)(offset + (ushort)RelayPropertiesOffsets.ControlProgram));
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

            return result;
        }

        private byte[] GetRelayEvents(ushort offset)
        {
            var controlByteB = new byte[1] { 0x00 };
            var bits = new BitArray(controlByteB);

            bits.Set(0, Relays.ElementAt(0).Events);
            bits.Set(1, Relays.ElementAt(1).Events);
            bits.Set(2, SupervisedRelays.ElementAt(0).Events);
            bits.Set(3, SupervisedRelays.ElementAt(1).Events);

            bits.CopyTo(controlByteB, 0);

            byte[] bytes = BitConverter.GetBytes((ushort)(offset + (ushort)RelayPropertiesOffsets.Events));
            return new byte[]
            {
                (byte)OrionCommands.WriteToDeviceMemoryMap,
                bytes[0],
                bytes[1],
                0x00,
                controlByteB[0]
            };
        }
        /*
        private byte GetRelayMonitors()
        {
            var controlByteB = new byte[1] { 0x00 };
            var bits = new BitArray(controlByteB);

            bits.Set(0, Relays.ElementAt(0).Events);
            bits.Set(1, Relays.ElementAt(1).Events);
            bits.Set(2, SupervisedRelays.ElementAt(0).Events);
            bits.Set(3, SupervisedRelays.ElementAt(1).Events);

            bits.CopyTo(controlByteB, 0);

        }
        */
        /*
        private IEnumerable<byte[]> GetSupervisedRelayConfig(ushort offset, SupervisedRelay supervisedRelay)
        {
            var result = new List<byte[]>();

            var bytes = new byte[2];
            var writeToMap = (byte)OrionCommands.WriteToDeviceMemoryMap;

            bytes = BitConverter.GetBytes((ushort)(offset + (ushort)RelayPropertiesOffsets.ControlProgram));
            result.Add(new byte[] { (byte)OrionCommands.WriteToDeviceMemoryMap, bytes[0], bytes[1], 0x00, (byte) });

            return result;
        }
        */
        private IEnumerable<byte[]> GetInputProperties(Shleif shleif)
        {
            var result = new List<byte[]>();

            ushort offset = (ushort)((shleif.ShleifIndex - 1) * shleifSettingsOffset);
            var bytes = new byte[2];
            var writeToMap = (byte)OrionCommands.WriteToDeviceMemoryMap;

            bytes = BitConverter.GetBytes((ushort)(offset + (ushort)InputPropertiesOffsets.InputType));
            result.Add(new byte[] { writeToMap, bytes[0], bytes[1], 0x00, (byte)shleif.InputType });

            bytes = BitConverter.GetBytes((ushort)(offset + (ushort)InputPropertiesOffsets.Zone));
            result.Add(new byte[] { writeToMap, bytes[0], bytes[1], 0x00, shleif.Zone });

            bytes = BitConverter.GetBytes((ushort)(offset + (ushort)InputPropertiesOffsets.AlarmDelay));
            result.Add(new byte[] { writeToMap, bytes[0], bytes[1], 0x00, shleif.AlarmDelay });

            bytes = BitConverter.GetBytes((ushort)(offset + (ushort)InputPropertiesOffsets.ArmingDelay));
            result.Add(new byte[] { writeToMap, bytes[0], bytes[1], 0x00, shleif.ArmingDelay });

            bytes = BitConverter.GetBytes((ushort)(offset + (ushort)InputPropertiesOffsets.InputAnalysisDelayAfterReset));
            result.Add(new byte[] { writeToMap, bytes[0], bytes[1], 0x00, shleif.InputAnalysisDelayAfterReset });

            result.AddRange(GetRelayActivationDelays(shleif, offset));

            result.AddRange(GetBitsShleifTune(shleif, offset));
            result.AddRange(GetBitsShleifRelayTune(shleif, offset));

            return result;
        }

        private IEnumerable<byte[]> GetBitsShleifRelayTune(Shleif shleif, ushort offset)
        {
            var controlByteB = new byte[1] { 0x00 };
            var bits = new BitArray(controlByteB);

            foreach (var relayDelayPair in shleif.RelayControlActivations)
            {
                bits.Set(relayDelayPair.Key.RelayIndex - 1, true);
            }

            bits.CopyTo(controlByteB, 0);

            var _ = BitConverter.GetBytes((ushort)(offset + (ushort)InputPropertiesOffsets.ActivateRelayOffset));
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
            bits.Set(1, !shleif.RearmingIfArmingFailed); // 0110 0000 - 0110 0010
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

        private IEnumerable<byte[]> GetRelayActivationDelays(Shleif shleif, ushort offset)
        {
            var relaysCount = Relays.Count() + SupervisedRelays.Count();

            for (var i = 0; i < relaysCount; i++)
            {
                var bytes = BitConverter.GetBytes((ushort)(offset + (ushort)InputPropertiesOffsets.ActivationDelay1 + i));

                var relayDelay = GetValueFromDict(i, shleif.RelayControlActivations);

                yield return new byte[]
                {
                    // 7f 08 e2 (41 04 00 00 01)
                    (byte)OrionCommands.WriteToDeviceMemoryMap, bytes[0], bytes[1], 0x00, relayDelay
                };
            }
        }

        private byte GetValueFromDict(int relayIndex, Dictionary<IRelay, byte> relayControlActivations)
        {
            foreach (var relay in relayControlActivations)
            {
                if (relay.Key.RelayIndex == relayIndex + 1) // нумерация реле начинается с 1
                {
                    return relay.Value;
                }
            }
            return 0x00;
        }

        private byte[] GetTwoPowerEn54Buzzer()
        {
            var controlByte = new byte[] { 0x0F };

            var bitArray = new BitArray(controlByte);
            bitArray.Set(4, TwoPowerInputsMonitor);
            bitArray.Set(5, En54);
            bitArray.Set(6, Buzzer);
            bitArray.CopyTo(controlByte, 0);

            return new byte[]
            {
                (byte)OrionCommands.WriteToDeviceMemoryMap,
                0xD4,
                0x00,
                0x00,
                controlByte[0]
            };
        }
    }
}
