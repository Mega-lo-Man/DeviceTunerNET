using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class RipRs_48 : RipRs
    {
        public new const int ModelCode = 53;
        public new const int Code = 53;

        public RipRs_48(IPort port) : base(port)
        {
            Model = "РИП-48 исп.01";
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
