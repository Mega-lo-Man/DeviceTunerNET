using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000_bki : OrionDevice
    {
        public const int Code = 45;
        public C2000_bki(IPort port) : base(port)
        {
            ModelCode = Code;
            Model = "Поток-БКИ";
            SupportedModels = new List<string>
            {
                Model
            };
        }

        public override bool Setup(Action<int> updateProgressBar, int modelCode = 0)
        {
            return base.Setup(updateProgressBar, Code);
        }
    }
}
