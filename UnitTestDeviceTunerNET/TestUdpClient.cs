using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections;

namespace UnitTestDeviceTunerNET
{
    [TestClass]
    public class TestUdpClient
    {
        private UdpClient _udpClient;
        private readonly IPAddress _remoteServerIp = IPAddress.Parse("127.0.0.1");
        private const int _remoteServerUdpPort = 12345;
        private const int _clientUdpPort = 54321;

        [TestInitialize]
        public void Setup()
        {
            _udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, _clientUdpPort));
        }

        [TestCleanup]
        public void Cleanup()
        {
            _udpClient?.Close();
        }

        [TestMethod]
        public void TestSendAndReceive()
        {
            // Arrange
            byte[] requestData = new byte[] { 1, 2, 3 };
            byte[] responseData = new byte[] { 4, 5, 6 };

            // Start a listener thread to receive the request and send the response
            var listenerTask = new Task(() =>
            {
                using (var server = new UdpClient(_remoteServerUdpPort))
                {
                    var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] receiveBuffer = server.Receive(ref remoteEndPoint);
                    server.Send(responseData, responseData.Length, remoteEndPoint);
                }
            });
            listenerTask.Start();

            // Act
            var client = new UdpClientWrapper(_udpClient, _remoteServerIp, _remoteServerUdpPort, _clientUdpPort);
            byte[] actualResponse = client.Send(requestData);

            // Assert
            Assert.IsNotNull(actualResponse);

            CollectionAssert.AreEqual(responseData, actualResponse);
        }
    }
}
