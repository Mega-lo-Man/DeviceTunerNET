using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using Tftp.Net;
using System.Net;

namespace DeviceTunerNET.Services
{
    public class UploadSwitchManager //: IUploadManager

    {
        private readonly IConfigParser _configParser;
        private readonly ITftpServerManager _tftpServerManager;

        public string TemplateConfigPath { get; set; }
        public string TargetConfigPath { get; set; }
        public Dictionary<string, string> ReplaceableVariables { get; set; }
        public string ServerDirectory { get; private set; }

        public UploadSwitchManager(IConfigParser configParser, ITftpServerManager tftpServerManager)
        {
            _configParser = configParser;
            _tftpServerManager = tftpServerManager;
        }

        private bool GenerateConfig()
        {
            ServerDirectory = Path.GetDirectoryName(TargetConfigPath);
            var result1 = _configParser.Parse(ReplaceableVariables, TemplateConfigPath, TargetConfigPath);
            
            return false;
        }

        public EthernetSwitch UploadConfig(EthernetSwitch ethernetSwitch, CancellationToken token)
        {
            GenerateConfig();
            _tftpServerManager.Start(ServerDirectory);
            var result3 = Send(ethernetSwitch, token);
            _tftpServerManager.Stop();
            //var result4
            return result3;
        }

        private EthernetSwitch Send(EthernetSwitch ethernetSwitch, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
