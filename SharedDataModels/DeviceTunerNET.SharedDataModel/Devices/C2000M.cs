using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Xml.Linq;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000M : OrionDevice
    {
        private readonly Dictionary<byte, Func<IRS485device>> _bolidDict = new()
        {
            { 0, () => new C2000M(null) },          
            { 1, () => new RS485device() },          //"Сигнал-20" },
            { 2, () => new Signal20P(null) },        //"Сигнал-20П, Сигнал-20П исп.01" },
            { 3, () => new C2000sp1(null) },         //"С2000-СП1, С2000-СП1 исп.01" },
            { 4, () => new C2000_4(null) },          //"С2000-4" },
            { 7, () => new RS485device() },          //"С2000-К" },
            { 8, () => new RS485device() },          //"С2000-ИТ" },
            { 9, () => new C2000_Kdl(null) },        //"С2000-КДЛ" },
            { 10, () => new RS485device() },         //"С2000-БИ/БКИ" },
            { 11, () => new RS485device() },         //"Сигнал-20(вер. 02)" },
            { 13, () => new RS485device() },         //"С2000-КС" },
            { 14, () => new RS485device() },         //"С2000-АСПТ" },
            { 15, () => new RS485device() },         //"С2000-КПБ" },
            { 16, () => new C2000_2(null) },         //"С2000-2" },
            { 19, () => new RS485device() },         //"УО-ОРИОН" },
            { 20, () => new RS485device() },         //"Рупор" },
            { 21, () => new RS485device() },         //"Рупор-Диспетчер исп.01" },
            { 22, () => new RS485device() },         //"С2000-ПТ" },
            { 24, () => new RS485device() },         //"УО-4С" },
            { 25, () => new RS485device() },         //"Поток-3Н" },
            { 26, () => new Signal20M(null) },       //"Сигнал-20М" },
            { 28, () => new RS485device() },         //"С2000-БИ-01" },
            { 29, () => new C2000Ethernet(null) },   //"С2000-Ethernet" },
            { 30, () => new RS485device() },         //"Рупор-01" },
            { 31, () => new RS485device() },         //"С2000-Adem" },
            { 33, () => new RipRs12_51(null) },      //"РИП-12 исп.50, РИП-12 исп.51, РИП-12 без исполнения"
            { 34, () => new Signal_10(null) },       //"Сигнал-10" },
            { 36, () => new RS485device() },         //"С2000-ПП" },
            { 38, () => new RipRs24_54(null) },      //"РИП-12 исп.54" },
            { 39, () => new RipRs24_51(null) },      //"РИП-24 исп.50, РИП-24 исп.51" },
            { 41, () => new C2000_Kdl2i(null) },     //"С2000-КДЛ-2И" },
            { 43, () => new RS485device() },         //"С2000-PGE" },
            { 44, () => new RS485device() },         //"С2000-БКИ" },
            { 45, () => new RS485device() },         //"Поток-БКИ" },
            { 46, () => new RS485device() },         //"Рупор-200" },
            { 47, () => new C2000Perimeter(null) },  //"С2000-Периметр" },
            { 48, () => new RS485device() },         //"МИП-12" },
            { 49, () => new RS485device() },         //"МИП-24" },
            { 53, () => new RipRs_48(null) },        //"РИП-48 исп.01" },
            { 54, () => new RipRs12_56(null) },      //"РИП-12 исп.56" },
            { 55, () => new RipRs24_56(null) },      //"РИП-24 исп.56" },
            { 59, () => new RS485device() },         //"Рупор исп.02" },
            { 61, () => new RS485device() },         //"С2000-КДЛ-Modbus" },
            { 66, () => new RS485device() },         //"Рупор исп.03" },
            { 67, () => new RS485device() }          //"Рупор-300" }
        };

        public C2000M(IPort port) : base(port)
        {
            ModelCode = 0;
        }

        public IEnumerable<RS485device> SearchOnlineDevices(Action<int> progressStatus)
        {
            var progress = 1.0;
            var progressStep = 0.7874;

            progressStatus(Convert.ToInt32(progress));
            var result = new Dictionary<byte, string>();
            for (byte devAddr = 1; devAddr <= 127; devAddr++)
            {
                var isSearchSuccess = GetModelCode(devAddr, out var deviceCode);
                if (!isSearchSuccess)
                {
                    progressStatus(Convert.ToInt32(progress));
                    progress += progressStep;
                    continue;
                }
                
                if (_bolidDict.TryGetValue(deviceCode, out var device))
                {
                    var rs485device = (RS485device)_bolidDict[deviceCode]();
                    rs485device.AddressRS485 = devAddr;
                    yield return rs485device;
                }
                progressStatus(Convert.ToInt32(progress));
                progress += progressStep;
            }
        }
    }
}
