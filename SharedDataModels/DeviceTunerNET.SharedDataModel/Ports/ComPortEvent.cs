using DeviceTunerNET.SharedDataModel.Devices;
using DeviceTunerNET.SharedDataModel.Utils;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace DeviceTunerNET.SharedDataModel.Ports
{
    public class ComPortEvent : IPort
    {
        private static List<byte> _readBuffer = new();// Буфер чтения для RS485
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

            // CompletePacket = Packet - CRC8
            if (packetLength != _readBuffer.Count() - 1)
                return false;

            if (OrionCRC.IsCrcValid(_readBuffer))
                return true;

            return false;
        }

        public SerialPort SerialPort { get; set; }
        public int MaxRepetitions { get; set; } = 15;
        public int Timeout { get; set; } = 1;

        public byte[] Send(byte[] command)
        {
            // make DataReceived event handler
            SerialPort.DataReceived += Sp_DataReceived;

            for (int i = 0; i < MaxRepetitions; i++)
            {
                _readBuffer.Clear();
                SendPacketWithCrc(command);

                var timeCounter = (int)IOrionNetTimeouts.Timeouts.notResponse;
                while ((IsReceivePacketComplete() != true) && (timeCounter >= 0))
                {
                    Thread.Sleep(Timeout);
                    timeCounter -= Timeout;
                }

                if (IsReceivePacketComplete())
                    break;

            }

            if (_readBuffer.Count == 0)
                throw new Exception("Device is not responding!");

            return _readBuffer.ToArray();

        }

        public void SendWithoutСonfirmation(byte[] command)
        {
            SendPacketWithCrc(command);
        }

        private void SendPacketWithCrc(byte[] command)
        {
            // Orion-RS485 require to send two packets "Command + CRC"

            //send command
            SerialPort.Write(command, 0, command.Length);

            var crc = OrionCRC.GetCrc8(command);

            // send CRC
            SerialPort.Write(crc, 0, crc.Length);
        }

        private static void Sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var sPort = (SerialPort)sender;

            var tempBuffer = new byte[sPort.BytesToRead];
            sPort.Read(tempBuffer, 0, tempBuffer.Length);

            foreach (byte b in tempBuffer)
            {
                _readBuffer.Add(b);
            }
        }
    }
}
