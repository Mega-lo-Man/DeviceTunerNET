using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class UO4S : OrionDevice
    {
        public new const int Code = 24;

        public UO4S(IPort port) : base(port)
        {
            Model = "УО-4С";
            SupportedModels = new List<string>
            {
                Model,
                "УО-4С исп.02"
            };
        }
    }
}
