using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface IDialogCaller
    {
        public string GetSerialNumber(string model, string designation);
        public void ShowMessage(string message);
    }
}
