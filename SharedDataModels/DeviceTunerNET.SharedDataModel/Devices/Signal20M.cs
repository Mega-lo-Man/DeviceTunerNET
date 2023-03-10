using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class Signal20M : Signal20P
    {
        public Signal20M(IPort port) : base(port)
        {
            ModelCode = 26;
        }
    }
}
