using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.Services.SwitchesStrategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services
{
    public class StrategiesFactory : IStrategiesFactory
    {
        private readonly IEnumerable<System.Type> _availableStrategies;

        public StrategiesFactory(IEnumerable<System.Type> availableStrategies)
        {
            _availableStrategies = availableStrategies;
        }

        public StrategiesFactory()
        {
            _availableStrategies = new List<System.Type>() { typeof(Eltex) };
        }

        public ISwitchConfigUploader GetInstance<T>() where T : ISwitchConfigUploader =>
            (ISwitchConfigUploader)Activator.CreateInstance(typeof(T));
    }
}
