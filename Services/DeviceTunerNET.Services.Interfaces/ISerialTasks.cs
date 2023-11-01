using DeviceTunerNET.SharedDataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface ISerialTasks
    {
        public enum ResultCode
        {
            ok = 1,
            undefinedError = 0,
            deviceTypeMismatch = -1,
            addressFieldNotValid = -2,
            deviceNotRespond = -3,
            errorConfigDownload = -4,
            comPortBusy = -5
        }

        public ResultCode ShiftDevicesAddresses(string comPort, int startAddress, int targetAddress, int range);

        public ObservableCollection<string> GetAvailableCOMPorts();
    }
}
