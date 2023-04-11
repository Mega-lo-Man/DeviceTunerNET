using DeviceTunerNET.SharedDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestDeviceTunerNET
{
    public class UdpClientWrapper : IPort
    {
        private UdpClient _udpClient;
        private int _maxRepetitions = 3;
        private IPAddress _remoteServerIp;
        private int _remoteServerUdpPort;
        private int _clientUdpPort;

        public UdpClientWrapper(UdpClient udpClient, IPAddress remoteServerIp, int remoteServerUdpPort, int clientUdpPort)
        {
            _udpClient = udpClient;
            _remoteServerIp = remoteServerIp;
            _remoteServerUdpPort = remoteServerUdpPort;
            _clientUdpPort = clientUdpPort;
        }

        public int MaxRepetitions
        {
            get { return _maxRepetitions; }
            set { _maxRepetitions = value; }
        }

        public IPAddress RemoteServerIp
        {
            get { return _remoteServerIp; }
            set { _remoteServerIp = value; }
        }

        public int RemoteServerUdpPort
        {
            get { return _remoteServerUdpPort; }
            set { _remoteServerUdpPort = value; }
        }

        public int ClientUdpPort
        {
            get { return _clientUdpPort; }
            set { _clientUdpPort = value; }
        }

        public int Timeout { get; set; }

        public byte[] Send(byte[] data)
        {
            // Send the data to the remote server
            _udpClient.Send(data, data.Length, _remoteServerIp.ToString(), _remoteServerUdpPort);

            // Receive the response from the server
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] receiveBuffer = _udpClient.Receive(ref remoteEndPoint);

            return receiveBuffer;
        }

        public void SendWithoutСonfirmation(byte[] data)
        {
            throw new NotImplementedException();
        }
    }

}
