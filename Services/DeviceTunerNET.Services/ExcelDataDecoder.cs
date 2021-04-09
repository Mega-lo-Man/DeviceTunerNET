using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using System;
using System.Collections.Generic;
using System.Text;
using OfficeOpenXml;
using System.IO;
using System.Net;
using System.CodeDom;

namespace DeviceTunerNET.Services
{
    public class ExcelDataDecoder : IExcelDataDecoder
    {
        private int IPaddressCol = 0; // Index of column that containing device addresses
        private int RS485addressCol = 0; // Index of column that containing device addresses
        private int RS232addressCol = 0; // Index of column that containing device addresses
        private int nameCol = 0;    // Index of column that containing device names
        private int serialCol = 0;  // Index of column that containing device serial number
        private int modelCol = 0;   // Index of column that containing device model
        private int parentCol = 0;   // Index of column that containing parent cabinet
        private int CaptionRow = 1; //Table caption row index

        private string ColIPAddressCaption = "IP"; //Заголовок столбца с IP-адресами
        private string ColRS485AddressCaption = "RS485"; //Заголовок столбца с адресами RS485
        private string ColRS232AddressCaption = "RS232"; //Заголовок столбца с адресами RS232
        private string ColNamesCaption = "Обозначение"; //Заголовок столбца с обозначениями приборов
        private string ColSerialCaption = "Серийный номер"; //Заголовок столбца с обозначениями приборов
        private string ColModelCaption = "Модель"; //Заголовок столбца с наименованием модели прибора
        private string ColParentCaption = "Шкаф"; //Заголовок столбца с наименованием шкафа в котором находится дивайс

        private ExcelPackage package;
        private FileInfo sourceFile;
        private ExcelWorksheet worksheet;
        int rows; // number of rows in the sheet
        int columns;//number of columns in the sheet

        public ExcelDataDecoder()
        {
            // Remove "IBM437 is not a supported encoding" error
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        private int GetDeviceType(string DevModel)
        {
            int devType = 0;
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
            List<Cabinet> cabinetsLst = new List<Cabinet>();
            Cabinet cabinet = new Cabinet();
            string lastDevParent = ""; //= worksheet.Cells[CaptionRow + 1, parentCol].Value?.ToString();;
            for (int rowIndex = CaptionRow + 1; rowIndex <= rows; rowIndex++)
            {
                string devParent = worksheet.Cells[rowIndex, parentCol].Value?.ToString();
                string devName = worksheet.Cells[rowIndex, nameCol].Value?.ToString();
                string devModel = worksheet.Cells[rowIndex, modelCol].Value?.ToString();
                string devIPAddr = worksheet.Cells[rowIndex, IPaddressCol].Value?.ToString();
                string devSerial = worksheet.Cells[rowIndex, serialCol].Value?.ToString();

                int.TryParse(worksheet.Cells[rowIndex, RS232addressCol].Value?.ToString(), out int devRS232Addr);
                int.TryParse(worksheet.Cells[rowIndex, RS485addressCol].Value?.ToString(), out int devRS485Addr);

                if (!string.Equals(devParent, lastDevParent)) // Если новый шкаф - сохранить старый в список шкафов
                {
                    if(!(rowIndex == CaptionRow + 1)) cabinetsLst.Add(cabinet); // первый шкаф надо сначала наполнить а потом добавлять в cabinetsLst
                    cabinet = new Cabinet
                    {
                        Designation = devParent
                    };
                }
                
                switch(GetDeviceType(devModel))
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
                            AddressIP = devIPAddr
                        });
                        break;
                    case 2:
                        cabinet.AddItem(new C2000Ethernet
                        {
                            Id = rowIndex,
                            Designation = devName,
                            Model = devModel,
                            Serial = devSerial,
                            AddressRS485 = devRS485Addr,
                            AddressRS232 = devRS232Addr,
                            AddressIP = devIPAddr
                        });
                        break;
                }
                if(rowIndex == rows) // В последней строчке таблицы надо добавить последний шкаф в список шкафов, иначе (исходя из условия) он туда не попадёт
                {
                    cabinetsLst.Add(cabinet);
                }
                lastDevParent = devParent;
            }
            return cabinetsLst;
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
            for (int colIndex = 1; colIndex <= columns; colIndex++)
            {
                string content = worksheet.Cells[CaptionRow, colIndex].Value?.ToString();
                if (content == ColNamesCaption) { nameCol = colIndex; }
                if (content == ColIPAddressCaption) { IPaddressCol = colIndex; }
                if (content == ColRS485AddressCaption) { RS485addressCol = colIndex; }
                if (content == ColRS232AddressCaption) { RS232addressCol = colIndex; }
                if (content == ColSerialCaption) { serialCol = colIndex; }
                if (content == ColModelCaption) { modelCol = colIndex; }
                if (content == ColParentCaption) { parentCol = colIndex; }
            }
        }

        public bool SaveSerialNumber(int id, string serialNumber)
        {
            int _id = id;
            string _serialNumber = serialNumber;
            SaveSerialById(_id, _serialNumber);
            return true;
        }

        /*public bool SaveDevice<T>(T arg) where T : SimplestСomponent
        {
            object someDevice = arg;
            if (typeof(T) == typeof(EthernetSwitch)) return SaveSwitchDevice((EthernetSwitch)someDevice);
            if (typeof(T) == typeof(RS485device)) return SaveRS485Device((RS485device)someDevice);
            return false;
        }

        private bool SaveRS485Device(RS485device rs485Device)
        {
            bool result = SaveSerialByAddress(rs485Device.AddressRS485.ToString(), rs485Device.Serial, RS485addressCol);
            // Recording other parameters
            return result;
        }

        private bool SaveSwitchDevice(EthernetSwitch ethernetSwitch)
        {
            bool result = SaveSerialByAddress(ethernetSwitch.AddressIP, ethernetSwitch.Serial, IPaddressCol);
            // Recording other parameters
            return result;
        }
        

        private bool SaveSerialByAddress(string address, string serial, int addrColumn)
        {
            //поиск в таблице строки которая содержит IP-адрес такой же как в networkDevice
            int? foundRow = SearchRowByCellValue(address, addrColumn);
            if (foundRow != null)
            {
                // записываем серийник коммутатора в графу "Серийный номер" напротив IP-адреса этого коммутатора
                worksheet.Cells[foundRow.Value, serialCol].Value = serial;
                package.Save();
                return true;
            }
            return true;
        }
*/

        private void SaveSerialById(int id, string serialNumber)
        {
            // записываем серийник коммутатора в графу "Серийный номер" напротив номера строки указанного в id
            worksheet.Cells[id, serialCol].Value = serialNumber;
            package.Save();
        }

        // Поиск номера строки к которой относится только что сконфигурированный дивайс
        // searchValue - что ищем, column - столбец в котором ищем
        /*
        private int? SearchRowByCellValue(string searchValue, int column)
        {
            //Return first entry
            for (int rowCounter = CaptionRow + 2; rowCounter <= rows; rowCounter++)
            {
                if(searchValue.Equals(worksheet.Cells[rowCounter, column].Value?.ToString())) return rowCounter;
            }
            return null;
        }
        */
    }
}
