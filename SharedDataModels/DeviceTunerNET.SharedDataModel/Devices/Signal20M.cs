﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class Signal20M : Signal20P
    {
        public new const int Code = 26;

        public Signal20M(IPort port) : base(port)
        {
            Model = "Сигнал-20М";
            SupportedModels = new List<string>
            {
                Model,
            };
        }
    }
}
