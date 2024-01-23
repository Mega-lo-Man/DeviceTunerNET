using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000_it : OrionDevice
    {
        public new const int Code = 8;
        public C2000_it(IPort port) : base(port)
        {
            Model = "С2000-ИТ";
            SupportedModels = new List<string>
            {
                Model
            };
        }
    }
}
