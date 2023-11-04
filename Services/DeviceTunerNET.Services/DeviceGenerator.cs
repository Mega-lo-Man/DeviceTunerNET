using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using DeviceTunerNET.SharedDataModel.Devices;
using System;
using System.CodeDom;
using System.Collections.Generic;

namespace DeviceTunerNET.Services
{
    public class DeviceGenerator : IDeviceGenerator
    {
        private readonly Dictionary<string, Func<IOrionDevice>> _orionDevices= new()
        {
            {"С2000М", () => new C2000M(null)},                             
            {"Сигнал-20", () => new OrionDevice(null) },                        
            {"Сигнал-20П", () => new Signal20P(null) }, // complete         
            {"Сигнал-20П исп.01", () => new Signal20P(null) }, // complete  
            {"С2000-СП1", () => new C2000sp1(null) },                       
            {"С2000-СП1 исп.01", () => new C2000sp1(null) },                
            {"С2000-4", () => new C2000_4(null) },                          
            {"С2000-К", () => new OrionDevice(null) },                          
            {"С2000-ИТ", () => new OrionDevice(null) },                         
            {"С2000-КДЛ", () => new C2000_Kdl(null) },                      
            {"С2000-БИ/БКИ", () => new OrionDevice(null) },                     
            {"Сигнал-20(вер. 02)", () => new OrionDevice(null) },               
            {"С2000-КС", () => new OrionDevice(null) },                         
            {"С2000-АСПТ", () => new OrionDevice(null) },                       
            {"С2000-КПБ", () => new OrionDevice(null) },                        
            {"С2000-2", () => new C2000_2(null) },                          
            {"УО-ОРИОН", () => new OrionDevice(null) },                         
            {"Рупор", () => new OrionDevice(null) },                            
            {"Рупор-Диспетчер исп.01", () => new OrionDevice(null) },           
            {"С2000-ПТ", () => new OrionDevice(null) },                         
            {"УО-4С", () => new OrionDevice(null) },                            
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
            {"С2000-PGE", () => new OrionDevice(null) },                        
            {"С2000-БКИ", () => new OrionDevice(null) },                        
            {"Поток-БКИ", () => new OrionDevice(null) },                        
            {"Рупор-200", () => new OrionDevice(null) },                        
            {"С2000-Периметр", () => new C2000Perimeter(null) },            
            {"МИП-12", () => new OrionDevice(null) },                           
            {"МИП-24", () => new OrionDevice(null) },                           
            {"РИП-48 исп.01", () => new RipRs_48(null) },                   
            {"РИП-12 исп.56", () => new RipRs12_56(null) },
            {"РИП-24 исп.56", () => new OrionDevice(null) },
            {"Рупор исп.02", () => new OrionDevice(null) },
            {"С2000-КДЛ-Modbus", () => new OrionDevice(null) },
            {"Рупор исп.03", () => new OrionDevice(null) },
            {"Рупор-300", () => new OrionDevice(null) },
        };

        private readonly Dictionary<string, Func<IEthernetDevice>> _ethernetSwitches = new()
        {
            {"MES3508", () => new EthernetSwitch(null) },
            {"MES3508P", () => new EthernetSwitch(null) },
            {"MES2308", () => new EthernetSwitch(null) },
            {"MES2308_AC", () => new EthernetSwitch(null) },
            {"MES2308_DC", () => new EthernetSwitch(null) },
            {"MES2308P", () => new EthernetSwitch(null) },
            {"MES2308P_AC", () => new EthernetSwitch(null) },
            {"MES2308P_DC", () => new EthernetSwitch(null) },
            {"MES2324", () => new EthernetSwitch(null) },
            {"MES2324_AC", () => new EthernetSwitch(null) },
            {"MES2324_DC", () => new EthernetSwitch(null) },
        };

        public DeviceGenerator()
        {

        }

        public bool TryGetDevice(string name, out ICommunicationDevice device)
        {
            device = default;
            if (_ethernetSwitches.ContainsKey(name))
            {
                device = _ethernetSwitches[name]();
                return true;
            }

            if (_orionDevices.ContainsKey(name))
            {
                device = _orionDevices[name]();
                return true;
            }
            
            return false;
        }

        public bool TryGetDeviceByCode(int code, out IOrionDevice device)
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
    }
}
