using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000M : OrionDevice
    {
        public new const int Code = 0;

        private readonly Dictionary<byte, Func<IOrionDevice>> _bolidDict = new()
        {
            { 0, () => new C2000M(null) },          
            { 1, () => new OrionDevice(null) },          //"Сигнал-20" },
            { 2, () => new Signal20P(null) },        //"Сигнал-20П, Сигнал-20П исп.01" },
            { 3, () => new C2000sp1(null) },         //"С2000-СП1, С2000-СП1 исп.01" },
            { 4, () => new C2000_4(null) },          //"С2000-4" },
            { 7, () => new OrionDevice(null) },          //"С2000-К" },
            { 8, () => new OrionDevice(null) },          //"С2000-ИТ" },
            { 9, () => new C2000_Kdl(null) },        //"С2000-КДЛ" },
            { 10, () => new OrionDevice(null) },         //"С2000-БИ/БКИ" },
            { 11, () => new OrionDevice(null) },         //"Сигнал-20(вер. 02)" },
            { 13, () => new OrionDevice(null) },         //"С2000-КС" },
            { 14, () => new OrionDevice(null) },         //"С2000-АСПТ" },
            { 15, () => new OrionDevice(null) },         //"С2000-КПБ" },
            { 16, () => new C2000_2(null) },         //"С2000-2" },
            { 19, () => new OrionDevice(null) },         //"УО-ОРИОН" },
            { 20, () => new OrionDevice(null) },         //"Рупор" },
            { 21, () => new OrionDevice(null) },         //"Рупор-Диспетчер исп.01" },
            { 22, () => new OrionDevice(null) },         //"С2000-ПТ" },
            { 24, () => new OrionDevice(null) },         //"УО-4С" },
            { 25, () => new OrionDevice(null) },         //"Поток-3Н" },
            { 26, () => new Signal20M(null) },       //"Сигнал-20М" },
            { 28, () => new OrionDevice(null) },         //"С2000-БИ-01" },
            { 29, () => new C2000Ethernet(null) },   //"С2000-Ethernet" },
            { 30, () => new OrionDevice(null) },         //"Рупор-01" },
            { 31, () => new OrionDevice(null) },         //"С2000-Adem" },
            { 33, () => new RipRs12_51(null) },      //"РИП-12 исп.50, РИП-12 исп.51, РИП-12 без исполнения"
            { 34, () => new Signal_10(null) },       //"Сигнал-10" },
            { 36, () => new OrionDevice(null) },         //"С2000-ПП" },
            { 38, () => new RipRs24_54(null) },      //"РИП-12 исп.54" },
            { 39, () => new RipRs24_51(null) },      //"РИП-24 исп.50, РИП-24 исп.51" },
            { 41, () => new C2000_Kdl2i(null) },     //"С2000-КДЛ-2И" },
            { 43, () => new OrionDevice(null) },         //"С2000-PGE" },
            { 44, () => new OrionDevice(null) },         //"С2000-БКИ" },
            { 45, () => new OrionDevice(null) },         //"Поток-БКИ" },
            { 46, () => new OrionDevice(null) },         //"Рупор-200" },
            { 47, () => new C2000Perimeter(null) },  //"С2000-Периметр" },
            { 48, () => new OrionDevice(null) },         //"МИП-12" },
            { 49, () => new OrionDevice(null) },         //"МИП-24" },
            { 53, () => new RipRs_48(null) },        //"РИП-48 исп.01" },
            { 54, () => new RipRs12_56(null) },      //"РИП-12 исп.56" },
            { 55, () => new RipRs24_56(null) },      //"РИП-24 исп.56" },
            { 59, () => new OrionDevice(null) },         //"Рупор исп.02" },
            { 61, () => new OrionDevice(null) },         //"С2000-КДЛ-Modbus" },
            { 66, () => new OrionDevice(null) },         //"Рупор исп.03" },
            { 67, () => new OrionDevice(null) }          //"Рупор-300" }
        };

        public C2000M(IPort port) : base(port)
        {
            Model = "С2000М";
            SupportedModels = new List<string>
            {
                Model,
            };
        }

        public IEnumerable<IOrionDevice> SearchOnlineDevices(Action<int> progressStatus, CancellationToken token)
        {
            var progress = 1.0;
            var progressStep = 0.7874;

            progressStatus(Convert.ToInt32(progress));
            var result = new Dictionary<byte, string>();
            Port.MaxRepetitions = 3;
            
            for (byte i = 0; i < 127; i++)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }
                byte devAddr = (byte)((i == 0) ? 127 : i); // Start searching from 127 (default bolid address
                var isSearchSuccess = GetModelCode(devAddr, out var deviceCode);
                if (!isSearchSuccess)
                {
                    progressStatus(Convert.ToInt32(progress));
                    progress += progressStep;
                    continue;
                }
                
                if (_bolidDict.TryGetValue(deviceCode, out var device))
                {
                    var rs485device = _bolidDict[deviceCode]();
                    rs485device.AddressRS485 = devAddr;
                    rs485device.Port = Port;
                    yield return rs485device;
                }
                progressStatus(Convert.ToInt32(progress));
                progress += progressStep;
            }
            Port.MaxRepetitions = 15;
        }
    }
}
