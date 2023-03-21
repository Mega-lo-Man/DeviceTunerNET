using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public enum OrionCommands : byte
    {
        IsOnline = 0x01,
        GetModel = 0x0D,
        ChangeAddress = 0x0F,
        WriteToDeviceMemoryMap = 0x41,
        Reboot = 0x17,
    }
}
