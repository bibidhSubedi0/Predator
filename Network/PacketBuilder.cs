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
        public void WriteOPCode(byte opcode)
        {
            _ms.WriteByte(opcode);
        }


        // This packet is going to be read as [OPCode|Length|Payload|] Sizes => [1|4|X] => 4 assuming length is INT
        public void WriteString(string msg)
        {
            int len = msg.Length;
            _ms.Write(BitConverter.GetBytes(len));
            _ms.Write(Encoding.ASCII.GetBytes(msg));
        }


        public byte[] GetCompletePacket()
        {
            return _ms.ToArray();
        }
    }
}