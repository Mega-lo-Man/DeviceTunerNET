using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using OfficeOpenXml;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using static System.Int32;
using DeviceTunerNET.SharedDataModel.Devices;

namespace DeviceTunerNET.Services
{
    public class ExcelDataDecoder : IExcelDataDecoder
    {
        #region Constants
        private const char transparent = 'T';
        private const char master = 'M';
        private const char slave = 'S';
        #endregion Constants

        private int IPaddressCol = 0; // Index of the column containing device addresses
        private int RS485addressCol = 0; // Index of the column containing device addresses
        private int RS232addressCol = 0; // Index of the column containing device addresses
        private int nameCol = 0;    // Index of the column containing device names
        private int serialCol = 0;  // Index of the column containing device serial number
        private int modelCol = 0;   // Index of the column containing device model
        private int parentCol = 0;   // Index of the column containing parent cabinet
        private int CaptionRow = 1; // Table caption row index
        private int rangCol = 0; // Index of the column containing networkRelationship (master, slave, Transparent)
        private int qcCol = 0; // Index of the column contaning quality control passed mark

        private const string ColIPAddressCaption = "IP"; //Заголовок столбца с IP-адресами
        private const string ColRS485AddressCaption = "RS485"; //Заголовок столбца с адресами RS485
        private const string ColRS232AddressCaption = "RS232"; //Заголовок столбца с адресами RS232
        private const string ColNamesCaption = "Обозначение"; //Заголовок столбца с обозначениями приборов
        private const string ColSerialCaption = "Серийный номер"; //Заголовок столбца с обозначениями приборов
        private const string ColModelCaption = "Модель"; //Заголовок столбца с наименованием модели прибора
        private const string ColParentCaption = "Шкаф"; //Заголовок столбца с наименованием шкафа в котором находится дивайс
        private const string ColNetRelationship = "Rang"; //Заголовок столбца с мастерами и слевами C2000-Ethernet
        private const string ColQualityControl = "QC"; //Заголовок столбца о прохождении шкафом ОТК

        private const string qcPassed = "Passed";
        private const string qcDidntPass = "Failed!";


        private ExcelPackage package;
        private FileInfo sourceFile;
        private ExcelWorksheet worksheet;
        int rows; // number of rows in the sheet
        int columns;//number of columns in the sheet

        //Dictionary with all found C2000-Ethernet
        private Dictionary<C2000Ethernet, Tuple<char, int>> dictC2000Ethernet = new Dictionary<C2000Ethernet, Tuple<char, int>>();

        private readonly IDeviceGenerator _devicesGenerator;

