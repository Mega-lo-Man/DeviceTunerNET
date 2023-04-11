using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public interface IOrionNetTimeouts
    {
        public enum Timeouts
        {
            addressChanging = 500, // Сингал-20П V3.10 после смены адреса подтверждает через 400 мс (остальные быстрее)
            readModel = 70, // Чтение типа прибора занимает не более 50 мс
            ethernetConfig = 350,
            restartC2000Ethernet = 7000,
            notResponse = 400 // оижидание конца пакета, если пакет за это время полоностью не пришёл, наверно, уже и не придёт
        }
    }
}
