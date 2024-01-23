using System;
using System.Collections.Generic;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000_pge : OrionDevice
    {
        public new const int Code = 43;
        public C2000_pge(IPort port) : base(port)
        {
            Model = "С2000-PGE";
            SupportedModels = new List<string>
            {
                Model,
                "С2000-PGE исп.01"
            };
        }
    }
}
