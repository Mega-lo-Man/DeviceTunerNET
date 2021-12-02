using DeviceTunerNET.Services.Interfaces;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;

namespace DeviceTunerNET.DymoModules
{
    public class DymoModule : IPrintService
    {
        private const string _availablePrintersRegPath = "SOFTWARE\\DymoModule\\AvaliablePrinters";
        private const string _DymoModuleExecutorPath = @"C:\Programs\DymoModule\DymoModule\bin\Debug\DymoModule.exe";
        private const int APP_GENERATE_SUCCESS = 100;

        #region Commands
        private const string _cmdPrinterName = "Command_PrinterName";
        private const string _cmdGetPrinters = "Command_GetPrinters";
        private const string _cmdPrintLabel = "Command_PrintLabel";

        #endregion Commands

        private const char separateString = '=';


        public DymoModule()
        {

        }

        public List<string> CommonGetAvailablePrinters()
        {
            var availablePrinters = new List<string>();
            // Второй параметр в GetCommand при _cmdGetPrinters может быть любой строкой
            if (!StartExternalApp(_DymoModuleExecutorPath, GetCommand(_cmdGetPrinters, "1"))) 
                return availablePrinters;
            var regBranch = Registry.CurrentUser;
            if (regBranch.OpenSubKey(_availablePrintersRegPath) == null) 
                return availablePrinters;
            var key = regBranch.OpenSubKey(_availablePrintersRegPath);
            var valueNames = key.GetValueNames();
            foreach (var item in valueNames)
            {
                availablePrinters.Add(item);
            }
            return availablePrinters;
        }

        public bool CommonPrintLabel(string printerName, string labelPath, Dictionary<string, string> labelDict)
        {
            var _printerName = printerName;
            var _labelDict = labelDict;
            var _labelPath = labelPath;



            GetCommand(_cmdPrinterName, _printerName);


            var resultArgsString = GetCommand(_cmdPrintLabel, _labelPath) +
                                   GetCommand(_cmdPrinterName, _printerName) +
                                   GetLabelArguments(_labelDict);

            return StartExternalApp(_DymoModuleExecutorPath, resultArgsString);
        }

        private string GetLabelArguments(Dictionary<string, string> labelDict)
        {
            var result = "";
            foreach (var item in labelDict)
            {
                var pair = " \"" + item.Key + separateString + item.Value + "\"";
                result += pair;
            }
            return result;
        }

        public static bool StartExternalApp(string installApp, string installArgs)
        {
            //settings up parameters for the install process
            var installProcess = new System.Diagnostics.Process
            {
                StartInfo = {FileName = installApp, Arguments = installArgs}
            };
            
            try
            {
                installProcess.Start();
                installProcess.WaitForExit();
                // Check for successful completion
                return (installProcess.ExitCode == APP_GENERATE_SUCCESS);
            }
            catch
            {
                MessageBox.Show("Print module: \"" + _DymoModuleExecutorPath + "\" not found.");
                return false;
            }
        }

        private string GetCommand(string commandName, string commandValue)
        {
            return " \"" + commandName + separateString + commandValue + "\"";
        }
    }
}
