using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000_2 : OrionDevice
    {
        public C2000_2(IPort port) : base (port)
        {
            ModelCode = 16;
        }


    }
}
