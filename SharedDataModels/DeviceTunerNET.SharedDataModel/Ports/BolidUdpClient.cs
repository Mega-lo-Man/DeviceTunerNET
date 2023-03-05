using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.SharedDataModel.Ports
{

    public class BolidUdpClient : IPort
    {
        private const int DEFAULT_MAX_REPETITIONS = 15;
        private const int DEFAULT_TIMEOUT = 60;
        private UdpClient _udpClient;

        public int MaxRepetitions { get; set; }

        public IPAddress RemoteServerIp { get; set; }

        public int RemoteServerUdpPort { get; set; }

        public int ClientUdpPort { get; set; }

        public int Timeout { get; set; }

        public BolidUdpClient(int clientUdpPort)
        {
            ClientUdpPort = clientUdpPort;
            MaxRepetitions = DEFAULT_MAX_REPETITIONS;
            Timeout = DEFAULT_TIMEOUT;
            _udpClient = new UdpClient(ClientUdpPort);
        }

        public byte[] Send(byte[] data)
        {
            int attempts = 0;
            var remoteEndPoint = new IPEndPoint(RemoteServerIp, RemoteServerUdpPort);
            byte[] receiveBuffer = null;

            while (attempts < MaxRepetitions)
            {
                attempts++;
                _udpClient.Send(data, data.Length, remoteEndPoint);
                receiveBuffer = _udpClient.Receive(ref remoteEndPoint);
                if (receiveBuffer != null)
                {
                    break;
                }
            }

            if (receiveBuffer == null)
            {
                throw new TimeoutException("Timeout waiting for server response.");
            }

            return receiveBuffer;
        }

    }
}
