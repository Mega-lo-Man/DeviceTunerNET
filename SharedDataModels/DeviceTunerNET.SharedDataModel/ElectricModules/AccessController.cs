using DeviceTunerNET.SharedDataModel.Devices;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.ElectricModules
{
    public class AccessController(IOrionDevice orionDevice)
    {
        private IOrionDevice _parentDevice = orionDevice;
        private byte accessCommandCode = 0x23;

        public byte[] ProvidingAccess()
        {
            var packet = new byte[] { accessCommandCode, 0x00, 0x00 };
            var result = _parentDevice.AddressTransaction((byte)_parentDevice.AddressRS485, packet, IOrionNetTimeouts.Timeouts.readModel);
            return result;
        }

        public byte[] AccessPermission ()
        {
            var packet = new byte[] { accessCommandCode, 0x00, 0x01 };
            var result = _parentDevice.AddressTransaction((byte)_parentDevice.AddressRS485, packet, IOrionNetTimeouts.Timeouts.readModel);
            return result;
        }

        public byte[] PermissionEntrance ()
        {
            var packet = new byte[] { accessCommandCode, 0x00, 0x02 };
            var result = _parentDevice.AddressTransaction((byte)_parentDevice.AddressRS485, packet, IOrionNetTimeouts.Timeouts.readModel);
            return result;
        }

        public byte[] PermissionOutput ()
        {
            var packet = new byte[] { accessCommandCode, 0x00, 0x03 };
            var result = _parentDevice.AddressTransaction((byte)_parentDevice.AddressRS485, packet, IOrionNetTimeouts.Timeouts.readModel);
            return result;
        }

        public byte[] AccessDenied ()
        {
            var packet = new byte[] { accessCommandCode, 0x00, 0x04 };
            var result = _parentDevice.AddressTransaction((byte)_parentDevice.AddressRS485, packet, IOrionNetTimeouts.Timeouts.readModel);
            return result;
        }

        public byte[] EntranceDenied ()
        {
            var packet = new byte[] { accessCommandCode, 0x00, 0x05 };
            var result = _parentDevice.AddressTransaction((byte)_parentDevice.AddressRS485, packet, IOrionNetTimeouts.Timeouts.readModel);
            return result;
        }

        public byte[] OutputeDenied ()
        {
            var packet = new byte[] { accessCommandCode, 0x00, 0x06 };
            var result = _parentDevice.AddressTransaction((byte)_parentDevice.AddressRS485, packet, IOrionNetTimeouts.Timeouts.readModel);
            return result;
        }

        public byte[] AllowAccess ()
        {
            var packet = new byte[] { accessCommandCode, 0x00, 0x07 };
            var result = _parentDevice.AddressTransaction((byte)_parentDevice.AddressRS485, packet, IOrionNetTimeouts.Timeouts.readModel);
            return result;
        }
    }
}
