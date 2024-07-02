using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class RipRs24_51 : RipRs
    {
        public new const int ModelCode = 39;
        public new const int Code = 39;

        public RipRs24_51(IPort port) : base(port)
        {
            Model = "РИП-24 исп.51";
            SupportedModels = new List<string>
            {
                Model,
                "РИП-12 исп.50",
            };
        }

        public override bool Setup(Action<int> updateProgressBar, int modelCode = 0)
        {
            return base.Setup(updateProgressBar, Code);
        }
    }
}
