namespace DeviceTunerNET.SharedDataModel
{
    public class RS485device : CommunicationDevice, Irs485device
    {
        /// <summary>
        /// Адрес прибора на линии RS-485 ("23").
        /// </summary>
        public uint AddressRS485 { get; set; }     
    }
}
