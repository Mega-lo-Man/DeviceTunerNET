using DeviceTunerNET.Core;
using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using DeviceTunerNET.SharedDataModel.Devices;
using Prism.Events;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DeviceTunerNET.Services
{
    public class BolidAddressChanger(
        IDeviceGenerator deviceGenerator,
        IEventAggregator ea) : IAddressChanger
    {
        private readonly IDeviceGenerator _deviceGenerator = deviceGenerator;
        private readonly IEventAggregator _ea = ea;

        private CancellationToken _token;

        public List<RS485device> FoundDevices { get; set; } = [];

        public IPort Port { get; set; }

        public string DefaultDeviceFoundMessage { get; private set; } = "1";
        public void ChangeDefaultAddresses(CancellationToken cancellationToken)
        {
            _token = cancellationToken;
            var c2000M = new C2000M(Port);
            c2000M.Port.MaxRepetitions = 1;

            while (!_token.IsCancellationRequested)
            {
                var response = c2000M.GetModelCode(127, out var deviceCode);
                if (response)
                {
                    if (!C2000M.TryGetDeviceByCode(deviceCode, out var orionDevice))
                    {
                        Log.Error($"Invalid deviceCode: {deviceCode}. Method: _deviceGenerator.TryGetDeviceByCode(deviceCode, out var orionDevice)");
                        continue;
                    }

                    orionDevice.AddressRS485 = FindFreeAddress(c2000M);

                    // We created new device with empty port!
                    orionDevice.Port = c2000M.Port;
                    orionDevice.SetAddress();
                    FoundDevices.Add((RS485device)orionDevice);
                    MessageUpdateOnlineDevices(DefaultDeviceFoundMessage);
                }
            }
        }

        public void TryToChangeDeviceAddress(uint address, IOrionDevice currentDevice)
        {
            bool changeAddressSuccess = currentDevice.ChangeDeviceAddress((byte)address);

            if (changeAddressSuccess)
            {
                currentDevice.AddressRS485 = address;
            }
        }

        private uint FindFreeAddress(IOrionDevice c2000M)
        {
            while (!_token.IsCancellationRequested)
            {
                var busyAddresses = FoundDevices.Select(o => o.AddressRS485).ToList();
                var addressRS485 = GetFirstMissing(busyAddresses, 127);

                var result = IsAddressFree(c2000M, addressRS485, out var device);
                
                if (!result)
                {
                    FoundDevices.Add(device);
                    MessageUpdateOnlineDevices("");
                    continue;
                }

                return addressRS485;
            }
            throw new Exception("There aren't free addresses!");
        }

        private bool IsAddressFree(IOrionDevice c2000M, uint addressRS485, out RS485device device)
        {
            var response = c2000M.GetModelCode((byte)addressRS485, out var deviceCode);
            if (response)
            {
                if (!C2000M.TryGetDeviceByCode(deviceCode, out var orionDevice))
                    throw new Exception("Unknow device was found. Address: " + addressRS485);
                orionDevice.AddressRS485 = addressRS485;
                orionDevice.Port = c2000M.Port;
                device = (RS485device)orionDevice;

                return false;
            }
            device = new RS485device();
            return true;
        }

        private void MessageUpdateOnlineDevices(string message)
        {
            //Сообщаем об обновлении данных в репозитории
            _ea.GetEvent<MessageSentEvent>().Publish(new Message
            {
                ActionCode = MessageSentEvent.FoundNewOnlineDevice,
                MessageString = message
            });
        }

        public uint GetFirstMissing(IEnumerable<uint> numbers, uint maximum)
        {
            for (uint i = 1; i < maximum; i++)
            {
                if (!numbers.Contains(i))
                    return i;
            }
            return 127;
        }
    }
}
