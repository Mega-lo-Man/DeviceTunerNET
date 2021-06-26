using System;

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
            get { return _address_RS232; }
            set { if (value > 0 && value <= 127) _address_RS232 = value; }
        }

        /// <summary>
        /// IP адрес прибора ("192.168.2.12")
        /// </summary>
        private string _address_IP;
        public string AddressIP
        {
            get { return _address_IP; }
            set { _address_IP = value; }
        }

        private string _netmask;
        public string Netmask
        {
            get { return _netmask; }
            set { _netmask = value; }
        }


        public int CIDR
        {
            get { return ConvertToCidr(Netmask); }
            set { _netmask = CidrToString(value); }
        }

        /// <summary>
        /// MAC-адрес прибора
        /// </summary>
        private string _macAddress;
        public string MACaddress
        {
            get { return _macAddress; }
            set { if (value.Length <= 17 && value.Length >= 12) _macAddress = value; }
        }

        private string _defaultGateway;
        public string DefaultGateway
        {
            get { return _defaultGateway; }
            set { _defaultGateway = value; }
        }

        private string CidrToString(int cidr)
        {
            uint range = 0xFFFFFFFF;
            range <<= 32 - cidr;
            string[] fourBytes = new[] { "0", "0", "0", "0" };

            for (int i = 3; i >= 0; i--)
            {
                uint x = range & 255;
                fourBytes[i] = x.ToString();
                range >>= 8;

            }
            return String.Join(".", fourBytes);
        }

        private int ConvertToCidr(string address)
        {
            string addr = address;
            uint range = (uint)ConvertStringToRange(addr);
            int bitsCounter = 0;

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
            string textAddress = addrStr;
            int result = 0;
            string[] bytesArray = textAddress.Split(new char[] { '.' });
            for (int i = 0; i < 4; i++)
            {
                result <<= 8;
                result |= Int32.Parse(bytesArray[i]);

            }
            return result;
        }
    }
}
