using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.SharedDataModel.ElectricModules.Interfaces
{
    internal interface IRelays
    {
        public IEnumerable<MechanicalRelay> Relays{ get; set; }
    }
}
