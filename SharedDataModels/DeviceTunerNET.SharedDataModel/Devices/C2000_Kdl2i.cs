using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000_Kdl2i : C2000_Kdl
    {
        public C2000_Kdl2i(IPort port) : base(port)
        {
            ModelCode = 41;
        }
    }
}
