using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.SharedDataModel.ElectricModules
{
    public class SupervisedRelay : Relay
    {
        private const ushort _minimumTresholdLevel = 0x005;
        private const ushort _maximumTresholdLevel = 0x9C4;

        /// <summary>
        /// Activated Output Monitoring Types
        /// </summary>
        public enum ActivatedTypes : byte
        {
            NoMonitoring = 0x00,
            OpenFaults = 0x01,
            ShortFaults = 0x02,
            OpenAndShortFaults = 0x03,

        }

        /// <summary>
        /// Deactivated Output Monitoring Types
        /// </summary>
        public enum DeactivatedTypes : byte
        {
            NoMonitoring = 0x00,
            ShortFaults = 0x04,
            OpenAndShortFaults = 0x08,
            OpenFaults = 0x0C
        }

        /// <summary>
        /// Activated Output Monitoring Modes
        /// </summary>
        public enum ActivatedModes : byte
        {
            ByTreshold = 0x00,
            SharpDecrease = 0x01,
            PeriodicLoadDisconnection = 0x02
        }

        public SupervisedRelay(IPort port, IOrionDevice orionDevice, byte relayIndex) : base(port, orionDevice, relayIndex)
        {
            parentDevice = orionDevice;
            RelayIndex = ++relayIndex;
        }

        /// <summary>
        /// Activated Output Monitoring Type
        /// </summary>
        public ActivatedTypes ActivatedOutputType { get; set; } = ActivatedTypes.OpenFaults;

        /// <summary>
        /// Deactivated Output Monitoring Type
        /// </summary>
        public DeactivatedTypes DeactivatedOutputType { get; set; } = DeactivatedTypes.NoMonitoring;

        /// <summary>
        /// Activated Output Monitoring Mode
        /// </summary>
        public ActivatedModes ActivatedOutputMode { get; set; } = ActivatedModes.ByTreshold;

        /// <summary>
        /// Activated output break threshold
        /// </summary>
        public ushort BreakTreshold { get; set; } = _maximumTresholdLevel;

        /// <summary>
        /// Short circuit threshold of activated output
        /// </summary>
        public ushort ShortCircuitTreshold { get; set; } = _minimumTresholdLevel;

    }
}
