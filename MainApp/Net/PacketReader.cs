using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    class PacketReader : BinaryReader
    {
        NetworkStream _ns;
        public PacketReader(NetworkStream ns) : base(ns)
        {
            _ns = ns;
        }
        public void FlushNetworkStream()
        {
            _ns.Flush();
        }

        // Packet Structure
        // [OPCode (1 byte)][Data Length (4 bytes)][Serialized Data (N bytes)]

        public string ReadUsername()
        {
            byte[] msgBuffer;
            var length = ReadInt32();
            msgBuffer = new byte[length];
            _ns.Read(msgBuffer, 0, length);
            return Encoding.ASCII.GetString(msgBuffer);
        }

        public bool ReadTurn()
        {
            // 1. Read byte length header
            byte[] lengthBuffer = new byte[4];
            ReadFully(_ns, lengthBuffer, 0, 4);
            int byteLength = BitConverter.ToInt32(lengthBuffer, 0);

            // 2. Validate payload size
            if (byteLength % 1 != 0)
                throw new ProtocolViolationException($"Invalid data length: {byteLength}");

            // 3. Read payload
            byte[] turnbytes = new byte[byteLength];
            ReadFully(_ns, turnbytes, 0, byteLength);

            // Convert bytes
            var turn = BitConverter.ToBoolean(turnbytes);

            // 5. Process data
            return turn;
        }

        public int ReadRemainingGoats()
        {
            // 1. Read byte length header
            byte[] lengthBuffer = new byte[4];
            ReadFully(_ns, lengthBuffer, 0, 4);
            int byteLength = BitConverter.ToInt32(lengthBuffer, 0);

            // 2. Validate payload size
            //if (byteLength % 4 != 0)
            //    throw new ProtocolViolationException($"Invalid data length: {byteLength}");

            // 3. Read payload
            byte[] AvilableGoatsByte = new byte[byteLength];
            ReadFully(_ns, AvilableGoatsByte, 0, byteLength);

            // Convert bytes
            var go = BitConverter.ToInt32(AvilableGoatsByte, 0);

            // 5. Process data
            return go;
        }

        public int[] ReadGoatPosition()
        {
            // 1. Read byte length header
            byte[] lengthBuffer = new byte[4];
            ReadFully(_ns, lengthBuffer, 0, 4);
            int byteLength = BitConverter.ToInt32(lengthBuffer, 0);

            // 2. Validate payload size
            if (byteLength % 4 != 0)
                throw new ProtocolViolationException($"Invalid data length: {byteLength} bytes (must be multiple of 4)");

            // 3. Read payload
            byte[] goatBytes = new byte[byteLength];
            ReadFully(_ns, goatBytes, 0, byteLength);

            // 4. Convert to int array efficiently
            int[] positions = new int[byteLength / 4];
            Buffer.BlockCopy(goatBytes, 0, positions, 0, byteLength);

            // 5. Process positions
            return positions;

        }

        public int[] ReadTigerPosition()
        {
            // 1. Read byte length header
            byte[] lengthBuffer = new byte[4];
            ReadFully(_ns, lengthBuffer, 0, 4);
            int byteLength = BitConverter.ToInt32(lengthBuffer, 0);

            // 2. Validate payload size
            if (byteLength % 4 != 0)
                throw new ProtocolViolationException($"Invalid data length: {byteLength} bytes (must be multiple of 4)");

            // 3. Read payload
            byte[] tigerBytes = new byte[byteLength];
            ReadFully(_ns, tigerBytes, 0, byteLength);

            // 4. Convert to int array efficiently
            int[] positions = new int[byteLength / 4];
            Buffer.BlockCopy(tigerBytes, 0, positions, 0, byteLength);

            // 5. Process positions
            return positions;
        }


        // Helper method to guarantee complete read
        private void ReadFully(NetworkStream stream, byte[] buffer, int offset, int count)
        {
            int totalRead = 0;
            while (totalRead < count)
            {
                int bytesRead = stream.Read(buffer, offset + totalRead, count - totalRead);
                if (bytesRead == 0) throw new EndOfStreamException("Unexpected end of stream");
                totalRead += bytesRead;
            }
        }

    }
}
