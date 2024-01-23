using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000_bki : OrionDevice
    {
        public new const int Code = 45;
        public C2000_bki(IPort port) : base(port)
        {
            Model = "Поток-БКИ";
            SupportedModels = new List<string>
            {
                Model
            };
        }
    }
}
