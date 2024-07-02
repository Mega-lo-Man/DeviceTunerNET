using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class Mip_12 : OrionDevice
    {
        public new const int ModelCode = 48;
        public new const int Code = 48;
        public Mip_12(IPort port) : base(port)
        {
            Model = "МИП-12";
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
