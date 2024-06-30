using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel
{
    public interface ISimplestComponent
    {
        public int Id { get; set; }

        /// <summary>
        /// Модель компонента ("С2000-СП1 исп.01 АЦДР.425412.001-01")
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Тип прибора ("Блок сигнально-пусковой")
        /// </summary>
        public string DeviceType { get; set; }

        /// <summary>
        /// Обозначение компонента на схеме ("SR1.3")
        /// </summary>
        public string Designation { get; set; }

        /// <summary>
        /// Обозначение шкафа в котором находится этот дивайс
        /// </summary>
        public string ParentName { get; set; }
    }
}
