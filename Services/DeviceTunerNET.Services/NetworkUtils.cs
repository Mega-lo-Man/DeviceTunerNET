using DeviceTunerNET.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services
{
    public class NetworkUtils : INetworkUtils
    {
        public bool SendMultiplePing(string NewIPAddr, int NumberOfRepetitions)
        {
            var _newIPAddr = NewIPAddr;
            var _numberOfRepetitions = NumberOfRepetitions;

            var counterGoodPing = 0;

            for (var i = 0; i < _numberOfRepetitions; i++)
            {
                if (SendPing(_newIPAddr))
                {
                    counterGoodPing++;
                }
                Thread.Sleep(50);
            }
            return counterGoodPing >= _numberOfRepetitions / 2;
        }

        public bool SendPing(string IpAddress)
        {
            var pingSender = new Ping();
            var options = new PingOptions
            {
                // Use the default Ttl value which is 128,
                // but change the fragmentation behavior.
                DontFragment = true
            };

            // Create a buffer of 32 bytes of data to be transmitted.
            var data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            var buffer = Encoding.ASCII.GetBytes(data);
            var timeout = 120;
            PingReply reply;
            try
            {
                reply = pingSender.Send(IpAddress, timeout, buffer, options);
                return reply.Status == IPStatus.Success;
            }
            catch (Exception ex)
            {
                Debug.Print("Ping exception: " + ex.Message);
                return false;
            }
        }
    }
}