        public ExcelDataDecoder(IDeviceGenerator deviceGenerator)
        {
            _devicesGenerator = deviceGenerator;
            // Remove "IBM437 is not a supported encoding" error
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public List<Cabinet> GetCabinetsFromExcel(string excelFileFullPath)
        {
            ExcelInit(excelFileFullPath);

            //Определяем в каких столбцах находятся обозначения приборов и их адреса
            FindColumnIndexesByHeader();

            return GetCabinetContent();
        }

        private List<Cabinet> GetCabinetContent()
        {
            var cabinetsLst = new List<Cabinet>();
            var cabinet = new Cabinet();
            var lastDevParent = "";
            for (var rowIndex = CaptionRow + 1; rowIndex <= rows; rowIndex++)
            {
                TryParse(worksheet.Cells[rowIndex, RS232addressCol].Value?.ToString(), out var devRS232Addr);
                TryParse(worksheet.Cells[rowIndex, RS485addressCol].Value?.ToString(), out var devRS485Addr);

                var deviceDataSet = new DeviceDataSet
                {
                    Id = rowIndex,
                    DevParent = worksheet.Cells[rowIndex, parentCol].Value?.ToString(),
                    DevName = worksheet.Cells[rowIndex, nameCol].Value?.ToString(),
                    DevModel = worksheet.Cells[rowIndex, modelCol].Value?.ToString(),
                    DevIPAddr = worksheet.Cells[rowIndex, IPaddressCol].Value?.ToString(),
                    DevSerial = worksheet.Cells[rowIndex, serialCol].Value?.ToString(),
                    DevRang = worksheet.Cells[rowIndex, rangCol].Value?.ToString(),
                    DevRS232Addr = devRS232Addr,
                    DevRS485Addr = devRS485Addr,
                    DevQcPassed = GetQcStatus(worksheet.Cells[rowIndex, qcCol].Value?.ToString())
                };

                if (!string.Equals(deviceDataSet.DevParent, lastDevParent)) // Если новый шкаф - сохранить старый в список шкафов
                {
                    if (rowIndex != CaptionRow + 1) 
                        cabinetsLst.Add(cabinet); // первый шкаф надо сначала наполнить а потом добавлять в cabinetsLst
                    
                    cabinet = new Cabinet
                    {
                        Designation = deviceDataSet.DevParent
                    };
                }

                if(_devicesGenerator.TryGetDevice(deviceDataSet.DevModel, out var device))
                {
                    var deviceWithSettings = GetDeviceWithSettings(device, deviceDataSet);
                    deviceWithSettings.Cabinet = cabinet.Designation;
                    cabinet.AddItem(deviceWithSettings);
                }
                               
                if (rowIndex == rows) // В последней строчке таблицы надо добавить последний шкаф в список шкафов, иначе (исходя из условия) он туда не попадёт
                {
                    cabinetsLst.Add(cabinet);
                }
                lastDevParent = deviceDataSet.DevParent;
            }
            FillDevicesDependencies(dictC2000Ethernet, master, slave);
            FillDevicesDependencies(dictC2000Ethernet, slave, master);
            return cabinetsLst;
        }

        private bool GetQcStatus(string qcStatus)
        {
            if(qcStatus != null && qcStatus.Equals(qcPassed))
            {
                return true;
            }
            return false;
        }

        // связываем все C2000-Ethernet в общую сеть, добавляя ссылки мастеров на слейв и прописывая мастеров в слейвы
        private void FillDevicesDependencies(Dictionary<C2000Ethernet, Tuple<char, int>> ethDevices, char dep1, char dep2)
        {
            foreach (var device in ethDevices)
            {
                switch (device.Value.Item1)
                {
                    case transparent:
                        device.Key.NetworkMode = C2000Ethernet.Mode.transparent; // Transparent
                        break;
                    case master:
                        device.Key.NetworkMode = C2000Ethernet.Mode.master; // master
                        break;
                    case slave:
                        device.Key.NetworkMode = C2000Ethernet.Mode.slave; // slave
                        break;
                }

                if (device.Value.Item1 != dep1)
                    continue;

                foreach (var item in ethDevices)
                {
                    if (item.Value.Item1 != dep2 || device.Value.Item2 != item.Value.Item2)
                        continue;

                    device.Key.RemoteDevicesList.Add(item.Key);
                    Debug.WriteLine(item.Key.AddressIP + " добавлен в " + device.Key.AddressIP + " (" + item.Value.Item2 + ")");
                }
            }
            Debug.WriteLine("----------------");
        }

        private void ExcelInit(string filepath)
        {
            sourceFile = new FileInfo(filepath);
            package = new ExcelPackage(sourceFile);

            worksheet = package.Workbook.Worksheets["Адреса"];
            /*
            worksheet.Cells[1, 1].Value = "tytytyty";
            worksheet.Cells["A2"].Value = "opopopop";
            */
            // get number of rows and columns in the sheet
            rows = worksheet.Dimension.Rows; // 20
            columns = worksheet.Dimension.Columns; // 7

        }

        private RS485device GetDeviceWithSettings(RS485device device, DeviceDataSet settings)
        {
            if (device is EthernetSwitch ethernetSwitch)
            {
                ethernetSwitch.Id = settings.Id;
                ethernetSwitch.Designation = settings.DevName;
                ethernetSwitch.Model = settings.DevModel;
                ethernetSwitch.Serial = settings.DevSerial;
                ethernetSwitch.AddressIP = settings.DevIPAddr;
                ethernetSwitch.QualityControlPassed = settings.DevQcPassed;

                return ethernetSwitch;
            }

            if (device is C2000Ethernet C2000Ethernet)
            {
                C2000Ethernet.Id = settings.Id;
                C2000Ethernet.Designation = settings.DevName;
                C2000Ethernet.Model = settings.DevModel;
                C2000Ethernet.Serial = settings.DevSerial;
                C2000Ethernet.AddressRS485 = (uint)settings.DevRS485Addr;
                C2000Ethernet.AddressRS232 = settings.DevRS232Addr;
                C2000Ethernet.AddressIP = settings.DevIPAddr;
                C2000Ethernet.NetName = settings.DevName;
                C2000Ethernet.QualityControlPassed = settings.DevQcPassed;

                return C2000Ethernet;
            }

            device.Id = settings.Id;
            device.Designation = settings.DevName;
            device.Model = settings.DevModel;
            device.Serial = settings.DevSerial;
            device.AddressRS485 = (uint)settings.DevRS485Addr;
            device.QualityControlPassed = settings.DevQcPassed;

            return device;
        }

        private void FindColumnIndexesByHeader()
        {
            for (var colIndex = 1; colIndex <= columns; colIndex++)
            {
                var content = worksheet.Cells[CaptionRow, colIndex].Value?.ToString();

                if (content == ColNamesCaption) { nameCol = colIndex; }
                if (content == ColIPAddressCaption) { IPaddressCol = colIndex; }
                if (content == ColRS485AddressCaption) { RS485addressCol = colIndex; }
                if (content == ColRS232AddressCaption) { RS232addressCol = colIndex; }
                if (content == ColSerialCaption) { serialCol = colIndex; }
                if (content == ColModelCaption) { modelCol = colIndex; }
                if (content == ColParentCaption) { parentCol = colIndex; }
                if (content == ColNetRelationship) { rangCol = colIndex; }
                if (content == ColQualityControl) { qcCol = colIndex; }
            }
        }

        public bool SaveSerialNumber(int id, string serialNumber)
        {
            // записываем серийник коммутатора в графу "Серийный номер" напротив номера строки указанного в id
            worksheet.Cells[id, serialCol].Value = serialNumber;

            return saveCurrentPackage();
        }

        public bool SaveQualityControlPassed(int id, bool qualityControlPassed)
        {
            // записываем метку прохождения прохождения контроля качества в графу "QC" напротив номера строки указанного в id
            if (qualityControlPassed)
            {
                worksheet.Cells[id, qcCol].Style.Font.Color.SetColor(Color.Black);
                worksheet.Cells[id, qcCol].Value = qcPassed;
            }
            else
            {
                worksheet.Cells[id, qcCol].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells[id, qcCol].Value = qcDidntPass;
            }

            return saveCurrentPackage();
        }

        private bool saveCurrentPackage()
        {
            try
            {
                package.Save();
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
    
    internal class DeviceDataSet
    {
        internal int Id { get; set; }
        internal string DevParent { get; set; } = "";
        internal string DevName { get; set; } = "";
        internal string DevModel { get; set; } = "";
        internal string DevIPAddr { get; set; } = "";
        internal string DevSerial { get; set; } = "";
        internal string DevRang { get; set; } = "";
        internal int DevRS232Addr { get; set; }
        internal int DevRS485Addr { get; set; }
        internal bool DevQcPassed { get; set; }
    }
}
