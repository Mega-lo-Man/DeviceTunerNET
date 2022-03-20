using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface INetworkUtils
    {
        public bool SendPing(string IpAddress);
        public bool SendMultiplePing(string NewIPAddr, int NumberOfRepetitions);

    }
}
