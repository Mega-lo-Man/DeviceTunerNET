using DeviceTunerNET.SharedDataModel.Devices;
using DeviceTunerNET.SharedDataModel.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace DeviceTunerNET.SharedDataModel.Ports
{
    public class ComPort : IPort
    {
        private const int packetLengthIndex = 1;
        private const int crcLength = 1;
        private static List<byte> _readBuffer = new();// Буфер чтения для RS485
        private static byte[] _addressLengthBuffer = new byte[2];
        private static byte[] _dataBuffer;
        protected byte[] _sendData; // Данные для отправки в порт

        /// <summary>
        /// Method  define how to find the end of packet CRC8 or another sign
        /// </summary>
        /// <returns>Return true if the end of the packet has found</returns>
        protected bool IsReceivePacketComplete()
        {
            if (_readBuffer.Count() < 2)
                return false;

            var packetLength = _readBuffer.ElementAt(1);

            // CompletePacket = Packet + CRC8 = Packet + 1
            if (_readBuffer.Count() < packetLength + 1)
                return false;

            var packets = PacketsHelper.GetPackets(_readBuffer);

            foreach (var packet in packets) 
            {
                if (!OrionCRC.IsCrcValid(packet))
                    return false;
            }
            return true;
        }

        public SerialPort SerialPort { get; set; }
        public int MaxRepetitions { get; set; } = 15;
        public int Timeout { get; set; } = 1;


        public byte[] Send(byte[] data)
        {
            SerialPort.ReadTimeout = Timeout;

            for (int i = 0; i < MaxRepetitions; i++)
            {
                SerialPort.DiscardOutBuffer();
                SerialPort.DiscardInBuffer();

                _readBuffer.Clear();

                SendPacketWithCrc(data);

                var packetLength = GetPacketLength();

                AddDataRemnants(packetLength);

                if (IsReceivePacketComplete())
                {
                    
                    return _readBuffer.ToArray();
                }
            }
            return new byte[0];
        }

        private void AddDataRemnants(byte packetLength)
        {
            WaitingForPacketEnd(packetLength);

            _dataBuffer = new byte[packetLength + crcLength - 2];

            SerialPort.Read(_dataBuffer, 0, packetLength - 1);

            _readBuffer.AddRange(_dataBuffer.Take(_dataBuffer.Count()));
        }

        private byte GetPacketLength()
        {
            WaitingForPacketLengthByte();

            SerialPort.Read(_addressLengthBuffer, 0, 2);

            _readBuffer.AddRange(_addressLengthBuffer.Take(_addressLengthBuffer.Count()));

            return _readBuffer[packetLengthIndex];
        }

        private void WaitingForPacketEnd(byte packetLength)
        {
            while(SerialPort.BytesToRead <= packetLength - 2)
            {
                //Thread.Sleep(1);
            }
        }

        private void WaitingForPacketLengthByte()
        {
            while (SerialPort.BytesToRead <= 2)
            {
                //Thread.Sleep(1);
            }
        }

        private IEnumerable<byte> GetBytes(string buffer)
        {
            foreach(char c in buffer)
            {
                yield return Convert.ToByte(c);
            }    
        }

        /*
        public byte[] Send(byte[] command)
        {
            SerialPort.ReadTimeout = Timeout;

            // make DataReceived event handler
            SerialPort.DataReceived += Sp_DataReceived;
            
            for (int i = 0; i < MaxRepetitions; i++)
            {
                _readBuffer.Clear();
                SendPacketWithCrc(command);

                var timeCounter = Timeout;//(int)IOrionNetTimeouts.Timeouts.notResponse;
                while ((IsReceivePacketComplete() != true) && (timeCounter >= 0))
                {
                    Thread.Sleep(2);
                    timeCounter -= 1;
                }

                if (IsReceivePacketComplete())
                    break;
            }
            
            SerialPort.DataReceived -= Sp_DataReceived;
            
            if (_readBuffer.Count == 0)
                throw new Exception("Device is not responding!");

            return _readBuffer.ToArray();
        }
        /*
        public byte[] Read()
        {

        }
        */

        public void SendWithoutСonfirmation(byte[] command)
        {
            SendPacketWithCrc(command);
        }

        private void SendPacketWithCrc(byte[] command)
        {
            // Orion-RS485 require to send two packets "Command + CRC"

            //send command
            SerialPort.Write(command, 0, command.Length );

            var crc = OrionCRC.GetCrc8(command);

            // send CRC
            SerialPort.Write(crc, 0, crc.Length); 
        }

        private static void Sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var sPort = (SerialPort)sender;

            var tempBuffer = new byte[sPort.BytesToRead];
            sPort.Read(tempBuffer, 0, tempBuffer.Length);

            _readBuffer = tempBuffer.ToList();
        }  
    }
}
