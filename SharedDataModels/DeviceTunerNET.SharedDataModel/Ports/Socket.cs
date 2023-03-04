using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Ports
{
    public class Socket : IPort
    {
        public int MaxRepetitions { get ; set ; }
        public int Timeout { get; set ; }

        public byte[] Send(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
