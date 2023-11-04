using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000sp1 : OrionDevice
    {
        public new const int Code = 3;

        public C2000sp1(IPort port) : base(port)
        { 
            Model = "С2000-СП1";
            SupportedModels = new List<string>
            {
                Model,
            };
        }
    }
}
