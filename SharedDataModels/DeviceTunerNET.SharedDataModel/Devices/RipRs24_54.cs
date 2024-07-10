using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class RipRs24_54 : RipRs
    {
        public const int Code = 38;

        public RipRs24_54(IPort port) : base(port)
        {
            ModelCode = Code;
            Model = "РИП-24 исп.54";
            SupportedModels = new List<string>
            {
                Model,
                "РИП-24 исп.54",
            };
        }

        public override bool Setup(Action<int> updateProgressBar, int modelCode = 0)
        {
            return base.Setup(updateProgressBar, Code);
        }
    }
}
