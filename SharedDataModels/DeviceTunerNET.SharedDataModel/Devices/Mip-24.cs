using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class Mip_24 : OrionDevice
    {
        public new const int Code = 49;
        public Mip_24(IPort port) : base(port)
        {
            Model = "МИП-24";
            SupportedModels = new List<string>
            {
                Model
            };
        }
    }
}
