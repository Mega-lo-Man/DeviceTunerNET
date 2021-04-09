using DeviceTunerNET.SharedDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface IExcelDataDecoder
    {
        List<Cabinet> GetCabinetsFromExcel(string ExcelFileFullPath);
        //bool SaveDevice<T>(T arg) where T : SimplestСomponent;
        bool SaveSerialNumber(int id, string serialNumber);
    }
}
