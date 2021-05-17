using DeviceTunerNET.Services.Interfaces;
using DymoSDK.Implementations;
using DymoSDK.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.DymoModules
{
    public class DymoModule : IPrintService
    {
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
    }
}
