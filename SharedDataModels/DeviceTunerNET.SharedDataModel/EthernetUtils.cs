using static System.String;

namespace DeviceTunerNET.SharedDataModel
{
    public static class EthernetUtils
    {
        public static string CidrToString(int cidr)
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

        public static int ConvertToCidr(string address)
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

        public static int ConvertStringToRange(string addrStr)
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
