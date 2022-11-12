using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.SharedDataModel.Utils
{
    public static class NetHelper
    {
        private const int ValidMacStringLength = 12;

        public static byte[] GetBytesFromMacAddress(string MacString)
        {
            if (string.IsNullOrEmpty(MacString)) 
                return new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            if (MacString.Count(x => x == ':') == 5)
            {
                return MacString.Split(':').Select(x => Convert.ToByte(x, 16)).ToArray();
            }

            if (MacString.Count(x => x == '-') == 5)
            {
                return MacString.Split('-').Select(x => Convert.ToByte(x, 16)).ToArray();
            }

            if (MacString.Length == ValidMacStringLength)
            {
                return Enumerable.Range(0, MacString.Length)
                    .GroupBy(x => x / 2)
                    .Select(x => new string(x.Select(y => MacString[y]).ToArray()))
                    .Select(x => Convert.ToByte(x, 16))
                    .ToArray();
            }
            throw new Exception("MAC address not valid: " + MacString);
        }

    }
}
