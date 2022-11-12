using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.SharedDataModel.ElectricModules.Interfaces
{
    internal interface IShleifs
    {
        public IEnumerable<Shleif> Shleifs { get; set; }
    }
}
