using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.SharedDataModel
{
    public class Device : SimplestСomponent
    {        
        /// <summary>
        /// Серийный номер прибора ("456426")
        /// </summary>
        private string _serial;
        public string Serial
        {
            get { return _serial; }
            set { _serial = value; }
        }
        /// <summary>
        /// Версия железяки
        /// </summary>
        private string _hardwareVersion;
        public string HardwareVersion
        {
            get { return _hardwareVersion; }
            set { _hardwareVersion = value; }
        }
        /// <summary>
        /// Версия прошивки
        /// </summary>
        private string _firmwareVersion;
        public string FirmwareVersion
        {
            get { return _firmwareVersion; }
            set { _firmwareVersion = value; }
        }
        /// <summary>
        /// Название площадки на которой находится шкаф с этим прибором ("КС "Невинномысская"")
        /// </summary>
        private string _area;
        public string Area
        {
            get { return _area; }
            set { _area = value; }
        }
        /// <summary>
        /// Наименование шкафа в котором находится этот прибор ("ШКО1")
        /// </summary>
        private string _cabinet;
        public string Cabinet
        {
            get { return _cabinet; }
            set { _cabinet = value; }
        }
    }
}
