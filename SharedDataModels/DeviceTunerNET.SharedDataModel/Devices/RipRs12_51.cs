using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class RipRs12_51 : RipRs
    {
        public new const int Code = 33;

        public RipRs12_51(IPort port) : base(port)
        {
            Model = "РИП-12 исп.51";
            SupportedModels = new List<string>
            {
                Model,
                "РИП-12 исп.50",
                "РИП-12",
            };
        }
    }
}
