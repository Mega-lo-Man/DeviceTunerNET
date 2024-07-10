using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000_Kdl2i : C2000_Kdl
    {
        public const int Code = 41;
        public C2000_Kdl2i(IPort port) : base(port)
        {
            ModelCode = Code;
            Model = "С2000-КДЛ-2И";
            SupportedModels = new List<string>
            {
                Model,
            };
        }

        public override bool Setup(Action<int> updateProgressBar, int modelCode = 0)
        {
            return base.Setup(updateProgressBar, Code);
        }
    }
}
