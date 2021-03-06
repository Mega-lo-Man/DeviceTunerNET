using System;
using static System.String;

namespace DeviceTunerNET.SharedDataModel
{
    public class RS232device : RS485device
    {
        /// <summary>
        /// Адрес прибора на линии RS-232 ("24")
        /// </summary>
        private int? _address_RS232;
        public int? AddressRS232
        {
            get => _address_RS232;
            set
            {
                if (value > 0 && value <= 127) _address_RS232 = value;
            }
        }

        /// <summary>
        /// IP адрес прибора ("192.168.2.12")
        /// </summary>
        public string AddressIP { get; set; }

        public string Netmask { get; set; }


        public int CIDR
        {
            get => ConvertToCidr(Netmask);
            set => Netmask = CidrToString(value);
        }

        /// <summary>
        /// MAC-адрес прибора
        /// </summary>
        private string _macAddress;
        public string MACaddress
        {
            get => _macAddress;
            set
            {
                if (value.Length <= 17 && value.Length >= 12) _macAddress = value;
            }
        }

        public string DefaultGateway { get; set; }

        private string CidrToString(int cidr)
        {
            var range = 0xFFFFFFFF;
            range <<= 32 - cidr;
            var fourBytes = new[] { "0", "0", "0", "0" };

            for (var i = 3; i >= 0; i--)
            {
                var x = range & 255;
                fourBytes[i] = x.ToString();
                range >>= 8;

            }
            return Join(".", fourBytes);
        }

        private int ConvertToCidr(string address)
        {
            var addr = address;
            var range = (uint)ConvertStringToRange(addr);
            var bitsCounter = 0;

            while (range > 0)
            {
                if ((range & 1) >= 0)
                    ++bitsCounter;
                range <<= 1;
            }
            return bitsCounter;
        }

        private int ConvertStringToRange(string addrStr)
        {
            var textAddress = addrStr;
            var result = 0;
            var bytesArray = textAddress.Split(new char[] { '.' });
            for (var i = 0; i < 4; i++)
            {
                result <<= 8;
                result |= int.Parse(bytesArray[i]);

            }
            return result;
        }
    }
}
