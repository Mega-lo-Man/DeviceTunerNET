using DeviceTunerNET.SharedDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface IUploadManager
    {
        public string TemplateConfigPath { get; set; }
        public string TargetConfigPath { get; set; }
        public Dictionary<string, string> ReplaceableVariables { get; set; }

        public bool UploadConfig(EthernetSwitch ethernetSwitch, CancellationToken token);

    }
}
