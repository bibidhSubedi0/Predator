using Predator.CoreEngine.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PredatorApp.Net
{
    class ServerHandeling
    {
        TcpClient _client;
        PacketBuilder _packetbuilder;
        public bool _isConnected { get; set; }
        string _Username = String.Empty;

        public ServerHandeling()
        {
            _client = new TcpClient();
        }

        public void ConnectToServer(string Username)
        {
            if (!_isConnected) {
                _Username = Username;
                _client.Connect("127.0.0.1", 5000);
                _isConnected = true;

                // Send username immidiately affter connecting to the server
                SendStrings(Username);
            }
        }

        // Packet Structure
        // [OPCode (1 byte)][Data Length (4 bytes)][Serialized Data (N bytes)]

        public void SendStrings(string str)
        {
            //Send the username to the server
            var connectPacket = new PacketBuilder();
            connectPacket.WriteOPCode(0);
            connectPacket.WriteNumber4bytes(str.Length);
            connectPacket.WriteString(str);

            byte[] payload = connectPacket.GetCompletePacket();
            _client.Client.Send(payload);
        }

        public void SendTurn(bool turn)
        {
            var connectPacket = new PacketBuilder();
            connectPacket.WriteOPCode(1);
            connectPacket.WriteNumber4bytes(sizeof(bool));
            connectPacket.WriteBooleanValue(turn);

            byte[] payload = connectPacket.GetCompletePacket();
            _client.Client.Send(payload);
        }

        public void SendNoOfAvilableGoats(int goats)
        {
            
            var testpacket = new PacketBuilder();
            testpacket.WriteOPCode(2);
            testpacket.WriteNumber4bytes(sizeof(int));
            testpacket.WriteNumber4bytes(goats);

            byte[] payload = testpacket.GetCompletePacket();
            _client.Client.Send(payload);
            
        }

        public void SendGoatsInformation(Goat[] goats)
        {
            var testpacket = new PacketBuilder();
            testpacket.WriteOPCode(3);
            testpacket.WriteNumber4bytes(goats.Length * sizeof(int));
            testpacket.WriteGoats(goats);

            byte[] payload = testpacket.GetCompletePacket();
            
            _client.Client.Send(payload);
        }

        public void SendTigersInformation(Tiger[] tigers)
        {
            var testpacket = new PacketBuilder();
            testpacket.WriteOPCode(4);
            testpacket.WriteNumber4bytes(tigers.Length * sizeof(int));
            testpacket.WriteTigers(tigers);

            byte[] payload = testpacket.GetCompletePacket();
            _client.Client.Send(payload);
        }




    }
}
