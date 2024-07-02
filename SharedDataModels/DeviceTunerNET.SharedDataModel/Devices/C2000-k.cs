using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000_k : OrionDevice
    {
        public new const int ModelCode = 7;
        public new const int Code = 7;
        public C2000_k(IPort port) : base(port)
        {
            Model = "С2000-К";
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
