using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000_Kdl : OrionDevice
    {
        public C2000_Kdl(IPort port) : base(port)
        {
            ModelCode = 9;
        }
    }
}
