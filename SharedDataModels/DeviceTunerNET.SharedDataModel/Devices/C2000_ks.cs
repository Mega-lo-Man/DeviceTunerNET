using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000_ks : OrionDevice
    {
        public new const int Code = 13;
        public C2000_ks(IPort port) : base(port)
        {
            Model = "С2000-КС";
            SupportedModels = new List<string>
            {
                Model
            };
        }
    }
}
