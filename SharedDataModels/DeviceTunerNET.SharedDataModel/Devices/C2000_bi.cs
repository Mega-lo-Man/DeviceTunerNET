using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000_bi : OrionDevice
    {
        public const int Code = 10;
        public C2000_bi(IPort port) : base(port)
        {
            ModelCode = Code;
            Model = "С2000-БИ/БКИ";
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
