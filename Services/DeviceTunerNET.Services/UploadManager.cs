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
    public class UploadManager : IUploadManager

    {
        public string TemplateConfigPath { get; set; }
        public string TargetConfigPath { get; set; }
        public Dictionary<string, string> ReplaceableVariables { get; set; }

        private string ServerDirectory;

        private bool GenerateConfig()
        {
            ServerDirectory = Path.GetDirectoryName(TargetConfigPath);
            var configParser = new ConfigParser();
            var result1 = configParser.Parse(ReplaceableVariables, TemplateConfigPath, TargetConfigPath);
            
            return false;
        }


        private bool TftpStartup()
        {
            var server = new TftpServer();
            
            server.OnReadRequest += new TftpServerEventHandler(server_OnReadRequest);
            server.OnWriteRequest += new TftpServerEventHandler(server_OnWriteRequest);
            server.Start();
            return true;
            //Console.Read();
            
        }

        public bool UploadConfig(EthernetSwitch ethernetSwitch, CancellationToken token)
        {
            GenerateConfig();
            var result2 = TftpStartup();
            var result3 = Send;
            var result4

        }




        private void server_OnWriteRequest(ITftpTransfer transfer, EndPoint client)
        {
            var file = Path.Combine(ServerDirectory, transfer.Filename);

            if (File.Exists(file))
            {
                CancelTransfer(transfer, TftpErrorPacket.FileAlreadyExists);
            }
            else
            {
                OutputTransferStatus(transfer, "Accepting write request from " + client);
                StartTransfer(transfer, new FileStream(file, FileMode.CreateNew));
            }
        }

        private void server_OnReadRequest(ITftpTransfer transfer, EndPoint client)
        {
            var path = Path.Combine(ServerDirectory, transfer.Filename);
            var file = new FileInfo(path);

            //Is the file within the server directory?
            if (!file.FullName.StartsWith(ServerDirectory, StringComparison.InvariantCultureIgnoreCase))
            {
                CancelTransfer(transfer, TftpErrorPacket.AccessViolation);
            }
            else if (!file.Exists)
            {
                CancelTransfer(transfer, TftpErrorPacket.FileNotFound);
            }
            else
            {
                OutputTransferStatus(transfer, "Accepting request from " + client);
                StartTransfer(transfer, new FileStream(file.FullName, FileMode.Open, FileAccess.Read));
            }
        }

        private static void StartTransfer(ITftpTransfer transfer, Stream stream)
        {
            transfer.OnProgress += new TftpProgressHandler(transfer_OnProgress);
            transfer.OnError += new TftpErrorHandler(transfer_OnError);
            transfer.OnFinished += new TftpEventHandler(transfer_OnFinished);
            transfer.Start(stream);
        }

        private static void CancelTransfer(ITftpTransfer transfer, TftpErrorPacket reason)
        {
            OutputTransferStatus(transfer, "Cancelling transfer: " + reason.ErrorMessage);
            transfer.Cancel(reason);
        }

        static void transfer_OnError(ITftpTransfer transfer, TftpTransferError error)
        {
            OutputTransferStatus(transfer, "Error: " + error);
        }

        static void transfer_OnFinished(ITftpTransfer transfer)
        {
            OutputTransferStatus(transfer, "Finished");
        }

        static void transfer_OnProgress(ITftpTransfer transfer, TftpTransferProgress progress)
        {
            OutputTransferStatus(transfer, "Progress " + progress);
        }

        private static void OutputTransferStatus(ITftpTransfer transfer, string message)
        {
            Console.WriteLine("[" + transfer.Filename + "] " + message);
        }
    }
}
