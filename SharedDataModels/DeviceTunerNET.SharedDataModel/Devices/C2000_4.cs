using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000_4 : OrionDevice
    {
        public new const int Code = 4;
        public C2000_4(IPort port) : base(port)
        {
            Model = "С2000-4";
            SupportedModels = new List<string>
            {
                Model,
            };
        }
    }
}
