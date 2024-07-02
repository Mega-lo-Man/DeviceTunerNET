using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class RipRs24_54 : RipRs
    {
        public new const int ModelCode = 38;
        public new const int Code = 38;

        public RipRs24_54(IPort port) : base(port)
        {
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
