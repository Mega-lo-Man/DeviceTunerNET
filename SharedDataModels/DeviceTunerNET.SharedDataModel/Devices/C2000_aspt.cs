using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000_aspt : OrionDevice
    {
        public new const int Code = 14;

        public C2000_aspt(IPort port) : base(port)
        {
            Model = "С2000-АСПТ";
            SupportedModels = new List<string>
            {
                Model,
            };
        }
    }
}
