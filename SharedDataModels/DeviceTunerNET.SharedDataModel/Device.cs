namespace DeviceTunerNET.SharedDataModel
{
    public class Device : SimplestСomponent
    {
        /// <summary>
        /// Серийный номер прибора ("456426")
        /// </summary>
        public string Serial { get; set; }

        /// <summary>
        /// Версия железяки
        /// </summary>
        public string HardwareVersion { get; set; }

        /// <summary>
        /// Версия прошивки
        /// </summary>
        public string FirmwareVersion { get; set; }

        /// <summary>
        /// Прибор прошёл проверку в собранном шкафу
        /// </summary>
        public bool QualityControlPassed { get; set; }

        /// <summary>
        /// Название площадки на которой находится шкаф с этим прибором ("КС "Невинномысская"")
        /// </summary>
        public string Area { get; set; }

        /// <summary>
        /// Наименование шкафа в котором находится этот прибор ("ШКО1")
        /// </summary>
        public string Cabinet { get; set; }
    }
}
