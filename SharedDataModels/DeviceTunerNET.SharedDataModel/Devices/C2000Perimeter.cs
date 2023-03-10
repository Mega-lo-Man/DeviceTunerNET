using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000Perimeter : OrionDevice
    {
        public C2000Perimeter(IPort port) : base(port)
        {
            ModelCode = 47;
        }
    }
}
