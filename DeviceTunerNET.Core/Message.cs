using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Core
{
    public class Message
    {
        public int ActionCode { get; set; }
        public string MessageString { get; set; }
        public object AttachedObject { get; set; }
    }
}
