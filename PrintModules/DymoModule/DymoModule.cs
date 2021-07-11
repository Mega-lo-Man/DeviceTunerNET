using DeviceTunerNET.Services.Interfaces;
using Microsoft.Win32;
using System.Collections.Generic;

namespace DeviceTunerNET.DymoModules
{
    public class DymoModule : IPrintService
    {
        /*
        private IEnumerable<DymoSDK.Interfaces.IPrinter> Printers;
        private List<DymoSDK.Interfaces.ILabelObject> LabelObjects;
        private DymoSDK.Implementations.DymoLabel dymoSDKLabel;
        private string SelectedRoll;

        public DymoModule()
        {
            Printers = DymoPrinter.Instance.GetPrinters();
            dymoSDKLabel = new DymoLabel();
            
        }

        private string _labelSwitchFileName = "Resources\\Files\\label25x25switch.dymo";
        public string LabelSwitchFileName
        { 
            get => _labelSwitchFileName;
            set => _labelSwitchFileName = value;
        }

        public List<string> CommonGetAvailablePrinters()
        {
            List<string> listNames = new List<string>();
            
            foreach(IPrinter printer in Printers)
            {
                listNames.Add(printer.Name);
            }
            return listNames;
        }

        public bool CommonPrintLabel(string PrinterName, int LabelType, Dictionary<string, string> LabelDict)
        {
            int copies = 1;
            if (LabelType == 0)
            {
                //Load label from file path
                dymoSDKLabel.LoadLabelFromFilePath(LabelSwitchFileName);
                //Get object names list
                LabelObjects = dymoSDKLabel.GetLabelObjects().ToList();
                if (PrinterName != null)
                {
                    bool barcodeOrGraphsquality = false;

                    //Default quality is TEXT
                    //Setting barcodeGraphsQuality will improve printing quality being easier to read Barcode or QRcode objects
                    if (ContainsBarcodeOrGraphObjects())
                    {
                        barcodeOrGraphsquality = true;
                    }

                    MappingDictionaryToLabel(LabelDict);

                    //Send to print
                    if (PrinterName.Contains("Twin Turbo"))
                    {
                        //0: Auto, 1: Left roll, 2: Right roll
                        int rollSel = SelectedRoll == "Auto" ? 0 : SelectedRoll == "Left" ? 1 : 2;

                        DymoPrinter.Instance.PrintLabel(dymoSDKLabel, PrinterName, copies, rollSelected: rollSel, barcodeGraphsQuality: barcodeOrGraphsquality);
                    }
                    else
                    {
                        DymoPrinter.Instance.PrintLabel(dymoSDKLabel, PrinterName, copies, barcodeGraphsQuality: barcodeOrGraphsquality);
                    }
                }
                return true;
            }
            return false;
        }

        private bool MappingDictionaryToLabel(Dictionary<string, string> dict)
        {
            foreach(KeyValuePair<string, string> entry in dict)
            {

                dymoSDKLabel.UpdateLabelObject(LabelObjects.Find(x => x.Name.Equals(entry.Key)), entry.Value);
            }
            
            return true;
        }

        /// <summary>
        /// Validate if label contains Barcode or QRCode objects
        /// </summary>
        /// <returns>True/False</returns>
        private bool ContainsBarcodeOrGraphObjects()
        {
            foreach (var obj in LabelObjects)
            {
                if (obj.Type == DymoSDK.Interfaces.TypeObject.BarcodeObject || obj.Type == DymoSDK.Interfaces.TypeObject.QRCodeObject)
                {
                    return true;
                }
            }
            return false;
        }
        */
        private const string _availablePrintersRegPath = "SOFTWARE\\DymoModule\\AvaliablePrinters";
        private const string _DymoModuleExecutorPath = @"C:\Users\texvi\source\repos\DymoModule\DymoModule\bin\Debug\DymoModule.exe";
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
            if (StartExternalApp(_DymoModuleExecutorPath, GetCommand(_cmdGetPrinters, "1")))
            {
                var regBranch = Registry.CurrentUser;
                if (regBranch.OpenSubKey(_availablePrintersRegPath) != null)
                {
                    var key = regBranch.OpenSubKey(_availablePrintersRegPath);
                    var valueNames = key.GetValueNames();
                    foreach (var item in valueNames)
                    {
                        availablePrinters.Add(item);
                    }
                }
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
            System.Diagnostics.Process installProcess = new System.Diagnostics.Process
            {
                StartInfo = {FileName = installApp, Arguments = installArgs}
            };
            //settings up parameters for the install process

            installProcess.Start();

            installProcess.WaitForExit();
            // Check for successful completion
            return (installProcess.ExitCode == APP_GENERATE_SUCCESS) ? true : false;
        }

        private string GetCommand(string commandName, string commandValue)
        {
            return " \"" + commandName + separateString + commandValue + "\"";
        }
    }
}
