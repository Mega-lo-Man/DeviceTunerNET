using DeviceTunerNET.SharedDataModel.ElectricModules;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000_4 : OrionDevice
    {
        private readonly int inputsCount = 4;
        private readonly int relayNumber = 2;
        private const int sirenTime = 0x03C0;

        public new const int Code = 4;

        #region Properties
        public IEnumerable<Shleif> Shleifs { get; set; }
        public IEnumerable<Relay> Relays { get; set; }
        public IEnumerable<SupervisedRelay> SupervisedRelays { get; }
        #endregion Properties

        public C2000_4(IPort port) : base(port)
        {
            Model = "С2000-4";
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

            var relays = new List<Relay>();

            for (byte i = 0; i < relayNumber; i++)
            {
                relays.Add(new Relay(this, i));
            }

            Relays = relays;

            var supervisedRelays = new List<SupervisedRelay>
            {
                new(this, 2),
                new(this, 3)
                {
                    ControlTime = sirenTime
                }
            };


            SupervisedRelays = supervisedRelays;
        }
    }
}
