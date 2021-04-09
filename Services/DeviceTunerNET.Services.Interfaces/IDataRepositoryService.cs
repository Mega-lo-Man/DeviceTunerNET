using DeviceTunerNET.SharedDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface IDataRepositoryService
    {
        /// <summary>
        /// Установить список приборов (найти в источнике данных все приборы)
        /// </summary>
        void SetDevices(int DataProviderType, string FullPathToData);

        /// <summary>
        /// Получить список шкафов с приборами заданного типа T для настройки
        /// </summary>
        /// <typeparam name="T">тип прибора унаследованный от SimplestComponent</typeparam>
        /// <returns>Список шкафов с приборами типа Т</returns>
        IList<Cabinet> GetCabinetsWithDevices<T>() where T : SimplestСomponent;

        /// <summary>
        /// Получить список шкафов с приборами заданных типов T1 и T2 для настройки.
        /// (Например, шкаф может содержать приборы типа RS485device и/или RS232device)
        /// </summary>
        /// <typeparam name="T1">Первый тип приборов</typeparam>
        /// <typeparam name="T2">Второй тип приборов</typeparam>
        /// <returns>Список шкафов с приборами типа T1 и/или T2</returns>
        IList<Cabinet> GetCabinetsWithTwoTypeDevices<T1, T2>()
            where T1 : SimplestСomponent
            where T2 : SimplestСomponent;

        /// <summary>
        /// Получить список шкафов со всеми приборами внутри
        /// </summary>
        /// <returns>Список шкафов со всеми приборами (всех типов)</returns>
        public IList<Cabinet> GetFullCabinets();

        /// <summary>
        /// Сложить списки шкафов с разными типами приборов (например, сложить список шкафов с приборами RS232device
        /// со списком шкафов с приборами RS485device)
        /// </summary>
        /// <param name="list1">Первый слагаемый список</param>
        /// <param name="list2">Второй слагаемый список</param>
        /// <returns>Третий список шкафов включающий в себя первый и второй списки шкафов</returns>
        public IList<Cabinet> AddTwoListsOfCabinets(IList<Cabinet> list1, IList<Cabinet> list2);

        /// <summary>
        /// Получить список всех приборов типа T во всех шкафах
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IList<T> GetAllDevices<T>() where T : SimplestСomponent;

        /// <summary>
        /// Записать свойства прибора в таблицу Excel или базу данных
        /// </summary>
        /// <typeparam name="T">тип прибора унаследованный от SimplestComponent</typeparam>
        /// <param name="device">экземпляр прибора</param>
        /// <returns>true - есди запись удалась, false - в противном случае</returns>
        //bool SaveDevice<T>(T device) where T : SimplestСomponent;
        bool SaveSerialNumber(int id, string serialNumber);
    }
}
