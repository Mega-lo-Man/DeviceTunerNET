using DeviceTunerNET.Core;
using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services
{
    public class DataRepositoryService : IDataRepositoryService
    {
        private List<Cabinet> cabinetsLst = new List<Cabinet>();

        private IEventAggregator _ea;
        private IExcelDataDecoder _excelDataDecoder;

        private int _dataProviderType = 1;

        public DataRepositoryService(IEventAggregator ea, IExcelDataDecoder excelDataDecoder)
        {
            _ea = ea;
            _excelDataDecoder = excelDataDecoder;
        }
        /*
        private bool SaveSwitchDevice(EthernetSwitch ethernetSwitch)
        {
            //throw new NotImplementedException();
            if (_dataProviderType == 1) // Если источник данных - таблица Excel
            {
                _excelDataDecoder.SaveDevice(ethernetSwitch);
                return true;
            }
            return false;
        }

        private bool SaveRS485Device(RS485device rs485Device)
        {
            if (_dataProviderType == 1) // Если источник данных - таблица Excel
            {
                _excelDataDecoder.SaveDevice(rs485Device);
                return true;
            }
            return false;
        }
        */

        public void SetDevices(int DataProviderType, string FullPathToData)
        {
            _dataProviderType = DataProviderType;
            string _fullPathToData = FullPathToData;

            cabinetsLst.Clear();
            switch (_dataProviderType)
            {
                case 1:
                    cabinetsLst = _excelDataDecoder.GetCabinetsFromExcel(_fullPathToData);
                    break;
            }
            //Сообщаем всем об обновлении данных в репозитории
            _ea.GetEvent<MessageSentEvent>().Publish(new Message
            {
                ActionCode = MessageSentEvent.RepositoryUpdated
            });
        }

        public bool SaveSerialNumber(int id, string serialNumber)
        {
            int _id = id;
            string _serial = serialNumber;
            if (_dataProviderType == 1) // Если источник данных - таблица Excel
            {
                _excelDataDecoder.SaveSerialNumber(_id, _serial);
                return true;
            }
            return false;
        }

        /*public bool SaveDevice<T>(T arg) where T : SimplestСomponent
        {
            object someDevice = arg;
            if (typeof(T) == typeof(EthernetSwitch)) return SaveSwitchDevice((EthernetSwitch)someDevice);
            if (typeof(T) == typeof(RS485device)) return SaveRS485Device((RS485device)someDevice);
            return false;
        }*/

        public IList<Cabinet> GetCabinetsWithTwoTypeDevices<T1, T2>()
            where T1 : SimplestСomponent
            where T2 : SimplestСomponent
        {
            List<Cabinet> CabinetsWithdevs = new List<Cabinet>();
            foreach (Cabinet cabinet in cabinetsLst)
            {
                List<T1> devicesListT1 = (List<T1>)cabinet.GetDevicesList<T1>();
                List<T2> devicesListT2 = (List<T2>)cabinet.GetDevicesList<T2>();
                 
                // Будем работать только с теми шкафами в которых есть приборы типа T1 или T2
                if (devicesListT1.Count > 0 || devicesListT2.Count > 0)
                {
                    // В возвращаемом из метода списке будем создавать новые шкафы
                    Cabinet newCabinet = new Cabinet
                    {
                        Designation = cabinet.Designation,
                        DeviceType = cabinet.DeviceType
                    };

                    foreach (T1 item in devicesListT1)
                    {
                        newCabinet.AddItem(item);
                    }

                    foreach (T2 item in devicesListT2)
                    {
                        newCabinet.AddItem(item);
                    }
                    CabinetsWithdevs.Add(newCabinet);
                }
            }
            return CabinetsWithdevs;
        }

        public IList<Cabinet> GetCabinetsWithDevices<T>() where T : SimplestСomponent
        {
            List<Cabinet> CabinetsWithdevs = new List<Cabinet>();
            foreach (Cabinet cabinet in cabinetsLst)
            {
                List<T> devicesList = (List<T>)cabinet.GetDevicesList<T>();
                if (devicesList.Count > 0)
                {
                    Cabinet newCabinet = new Cabinet
                    {
                        Designation = cabinet.Designation,
                        DeviceType = cabinet.DeviceType
                    };

                    foreach (T item in devicesList)
                    {
                        newCabinet.AddItem(item);
                    }
                    CabinetsWithdevs.Add(newCabinet);
                }
            }
            return CabinetsWithdevs;
        }

        public IList<Cabinet> GetFullCabinets()
        {
            return cabinetsLst;
        }

        public IList<T> GetAllDevices<T>() where T : SimplestСomponent
        {
            IList<Cabinet> cabinets = GetCabinetsWithDevices<T>();
            List<T> resultDevices = new List<T>();
            foreach (Cabinet cabinet in cabinets)
            {
                foreach (T device in cabinet.GetDevicesList<T>())
                {
                    resultDevices.Add(device);
                }
            }
            return resultDevices;
        }

        public IList<Cabinet> AddTwoListsOfCabinets(IList<Cabinet> list1, IList<Cabinet> list2)
        {
            IList<Cabinet> cabOut = new List<Cabinet>();

            foreach (Cabinet cab485 in list1)
            {
                Cabinet newCab = new Cabinet();
                newCab.Designation = cab485.Designation;
                foreach (RS485device device485 in cab485.GetAllDevicesList)
                {

                    newCab.AddItem(device485);
                }
                cabOut.Add(newCab);
            }

            foreach (Cabinet cab232 in list2)
            {
                foreach (Cabinet cab485 in cabOut)
                {
                    if (cab232.Designation.Equals(cab485.Designation))
                    {
                        foreach (RS232device device232 in cab232.GetAllDevicesList)
                        {
                            cab485.AddItem(device232);
                        }
                    }
                }
            }

            foreach (Cabinet cab232 in list2)
            {
                bool compare = false;
                foreach (Cabinet cab485 in list1)
                {
                    if (cab232.Designation.Equals(cab485.Designation))
                    {
                        compare = true;
                        break;
                    }

                }
                if (!compare)
                    cabOut.Add(cab232);
            }
            return cabOut;
        }
    }
}
