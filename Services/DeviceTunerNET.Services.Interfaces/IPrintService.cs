using System.Collections.Generic;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface IPrintService
    {
        /// <summary>
        /// Print the label with information from LabelDict using the selected printer name
        /// </summary>
        /// <param name="PrinterName"></param>
        /// <param name="LabelPath"></param>
        /// <param name="LabelDict"></param>
        /// <returns>"true" if label is printed</returns>
        public bool CommonPrintLabel(string PrinterName, string LabelPath, Dictionary<string, string> LabelDict);

        /// <summary>
        /// Get available printers
        /// </summary>
        /// <returns>List of available printers</returns>
        public List<string> CommonGetAvailablePrinters();
    }
}
