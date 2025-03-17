using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PredatorApp.Net;
using Predator.CoreEngine.Players;

namespace Network
{
    class Client
    {
        public string Username;
        public Guid UserID {get;set;}

        public TcpClient ClientSocket { get; set; }

        PacketReader _pcaketReader;

        public bool _isConnected = false;
        //public bool InMatch = false;
        //public bool MatchRequested = false;

        public event Action<Guid> MatchRequestedAction;



        private readonly object _lock = new object();
        private bool _inMatch;
        private bool _matchRequested;


        // All this backchodi because, these are shared resouces between both clients, so thread saftely issue
        public bool InMatch
        {
            get { 
                lock (_lock) { return _inMatch; } 
            }
            set { 
                lock (_lock) { _inMatch = value; } 
            }
        }

        public bool MatchRequested
        {
            get {
                lock (_lock) { return _matchRequested; } 
            }
            set { 
                lock (_lock) { _matchRequested = value; } 
            }
        }



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

            // Send Conformation to the client backvar connectPacket = new PacketBuilder();
            var connectPacket = new PacketBuilder();
            string str = "Connected Successfully!";
            connectPacket.WriteOPCode(0);
            connectPacket.WriteNumber4bytes(str.Length);
            connectPacket.WriteString(str);
            byte[] payload = connectPacket.GetCompletePacket();
            ClientSocket.Client.Send(payload);

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
                        msg += "Remaining Gats";
                        break;
               
                    case 3:
                        GoatPositions = _pcaketReader.ReadGoatPosition();
                        msg += "Goat Positions";
                        break;
                    case 4:
                        TigerPosition = _pcaketReader.ReadTigerPosition();
                        msg += "Tiger Position";
                        break;
                    //New Match Requested by the user
                    case 5:
                        if (!InMatch)
                        {
                            MatchRequested = true;
                            MatchRequestedAction?.Invoke(UserID);
                        }
                        break;
                    default:
                        msg += "Invalid Opcode, Currupted Packet";
                        break;

                }
                
                Console.WriteLine("Data Recived from client: "+ Username+ "  : "+ msg);
            }
        }


        public void SendStrings(string str)
        {
            //Send the username to the server
            var connectPacket = new PacketBuilder();
            connectPacket.WriteOPCode(0);
            connectPacket.WriteNumber4bytes(str.Length);
            connectPacket.WriteString(str);

            byte[] payload = connectPacket.GetCompletePacket();
            ClientSocket.Client.Send(payload);
        }

        public void SendTurn(bool turn)
        {
            var connectPacket = new PacketBuilder();
            connectPacket.WriteOPCode(1);
            connectPacket.WriteNumber4bytes(sizeof(bool));
            connectPacket.WriteBooleanValue(turn);

            byte[] payload = connectPacket.GetCompletePacket();
            ClientSocket.Client.Send(payload);
        }

        public void SendNoOfAvilableGoats(int goats)
        {

            var testpacket = new PacketBuilder();
            testpacket.WriteOPCode(2);
            testpacket.WriteNumber4bytes(sizeof(int));
            testpacket.WriteNumber4bytes(goats);

            byte[] payload = testpacket.GetCompletePacket();
            ClientSocket.Client.Send(payload);

        }

        public void SendGoatsInformation(Goat[] goats)
        {
            var testpacket = new PacketBuilder();
            testpacket.WriteOPCode(3);
            testpacket.WriteNumber4bytes(goats.Length * sizeof(int));
            testpacket.WriteGoats(goats);

            byte[] payload = testpacket.GetCompletePacket();

            ClientSocket.Client.Send(payload);
        }

        public void SendTigersInformation(Tiger[] tigers)
        {
            var testpacket = new PacketBuilder();
            testpacket.WriteOPCode(4);
            testpacket.WriteNumber4bytes(tigers.Length * sizeof(int));
            testpacket.WriteTigers(tigers);

            byte[] payload = testpacket.GetCompletePacket();
            ClientSocket.Client.Send(payload);
        }


    }
}
