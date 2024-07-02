using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class UO4S : OrionDevice
    {
        public new const int ModelCode = 24;
        public new const int Code = 24;

        public UO4S(IPort port) : base(port)
        {
            Model = "УО-4С";
            SupportedModels = new List<string>
            {
                Model,
                "УО-4С исп.02"
            };
        }

        public override bool Setup(Action<int> updateProgressBar, int modelCode = 0)
        {
            return base.Setup(updateProgressBar, Code);
        }
    }
}
