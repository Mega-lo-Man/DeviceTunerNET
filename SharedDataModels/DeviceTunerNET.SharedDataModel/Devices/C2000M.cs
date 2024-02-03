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

        private static Dictionary<string, Func<IOrionDevice>> _orionDevices = new()
        {
            {"С2000М", () => new C2000M(null)},
            {"Сигнал-20", () => new Signal20(null) },
            {"Сигнал-20П", () => new Signal20P(null) }, // complete         
            {"Сигнал-20П исп.01", () => new Signal20P(null) }, // complete  
            {"С2000-СП1", () => new C2000sp1(null) },
            {"С2000-СП1 исп.01", () => new C2000sp1(null) },
            {"С2000-4", () => new C2000_4(null) },
            {"С2000-К", () => new C2000_k(null) },
            {"С2000-ИТ", () => new C2000_it(null) },
            {"С2000-КДЛ", () => new C2000_Kdl(null) },
            {"С2000-БИ/БКИ", () => new C2000_bi(null) },
            {"Сигнал-20(вер. 02)", () => new OrionDevice(null) },
            {"С2000-КС", () => new C2000_ks(null) },
            {"С2000-АСПТ", () => new C2000_aspt(null) },
            {"С2000-КПБ", () => new C2000_kpb(null) },
            {"С2000-2", () => new C2000_2(null) },
            {"УО-ОРИОН", () => new OrionDevice(null) },
            {"Рупор", () => new OrionDevice(null) },
            {"Рупор-Диспетчер исп.01", () => new OrionDevice(null) },
            {"С2000-ПТ", () => new OrionDevice(null) },
            {"УО-4С", () => new UO4S(null) },
            {"Поток-3Н", () => new OrionDevice(null) },
            {"Сигнал-20М", () => new Signal20M(null) },
            {"С2000-БИ-01", () => new OrionDevice(null) },
            {"С2000-Ethernet", () => new C2000Ethernet(null) }, // complete 
            {"Рупор-01", () => new OrionDevice(null) },
            {"С2000-Adem", () => new OrionDevice(null) },
            {"РИП-12", () => new RipRs12_51(null) },
            {"РИП-12 исп.50", () => new RipRs12_51(null) },
            {"РИП-12 исп.51", () => new RipRs12_51(null) },
            {"Сигнал-10", () => new Signal_10(null) },
            {"С2000-ПП", () => new OrionDevice(null) },
            {"РИП-12 исп.54", () => new RipRs24_54(null) },
            {"РИП-24 исп.50", () => new RipRs24_51(null) },
            {"РИП-24 исп.51", () => new RipRs24_51(null) },
            {"С2000-КДЛ-2И", () => new C2000_Kdl2i(null) },
            {"С2000-PGE", () => new C2000_pge(null) },
            {"С2000-БКИ", () => new C2000_bki(null) },
            {"Поток-БКИ", () => new OrionDevice(null) },
            {"Рупор-200", () => new OrionDevice(null) },
            {"С2000-Периметр", () => new C2000Perimeter(null) },
            {"МИП-12", () => new Mip_12(null) },
            {"МИП-24", () => new Mip_24(null) },
            {"РИП-48 исп.01", () => new RipRs_48(null) },
            {"РИП-12 исп.56", () => new RipRs12_56(null) },
            {"РИП-24 исп.56", () => new RipRs24_56(null) },
            {"Рупор исп.02", () => new OrionDevice(null) },
            {"С2000-КДЛ-Modbus", () => new OrionDevice(null) },
            {"Рупор исп.03", () => new OrionDevice(null) },
            {"Рупор-300", () => new OrionDevice(null) },
        };

        public C2000M(IPort port) : base(port)
        {
            Model = "С2000М";
            SupportedModels = new List<string>
            {
                Model,
            };
        }

        /// <summary>
        /// Get an ICommunicationDevice instance based on its description
        /// </summary>
        /// <param name="name">Device model</param>
        /// <param name="device"></param>
        /// <returns></returns>
        public static bool TryGetDevice(string name, out ICommunicationDevice device)
        {
            device = default;
            if (_orionDevices.ContainsKey(name))
            {
                device = _orionDevices[name]();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get an IOrionDevice instance based on its device code
        /// </summary>
        /// <param name="code">Device code</param>
        /// <param name="device">Device instance</param>
        /// <returns>true if instance creating was succes</returns>
        public static bool TryGetDeviceByCode(int code, out IOrionDevice device)
        {
            foreach (var kvp in _orionDevices)
            {
                object obj = kvp.Value().GetType().GetField("Code")?.GetValue(kvp.Value());
                if (obj == null)
                    continue;
                if (obj is int deviceCode && deviceCode == code)
                {
                    device = kvp.Value();
                    return true;
                }
            }

            device = null;
            return false;
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
                
                if (TryGetDeviceByCode(deviceCode, out var device))
                {
                    device.AddressRS485 = devAddr;
                    device.Port = Port;
                    yield return device;
                }
                progressStatus(Convert.ToInt32(progress));
                progress += progressStep;
            }
            Port.MaxRepetitions = 15;
        }
    }
}
