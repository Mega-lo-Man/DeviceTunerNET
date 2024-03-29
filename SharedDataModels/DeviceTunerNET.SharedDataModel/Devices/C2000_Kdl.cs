﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000_Kdl : OrionDevice
    {
        public new const int Code = 9;
        public C2000_Kdl(IPort port) : base(port)
        {
            Model = "С2000-КДЛ";
            SupportedModels = new List<string>
            {
                Model,
            };
        }
    }
}
