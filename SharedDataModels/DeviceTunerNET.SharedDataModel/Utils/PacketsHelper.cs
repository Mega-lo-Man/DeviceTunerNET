using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Utils
{
    public static class PacketsHelper
    {
        public static IEnumerable<byte[]> GetPackets(IEnumerable<byte> readBuffer)
        {
            var source = readBuffer.ToList();

            while (source.Count >= 3)
            {
                var packetLength = source[1];
                var crcLength = 1;

                var packet = new List<byte>();

                packet.AddRange(source.Take(packetLength + crcLength));
                source.RemoveRange(0, packet.Count());

                yield return packet.ToArray();
            }
        }
    }
}
