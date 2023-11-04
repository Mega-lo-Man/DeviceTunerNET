using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class RipRs12_56 : RipRs
    {
        public new const int Code = 54;

        public RipRs12_56(IPort port) : base(port)
        {
            Model = "РИП-12 исп.54";
            SupportedModels = new List<string>
            {
                Model,
            };
        }
    }
}
