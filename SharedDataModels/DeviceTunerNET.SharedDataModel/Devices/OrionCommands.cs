using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public enum OrionCommands : byte
    {
        GetModel = 0x0D,
        ChangeAddress = 0x0F,
        WriteToDeviceMemoryMap = 0x41
    }
}
