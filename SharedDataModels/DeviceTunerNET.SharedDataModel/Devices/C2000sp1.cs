using DeviceTunerNET.SharedDataModel.ElectricModules;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000sp1 : OrionDevice
    {
        private readonly int relayNumber = 4;

        public new const int Code = 3;
        public IEnumerable<Relay> Relays { get; set; }

        public C2000sp1(IPort port) : base(port)
        { 
            Model = "С2000-СП1";
            SupportedModels = new List<string>
            {
                Model,
            };

            var relays = new List<Relay>();

            for (byte i = 0; i < relayNumber; i++)
            {
                relays.Add(new Relay(this, i));
            }

            Relays = relays;
        }
    }
}
