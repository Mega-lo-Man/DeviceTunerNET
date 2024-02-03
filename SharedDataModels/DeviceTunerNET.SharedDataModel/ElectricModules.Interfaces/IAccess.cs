using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.ElectricModules.Interfaces
{
    public interface IAccess
    {
        AccessController Access { get; set; }
    }
}
