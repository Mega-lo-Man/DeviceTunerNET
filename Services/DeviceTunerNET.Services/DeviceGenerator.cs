﻿using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using DeviceTunerNET.SharedDataModel.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services
{
    public class DeviceGenerator : IDeviceGenerator
    {
        private static readonly Dictionary<string, Func<IRS485device>> _deviceFactory = new Dictionary<string, Func<IRS485device>>()
        {
            {"С2000М", () => new RS485device()},
            {"Сигнал-20", () => new RS485device() },
            {"Сигнал-20П", () => new Signal20P() }, // complete
            {"Сигнал-20П исп.01", () => new Signal20P() }, // complete
            {"С2000-СП1", () => new RS485device() },
            {"С2000-СП1 исп.01", () => new RS485device() },
            {"С2000-4", () => new RS485device() },
            {"С2000-К", () => new RS485device() },
            {"С2000-ИТ", () => new RS485device() },
            {"С2000-КДЛ", () => new RS485device() },
            {"С2000-БИ/БКИ", () => new RS485device() },
            {"Сигнал-20(вер. 02)", () => new RS485device() },
            {"С2000-КС", () => new RS485device() },
            {"С2000-АСПТ", () => new RS485device() },
            {"С2000-КПБ", () => new RS485device() },
            {"С2000-2", () => new RS485device() },
            {"УО-ОРИОН", () => new RS485device() },
            {"Рупор", () => new RS485device() },
            {"Рупор-Диспетчер исп.01", () => new RS485device() },
            {"С2000-ПТ", () => new RS485device() },
            {"УО-4С", () => new RS485device() },
            {"Поток-3Н", () => new RS485device() },
            {"Сигнал-20М", () => new RS485device() },
            {"С2000-БИ-01", () => new RS485device() },
            {"С2000-Ethernet", () => new C2000Ethernet() }, // complete
            {"Рупор-01", () => new RS485device() },
            {"С2000-Adem", () => new RS485device() },

            {"РИП-12", () => new RS485device() },
            {"РИП-12 исп.50", () => new RS485device() },
            {"РИП-12 исп.51", () => new RS485device() },

            {"Сигнал-10", () => new RS485device() },
            {"С2000-ПП", () => new RS485device() },
            {"РИП-12 исп.54", () => new RS485device() },

            {"РИП-24 исп.50", () => new RS485device() },
            {"РИП-24 исп.51", () => new RS485device() },

            {"С2000-КДЛ-2И", () => new RS485device() },
            {"С2000-PGE", () => new RS485device() },
            {"С2000-БКИ", () => new RS485device() },
            {"Поток-БКИ", () => new RS485device() },
            {"Рупор-200", () => new RS485device() },
            {"С2000-Периметр", () => new RS485device() },
            {"МИП-12", () => new RS485device() },
            {"МИП-24", () => new RS485device() },
            {"РИП-48 исп.01", () => new RS485device() },
            {"РИП-12 исп.56", () => new RS485device() },
            {"РИП-24 исп.56", () => new RS485device() },
            {"Рупор исп.02", () => new RS485device() },
            {"С2000-КДЛ-Modbus", () => new RS485device() },
            {"Рупор исп.03", () => new RS485device() },
            {"Рупор-300", () => new RS485device() },
            {"MES3508", () => new EthernetSwitch() },
            {"MES3508P", () => new EthernetSwitch() },
            {"MES2308", () => new EthernetSwitch() },
            {"MES2308P", () => new EthernetSwitch() },
            {"MES2324", () => new EthernetSwitch() },

        };

        public bool TryGetDevice(string name, out RS485device device)
        {
            if(!_deviceFactory.ContainsKey(name))
            {
                device = default;
                return false;
            }
            device = (RS485device)_deviceFactory[name]();// _deviceFactory[name]();
            return true;
        }
    }
}