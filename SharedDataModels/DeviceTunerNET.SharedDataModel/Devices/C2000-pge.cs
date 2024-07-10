using System;
using System.Collections.Generic;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000_pge : OrionDevice
    {
        public const int Code = 43;
        public C2000_pge(IPort port) : base(port)
        {
            ModelCode = Code;
            Model = "С2000-PGE";
            SupportedModels = new List<string>
            {
                Model,
                "С2000-PGE исп.01"
            };
        }

        public override bool Setup(Action<int> updateProgressBar, int modelCode = 0)
        {
            return base.Setup(updateProgressBar, Code);
        }
    }
}
