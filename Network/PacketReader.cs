using System;
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

        public string ReadMessage()
        {
            var opcode = ReadByte();

            if(opcode == 0) { 
                byte[] msgBuffer;
                var length = ReadInt32();
                msgBuffer = new byte[length];
                _ns.Read(msgBuffer, 0, length);

                var msg = Encoding.ASCII.GetString(msgBuffer);
                return msg;
            }
            if(opcode == 1)
            {
                byte[] numberBuffer = new byte[4];
                _ns.Read(numberBuffer, 0, 4);
                int number = BitConverter.ToInt32(numberBuffer, 0);
                return number.ToString();
            }
            return "Invalid Packet";

        }

    }
}
