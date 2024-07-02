using DeviceTunerNET.SharedDataModel.ElectricModules;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class Signal_10 : OrionDevice
    {
        private readonly int inputsCount = 10;
        private readonly int relayNumber = 2;
        private const int sirenTime = 0x03C0;

        public new const int ModelCode = 34;
        public new const int Code = 34;

        #region Properties
        public IEnumerable<Shleif> Shleifs { get; set; }
        public IEnumerable<Relay> Relays { get; set; }
        public IEnumerable<SupervisedRelay> SupervisedRelays { get; }
        #endregion Properties

        public Signal_10(IPort port) : base(port)
        {
            Model = "Сигнал-10";
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

        public override bool Setup(Action<int> updateProgressBar, int modelCode = 0)
        {
            return base.Setup(updateProgressBar, Code);
        }
    }
}
