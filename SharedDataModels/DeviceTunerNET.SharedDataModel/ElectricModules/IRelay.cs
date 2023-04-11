using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.ElectricModules
{
    public interface IRelay
    {
        /// <summary>
        /// Relay number Relay 1, Relay, 2, Relay 3 etc.
        /// </summary>
        public byte RelayIndex { get; set; }

        bool TurnOn();
        bool TurnOff();
    }
}
