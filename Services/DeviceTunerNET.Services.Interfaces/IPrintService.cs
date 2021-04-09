using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface IPrintService
    {
        /// <summary>
        /// Print the label with information from LabelDict using the selected printer name
        /// </summary>
        /// <param name="PrinterName"></param>
        /// <param name="LabelType"></param>
        /// <param name="LabelDict"></param>
        /// <returns>"true" if label is printed</returns>
        public bool CommonPrintLabel(string PrinterName, int LabelType, Dictionary<string, string> LabelDict);

        /// <summary>
        /// Get available printers
        /// </summary>
        /// <returns>List of available printers</returns>
        public List<string> CommonGetAvailablePrinters();
    }
}
