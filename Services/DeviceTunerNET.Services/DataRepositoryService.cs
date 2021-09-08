using DeviceTunerNET.Core;
using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using Prism.Events;
using System.Collections.Generic;

namespace DeviceTunerNET.Services
{
    public class DataRepositoryService : IDataRepositoryService
    {
        private List<Cabinet> _cabinetsLst = new List<Cabinet>();

        private readonly IEventAggregator _ea;
        private readonly IExcelDataDecoder _excelDataDecoder;

        private int _dataProviderType = 1;

        public DataRepositoryService(IEventAggregator ea, IExcelDataDecoder excelDataDecoder)
        {
            _ea = ea;
            _excelDataDecoder = excelDataDecoder;
        }

        public void SetDevices(int DataProviderType, string FullPathToData)
        {
            _dataProviderType = DataProviderType;
            var _fullPathToData = FullPathToData;

            _cabinetsLst.Clear();
            switch (_dataProviderType)
            {
                case 1:
                    _cabinetsLst = _excelDataDecoder.GetCabinetsFromExcel(_fullPathToData);
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
            if (_dataProviderType != 1)
                return false;

            _excelDataDecoder.SaveSerialNumber(id, serialNumber);
            
            return true;
        }

        public IList<Cabinet> GetCabinetsWithTwoTypeDevices<T1, T2>()
            where T1 : SimplestСomponent
            where T2 : SimplestСomponent
        {
            var cabinetsWithdevs = new List<Cabinet>();
            foreach (var cabinet in _cabinetsLst)
            {
                var devicesListT1 = (List<T1>)cabinet.GetDevicesList<T1>();
                var devicesListT2 = (List<T2>)cabinet.GetDevicesList<T2>();

                // Будем работать только с теми шкафами в которых есть приборы типа T1 или T2
                if (devicesListT1.Count <= 0 && devicesListT2.Count <= 0) 
                    continue;

                // В возвращаемом из метода списке будем создавать новые шкафы
                var newCabinet = new Cabinet
                {
                    Designation = cabinet.Designation,
                    DeviceType = cabinet.DeviceType
                };

                foreach (var item in devicesListT1)
                {
                    newCabinet.AddItem(item);
                }

                foreach (var item in devicesListT2)
                {
                    newCabinet.AddItem(item);
                }
                cabinetsWithdevs.Add(newCabinet);
            }
            return cabinetsWithdevs;
        }

        public IList<Cabinet> GetCabinetsWithDevices<T>() where T : SimplestСomponent
        {
            var cabinetsWithdevs = new List<Cabinet>();
            foreach (var cabinet in _cabinetsLst)
            {
                var devicesList = (List<T>)cabinet.GetDevicesList<T>();
                if (devicesList.Count <= 0)
                    continue;

                var newCabinet = new Cabinet
                {
                    Designation = cabinet.Designation,
                    DeviceType = cabinet.DeviceType
                };

                foreach (var item in devicesList)
                {
                    newCabinet.AddItem(item);
                }
                cabinetsWithdevs.Add(newCabinet);
            }
            return cabinetsWithdevs;
        }

        public IList<Cabinet> GetFullCabinets()
        {
            return _cabinetsLst;
        }

        public IList<T> GetAllDevices<T>() where T : SimplestСomponent
        {
            var cabinets = GetCabinetsWithDevices<T>();
            var resultDevices = new List<T>();
            foreach (var cabinet in cabinets)
            {
                foreach (var device in cabinet.GetDevicesList<T>())
                {
                    resultDevices.Add(device);
                }
            }
            return resultDevices;
        }

        public IList<Cabinet> AddTwoListsOfCabinets(IList<Cabinet> list1, IList<Cabinet> list2)
        {
            var cabOut = new List<Cabinet>();

            foreach (var cab485 in list1)
            {
                var newCab = new Cabinet
                {
                    Designation = cab485.Designation
                };

                foreach (RS485device device485 in cab485.GetAllDevicesList)
                {

                    newCab.AddItem(device485);
                }
                cabOut.Add(newCab);
            }

            foreach (var cab232 in list2)
            {
                foreach (var cab485 in cabOut)
                {
                    if (!cab232.Designation.Equals(cab485.Designation))
                        continue;

                    foreach (RS232device device232 in cab232.GetAllDevicesList)
                    {
                        cab485.AddItem(device232);
                    }
                }
            }

            foreach (var cab232 in list2)
            {
                var compare = false;
                foreach (var cab485 in list1)
                {
                    if (!cab232.Designation.Equals(cab485.Designation))
                        continue;

                    compare = true;
                    break;

                }
                if (!compare)
                    cabOut.Add(cab232);
            }
            return cabOut;
        }
    }
}
