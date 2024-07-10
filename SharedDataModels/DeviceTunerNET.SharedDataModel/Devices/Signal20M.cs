using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class Signal20M : Signal20P
    {
        public const int Code = 26;

        public Signal20M(IPort port) : base(port)
        {
            ModelCode = Code;
            Model = "Сигнал-20М";
            SupportedModels = new List<string>
            {
                Model,
            };
        }

        public override bool Setup(Action<int> updateProgressBar, int modelCode = 0)
        {
            return base.Setup(updateProgressBar, Code);
        }
    }
}
