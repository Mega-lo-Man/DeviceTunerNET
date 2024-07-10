using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000Perimeter : OrionDevice
    {
        public const int Code = 47;

        public C2000Perimeter(IPort port) : base(port)
        {
            ModelCode = Code;
            Model = "С2000-Периметр";
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
