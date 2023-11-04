using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class Signal_10 : OrionDevice
    {
        public new const int Code = 34;

        public Signal_10(IPort port) : base(port)
        {
            Model = "Сигнал-10";
            SupportedModels = new List<string>
            {
                Model,
            };
        }
    }
}
