using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
                string msg = "Turn: ";
                byte[] numberBuffer = new byte[4];
                _ns.Read(numberBuffer, 0, 4);
                int number = BitConverter.ToInt32(numberBuffer, 0);

                return msg+number.ToString();
            }

            // Number of avilable goats 
            if (opcode == 2)
            {
                string msg = "No. of avilableGoats: ";
                byte[] numberBuffer = new byte[4];
                _ns.Read(numberBuffer, 0, 4);
                int number = BitConverter.ToInt32(numberBuffer, 0);
                return msg+number.ToString();
            }

            // Goats information 
            // Packet Structure
            // [OPCode (1 byte)][Data Length (4 bytes)][Serialized Data (N bytes)]
            if (opcode == 3)
            {
                Console.WriteLine("Goats Information: ");

                // Size of this goats array, should be 20
                byte[] numberBuffer = new byte[4];
                _ns.Read(numberBuffer, 0, 4);
                int size =BitConverter.ToInt32(numberBuffer, 0);

                // Because again, size are stored in bytes, so 4 bytes= 1 int = 1 position
                size = size * 4;
                byte[] goatpos = new byte[size];

                // Now read the actual goat positions
                _ns.Read(goatpos, 0, size);

                // Convert those bytes arrays into actual int positions
                if (goatpos.Length % 4 != 0)
                    throw new ArgumentException("Byte array length must be divisible by 4");

                int[] intArray = new int[goatpos.Length / 4];
                for (int i = 0; i < intArray.Length; i++)
                {
                    intArray[i] = BitConverter.ToInt32(goatpos, i * 4);
                    Console.WriteLine(intArray[i].ToString());
                }
                return "Goats information recived! ";
            }

            // Tigers information 
            if (opcode == 4)
            {
                string msg = "Tiger Information: ";
                byte[] numberBuffer = new byte[4];
                _ns.Read(numberBuffer, 0, 4);
                int number = BitConverter.ToInt32(numberBuffer, 0);
                return msg+number.ToString();
            }
            return "Invalid Packet";

        }

    }
}
