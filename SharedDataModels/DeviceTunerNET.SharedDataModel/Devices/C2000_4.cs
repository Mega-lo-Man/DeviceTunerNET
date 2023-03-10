using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000_4 : OrionDevice
    {
        public C2000_4(IPort port) : base(port)
        {
            ModelCode = 4;
        }
    }
}
