using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface IMessageService
    {
        void SendMessage(string sms);
        string GetMessage();
        event System.Action<string> DataArrived;
    }
}
