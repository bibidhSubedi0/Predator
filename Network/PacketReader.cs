using System;
using System.Collections;
using System.Collections.Generic;
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
        public PacketReader(NetworkStream ns): base(ns)
        {
            _ns = ns;
        }
        public void FlushNetworkStream()
        {
            _ns.Flush();
        }

        // Packet Structure
        // [OPCode (1 byte)][Data Length (4 bytes)][Serialized Data (N bytes)]
        public string ReadMessage()
        {
            // First we read opcode for all the data
            int opcode = ReadByte();
            Console.WriteLine("Opcode read: ",opcode);

            // A noraml string
            if(opcode == 0) {
                string msg = "Username: ";
                byte[] msgBuffer;
                var length = ReadInt32();
                msgBuffer = new byte[length];
                _ns.Read(msgBuffer, 0, length);
                msg += Encoding.ASCII.GetString(msgBuffer);
                return msg;
            }

            // Whose turn it is
            if(opcode == 1)
            {
                string msg = "Processing Turn ";
                Console.WriteLine(msg);

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
                Console.WriteLine($"Received {turn}");

                return "Successfully processed Turn";

            }

            // Number of avilable goats 
            if (opcode == 2)
            {
                string msg = "Processing No. Of avilable Goats";
                Console.WriteLine(msg);


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
                var go = BitConverter.ToInt32(AvilableGoatsByte,0);

                // 5. Process data
                Console.WriteLine($"Remaning Goats {go}");

                return "Successfully processed avilable goats";
            }

            // Goats information 

            if (opcode == 3)
            {
                try
                {
                    Console.WriteLine("Processing Goats Information...");

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
                    Console.WriteLine($"Received {positions.Length} goat positions:");
                    foreach (int position in positions)
                    {
                        // Just print for now!
                        Console.WriteLine(position);
                    }

                    return "Successfully received goat positions";
                }

                catch (EndOfStreamException ex)
                {
                    return $"Connection lost while reading goat data: {ex.Message}";
                }
                catch (ProtocolViolationException ex)
                {
                    Console.WriteLine($"Protocol error: {ex.Message}");
                    return "Invalid goat data format";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex}");
                    return "Goat data processing failed";
                }
            }


            // Tigers information 
            if (opcode == 4)
            {
                try
                {
                    Console.WriteLine("Processing Tigers Information...");

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
                    Console.WriteLine($"Received {positions.Length} goat positions:");
                    foreach (int position in positions)
                    {
                        // Just print for now!
                        Console.WriteLine(position);
                    }

                    return "Successfully received Tiger positions";
                }

                catch (EndOfStreamException ex)
                {
                    return $"Connection lost while reading goat data: {ex.Message}";
                }
                catch (ProtocolViolationException ex)
                {
                    Console.WriteLine($"Protocol error: {ex.Message}");
                    return "Invalid goat data format";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex}");
                    return "Goat data processing failed";
                }
            }
            return "Invalid Packet";

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
