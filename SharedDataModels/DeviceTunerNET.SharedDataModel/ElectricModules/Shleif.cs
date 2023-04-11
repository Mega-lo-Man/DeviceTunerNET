using DeviceTunerNET.SharedDataModel.Devices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Net;

namespace DeviceTunerNET.SharedDataModel.ElectricModules
{
    public class Shleif
    {
        private const int shleifValueIndex = 2;
        private const int getAdcValueCode = 0x1B;
        private const int getAdcState = 0x19;
        private readonly IOrionDevice _parentDevice;

        public enum InputTypes : byte
        {
            SmokeFire = 0,
            CombinedFire = 1,
            HeatFire = 2,
            Intrusion = 3,
            IntrusionAndTamper = 4,
            Auxilary = 5,
            Lobby = 6,
            Panic = 10,
            AuxProgrammable = 11,
            FireManualCallPoint = 15,
            FloodAlarm = 16,
            ManualRelease = 17
        }

        public enum States : byte
        {
            UnderGuard = 24, // Взят
            RemovedGuard = 109, // Снят
            Alarm = 3, // Тревога
            TakeFailed = 17, // Невзятие
            Unknow = 0 // неопознан
        }

        /// <summary>
        /// Input number Input 1, Input 2, ... Input 20 
        /// </summary>
        public byte ShleifIndex { get; set; } = 0;

        public InputTypes InputType { get; set; } = InputTypes.Intrusion;

        /// <summary>
        /// Zone
        /// </summary>
        public byte Zone { get; set; } = 0;

        /// <summary>
        /// Alarm delay, s
        /// </summary>
        public byte AlarmDelay { get; set; } = 0;

        /// <summary>
        /// Arming delay, n
        /// </summary>
        public byte ArmingDelay { get; set; } = 0;

        /// <summary>
        /// Input analysis delay after reset, s
        /// </summary>
        public byte InputAnalysisDelayAfterReset { get; set; } = 1;

        /// <summary>
        /// Shunt time, s
        /// </summary>
        public byte ShuntTime { get; set; } = 1;
        
        /// <summary>
        /// Never disarm
        /// </summary>
        public bool NeverDisarm { get; set; } = false;

        /// <summary>
        /// Rearming if armed failed
        /// </summary>
        public bool RearmingIfArmingFailed { get; set; } = true;

        /// <summary>
        /// Rearming after alarm
        /// </summary>
        public bool RearmingAfterAlarm { get; set; } = false;

        /// <summary>
        /// Disarmed input monitoring
        /// </summary>
        public bool DisarmedInputMonitoring { get; set; } = false;

        /// <summary>
        /// Inhibit fire input monitoring
        /// </summary>
        public bool InhibitFireInputMonitoring { get; set; } = false;

        /// <summary>
        /// Debounce time 300 ms
        /// </summary>
        public bool DebounceTime { get; set; } = true;

        /// <summary>
        /// Ignore 10% of lobby input deviation
        /// </summary>
        public bool IgnoreLobbyInputDeviation { get; set; } = true;

        /// <summary>
        /// Activation relay control (Relay, delay)
        /// </summary>
        public Dictionary<IRelay, byte> RelayControlActivations { get; set; } = new ();

        public Shleif(IOrionDevice orionDevice, byte ShleifIndex)
        {
            _parentDevice = orionDevice;
            this.ShleifIndex = ++ShleifIndex;
        }

        /// <summary>
        /// Get current Analog-to-Digital converter value
        /// </summary>
        /// <returns>ADC current value</returns>
        public byte GetShleifAdcValue()
        {
            var serialPort = _parentDevice.Port;
            var address = (byte)_parentDevice.AddressRS485;
            var adcValue = _parentDevice.AddressTransaction(address, new byte[] { getAdcValueCode, ShleifIndex, 0x00 }, IOrionNetTimeouts.Timeouts.addressChanging);
            return adcValue[shleifValueIndex];
        }

        public States GetShleifState()
        {
            var serialPort = _parentDevice.Port;
            var address = (byte)_parentDevice.AddressRS485;
            var adcValue = _parentDevice.AddressTransaction(address, new byte[] { getAdcState, ShleifIndex, 0x00 }, IOrionNetTimeouts.Timeouts.addressChanging);
            return adcValue[shleifValueIndex] switch
            {
                (byte)States.Alarm => States.Alarm,
                (byte)States.TakeFailed => States.TakeFailed,
                (byte)States.UnderGuard => States.UnderGuard,
                (byte)States.RemovedGuard => States.RemovedGuard,
                _ => States.Unknow
            };
        }
    }
}