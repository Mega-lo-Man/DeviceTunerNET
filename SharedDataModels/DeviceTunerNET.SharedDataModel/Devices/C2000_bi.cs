using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000_bi : OrionDevice
    {
        public new const int Code = 10;
        public C2000_bi(IPort port) : base(port)
        {
            Model = "С2000-БИ/БКИ";
            SupportedModels = new List<string>
            {
                Model
            };
        }
    }
}
