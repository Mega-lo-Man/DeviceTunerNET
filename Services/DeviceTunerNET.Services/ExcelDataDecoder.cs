using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using static System.Int32;

namespace DeviceTunerNET.Services
{
    public class ExcelDataDecoder : IExcelDataDecoder
    {
        #region Constants
        private const char transparent = 'T';
        private const char master = 'M';
        private const char slave = 'S';
        #endregion Constants

        private int IPaddressCol = 0; // Index of column that containing device addresses
        private int RS485addressCol = 0; // Index of column that containing device addresses
        private int RS232addressCol = 0; // Index of column that containing device addresses
        private int nameCol = 0;    // Index of column that containing device names
        private int serialCol = 0;  // Index of column that containing device serial number
        private int modelCol = 0;   // Index of column that containing device model
        private int parentCol = 0;   // Index of column that containing parent cabinet
        private int CaptionRow = 1; //Table caption row index
        private int rangCol = 0; //Index of column that containing networkRelationship (master, slave, transparent)

        private string ColIPAddressCaption = "IP"; //Заголовок столбца с IP-адресами
        private string ColRS485AddressCaption = "RS485"; //Заголовок столбца с адресами RS485
        private string ColRS232AddressCaption = "RS232"; //Заголовок столбца с адресами RS232
        private string ColNamesCaption = "Обозначение"; //Заголовок столбца с обозначениями приборов
        private string ColSerialCaption = "Серийный номер"; //Заголовок столбца с обозначениями приборов
        private string ColModelCaption = "Модель"; //Заголовок столбца с наименованием модели прибора
        private string ColParentCaption = "Шкаф"; //Заголовок столбца с наименованием шкафа в котором находится дивайс
        private string ColNetRelationship = "Rang"; //Заголовок столбца с мастерами и слевами C2000-Ethernet

        private ExcelPackage package;
        private FileInfo sourceFile;
        private ExcelWorksheet worksheet;
        int rows; // number of rows in the sheet
        int columns;//number of columns in the sheet

        //Dictionary with all found C2000-Ethernet
        private Dictionary<C2000Ethernet, Tuple<char, int>> dictC2000Ethernet = new Dictionary<C2000Ethernet, Tuple<char, int>>();

        public ExcelDataDecoder()
        {
            // Remove "IBM437 is not a supported encoding" error
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        private int GetDeviceType(string DevModel)
        {
            var devType = 0;

            if (DevModel.Contains("MES3508"))
                devType = 1;
            if (DevModel.Contains("MES2308"))
                devType = 1;
            if (DevModel.Contains("MES2324"))
                devType = 1;
            if (DevModel.Contains("2000-Ethernet"))
                devType = 2;

            return devType;
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
            var lastDevParent = ""; //= worksheet.Cells[CaptionRow + 1, parentCol].Value?.ToString();;
            for (var rowIndex = CaptionRow + 1; rowIndex <= rows; rowIndex++)
            {
                var devParent = worksheet.Cells[rowIndex, parentCol].Value?.ToString();
                var devName = worksheet.Cells[rowIndex, nameCol].Value?.ToString();
                var devModel = worksheet.Cells[rowIndex, modelCol].Value?.ToString();
                var devIPAddr = worksheet.Cells[rowIndex, IPaddressCol].Value?.ToString();
                var devSerial = worksheet.Cells[rowIndex, serialCol].Value?.ToString();
                var devRang = worksheet.Cells[rowIndex, rangCol].Value?.ToString();

                TryParse(worksheet.Cells[rowIndex, RS232addressCol].Value?.ToString(), out int devRS232Addr);
                TryParse(worksheet.Cells[rowIndex, RS485addressCol].Value?.ToString(), out int devRS485Addr);

                if (!string.Equals(devParent, lastDevParent)) // Если новый шкаф - сохранить старый в список шкафов
                {
                    if (rowIndex != CaptionRow + 1) 
                        cabinetsLst.Add(cabinet); // первый шкаф надо сначала наполнить а потом добавлять в cabinetsLst
                    
                    cabinet = new Cabinet
                    {
                        Designation = devParent
                    };
                }

                switch (GetDeviceType(devModel))
                {
                    case 0:
                        cabinet.AddItem(new RS485device
                        {
                            Id = rowIndex,
                            Designation = devName,
                            Model = devModel,
                            Serial = devSerial,
                            AddressRS485 = devRS485Addr
                        }); ;
                        break;
                    case 1:
                        cabinet.AddItem(new EthernetSwitch
                        {
                            Id = rowIndex,
                            Designation = devName,
                            Model = devModel,
                            Serial = devSerial,
                            AddressIP = devIPAddr,
                            Cabinet = cabinet.Designation
                        });
                        break;
                    case 2:

                        var c2000Ethernet = new C2000Ethernet
                        {
                            Id = rowIndex,
                            Designation = devName,
                            Model = devModel,
                            Serial = devSerial,
                            AddressRS485 = devRS485Addr,
                            AddressRS232 = devRS232Addr,
                            AddressIP = devIPAddr,

                        };
                        //Add to dict for master/slave/translate sort
                        dictC2000Ethernet.Add(c2000Ethernet, GetRangTuple(devRang));
                        //Add to Cabinet
                        cabinet.AddItem(c2000Ethernet);
                        break;
                }
                if (rowIndex == rows) // В последней строчке таблицы надо добавить последний шкаф в список шкафов, иначе (исходя из условия) он туда не попадёт
                {
                    cabinetsLst.Add(cabinet);
                }
                lastDevParent = devParent;
            }
            FillDevicesDependencies(dictC2000Ethernet, master, slave);
            FillDevicesDependencies(dictC2000Ethernet, slave, master);
            return cabinetsLst;
        }

        private Tuple<char, int> GetRangTuple(string rang)
        {
            var _rang = rang[0];
            var lineStr = rang.Substring(1); //right part of rang

            if (_rang != master && _rang != slave && _rang != transparent)
                return null;

            if (!TryParse(lineStr, out var lineNumb)) 
                return null;

            return new Tuple<char, int>(_rang, lineNumb);
        }

        private void FillDevicesDependencies(Dictionary<C2000Ethernet, Tuple<char, int>> ethDevices, char dep1, char dep2)
        {
            foreach (var device in ethDevices)
            {
                if (device.Value.Item1 == transparent)
                {
                    device.Key.NetworkMode = 0; // transparent
                }
                if (device.Value.Item1 == master)
                {
                    device.Key.NetworkMode = 1; // master
                }
                if (device.Value.Item1 == slave)
                {
                    device.Key.NetworkMode = 2; // slave
                }

                if (device.Value.Item1 != dep1)
                    continue;

                foreach (var item in ethDevices)
                {
                    if (item.Value.Item1 != dep2 || device.Value.Item2 != item.Value.Item2)
                        continue;

                    device.Key.ListOfRemoteDevices.Add(item.Key);
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
            }
        }

        public bool SaveSerialNumber(int id, string serialNumber)
        {
            SaveSerialById(id, serialNumber);
            return true;
        }
        
        private void SaveSerialById(int id, string serialNumber)
        {
            // записываем серийник коммутатора в графу "Серийный номер" напротив номера строки указанного в id
            worksheet.Cells[id, serialCol].Value = serialNumber;
            package.Save();
        }
    }
}
