using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.SharedDataModel
{
    public interface ITransaction
    {
        public byte[] Transaction(byte address, byte[] sendArray);
    }
}
