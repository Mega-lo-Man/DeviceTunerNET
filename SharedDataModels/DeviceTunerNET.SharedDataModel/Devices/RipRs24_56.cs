using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class RipRs24_56 : RipRs
    {
        public new const int Code = 55;

        public RipRs24_56(IPort port) : base(port)
        { 
            Model = "РИП-24 исп.56";
            SupportedModels = new List<string>
            {
                Model,
            };
        }
    }
}
