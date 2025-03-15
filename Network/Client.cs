using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    class Client
    {
        public string Username;
        public Guid UserID {get;set;}

        public TcpClient ClientSocket { get; set; }

        PacketReader _pcaketReader;

        public bool _isConnected = false;

        // Also need to actually store the data from the clients
        // For now lets just store the relevent informations and not build back the objects
        bool turn;
        int AvilableGoats;
        int[] GoatPositions;
        int[] TigerPosition;


        public Client(TcpClient client)
        {
            
            ClientSocket = client;
            UserID = Guid.NewGuid();
            _pcaketReader = new PacketReader(ClientSocket.GetStream());


            // We are assuming that when the client connects, the first thing it sends, is by deafult, a sting with its username
            Username = _pcaketReader.ReadUsername();
            Console.WriteLine($"{DateTime.Now}: Clinet Has connected with the username: {Username}");
            _pcaketReader.FlushNetworkStream();
            _isConnected = true;
        }

        public void HandelClient()
        {
            // Always recive data
            while (_isConnected) {
                // First we read opcode for all the data
                byte opcode = _pcaketReader.ReadByte();
                string msg = "";
                // Read the specific thing on the basis of opcode
                switch (opcode)
                {
                    // For usename [for now]
                    case 0:
                        Username = _pcaketReader.ReadUsername();
                        msg += "Username";
                        break;
                    // For turn
                    case 1:
                        turn = _pcaketReader.ReadTurn();
                        msg += "Turn";
                        break;
                    // For Remaining Goats
                    case 2:
                        AvilableGoats = _pcaketReader.ReadRemainingGoats();
                        msg += "Remaining GOats";
                        break;
               
                    case 3:
                        GoatPositions = _pcaketReader.ReadGoatPosition();
                        msg += "Goat Positions";
                        break;
                    case 4:
                        TigerPosition = _pcaketReader.ReadTigerPosition();
                        msg += "Tiger Position";
                        break;
                    default:
                        msg += "Invalid Opcode, Currupted Packet";
                        break;

                }
                Console.WriteLine("Data Recived from client: "+ msg);
                _pcaketReader.FlushNetworkStream();
            }
        }


    }
}
