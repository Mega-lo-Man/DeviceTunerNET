using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface IAuthLoader
    {
        IEnumerable<string> AvailableServicesNames { get; set; }
        Task<IEnumerable<string>> GetAvailableServices();
    }
}
