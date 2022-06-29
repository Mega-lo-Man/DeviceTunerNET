using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface ITftpServerManager
    {
        /// <summary>
        /// Start TFTP server 
        /// </summary>
        /// <param name="shareDirectory">Shared directory</param>
        void Start(string shareDirectory);

        void Stop();

    }
}
