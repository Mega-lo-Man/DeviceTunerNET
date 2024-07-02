using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000_aspt : OrionDevice
    {
        public new const int ModelCode = 14;
        public new const int Code = 14;

        public C2000_aspt(IPort port) : base(port)
        {
            Model = "С2000-АСПТ";
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
