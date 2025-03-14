using Predator.CoreEngine.Players;
using System.IO;
using System.Text;

namespace PredatorApp.Net
{
    class PacketBuilder
    {
        MemoryStream _ms;
        public PacketBuilder()
        {
            _ms = new MemoryStream();

        }

        // 1 byte = 256 unique types of opcods, more the enough
        // Each packet will start with an opcode
        /*
         *  0 -> Username
         *  1 -> Turn
         *  2 -> No. of avilable goats
         *  3 -> all the aviable goats
         *  4 -> Tiger position
         */
        public void WriteOPCode(byte opcode) 
        {
            _ms.WriteByte(opcode);
        }
        public void WriteString(string msg)
        {
            _ms.Write(Encoding.ASCII.GetBytes(msg));
        }

        public void WriteNumber4bytes(int num)
        {
            _ms.Write(BitConverter.GetBytes(num));
        }

        public void WriteBooleanValue(bool val)
        {
            _ms.Write(BitConverter.GetBytes(val));
        }

        public void WriteGoats(Goat[] goats)
        {

            foreach (Goat g in goats)
            {
                if (g != null) { 
                    WriteNumber4bytes(g.position);
                }
                else
                {
                    WriteNumber4bytes(0);
                }
            }

        }
        public void WriteTigers(Tiger[] tigers)
        {
            foreach (Tiger t in tigers)
            {
                WriteNumber4bytes(t.position);
            }
        }

        public byte[] GetCompletePacket()
        {
            return _ms.ToArray();
        }
    }
}
/*
    All the infromations needed to be send to the server and vice versa 
    
    -> Client information
        -> Username 
    
    -> Gane information
        -> tigers position
        -> turn
        -> avilableGoats
        -> All the goats
        
 */
