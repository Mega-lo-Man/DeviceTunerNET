using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class Signal20 : OrionDevice
    {
        public new const int Code = 1;
        public Signal20(IPort port) : base(port)
        {
            Model = "Сигнал-20";
            SupportedModels = new List<string>
            {
                Model
            };
        }
    }
}
