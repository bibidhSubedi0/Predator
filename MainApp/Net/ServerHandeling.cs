using Network;
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
        PacketReader _packetReader;
        public bool _isConnected;
        string _Username = String.Empty;

        // Information read form server
        bool turn;
        int AvilableGoats = 20;
        int[] GoatPositions = new int[20];
        int[] TigerPosition = { 1, 5, 21, 25 };
        string Message = String.Empty;

        public event Action<int[], int[], int, bool>? NewStateRecived;
        public event Action<bool>? NewMatchRecived;
        public event Action<string>? LogMessageNet;
        SemaphoreSlim MatchFound = new SemaphoreSlim(0);
        SemaphoreSlim UpdateFromServer = new SemaphoreSlim(0);

        public ServerHandeling()
        {
            _client = new TcpClient();
            //_packetbuilder = new PacketBuilder();
            //_packetReader = new PacketReader(_client.GetStream());
        }

        public void ReadPackets()
        {

            LogMessageNet?.Invoke("Listining to server!");

            while (_isConnected)
            {
                byte opcode = _packetReader.ReadByte();
                string msg = "";
                switch (opcode)
                {
                    // For usename [for now]
                    case 0:
                        Message = _packetReader.ReadUsername();
                        msg += "Text Message : " + Message;
                        break;
                    // For turn
                    case 1:
                        turn = _packetReader.ReadTurn();
                        msg += "Turn";
                        NewStateRecived?.Invoke(TigerPosition, GoatPositions, AvilableGoats, turn);
                        break;
                    // For Remaining Goats
                    case 2:
                        AvilableGoats = _packetReader.ReadRemainingGoats();
                        msg += "Remaining GOats";
                        NewStateRecived?.Invoke(TigerPosition, GoatPositions, AvilableGoats, turn);
                        break;

                    case 3:
                        GoatPositions = _packetReader.ReadGoatPosition();
                        msg += "Goat Positions";
                        NewStateRecived?.Invoke(TigerPosition, GoatPositions, AvilableGoats, turn);
                        break;
                    case 4:
                        TigerPosition = _packetReader.ReadTigerPosition();
                        msg += "Tiger Position";
                        NewStateRecived?.Invoke(TigerPosition, GoatPositions, AvilableGoats, turn);
                        break;
                    // Match found from server and server assigned a turn value
                    case 5:
                        turn = _packetReader.ReadTurn();
                        msg += turn;
                        NewMatchRecived?.Invoke(turn);
                        NotifyMatchFind();
                        break;
                    default:
                        msg += "Invalid Opcode, Currupted Packet";
                        break;
                }

                // Relay these info to the core game 
                //NotifyStateUpdate();





                //LogMessageNet?.Invoke("Server Message : "+msg);
                _packetReader.FlushNetworkStream();
            }

        }
        public void ConnectToServer(string Username)
        {
            if (!_isConnected)
            {
                _Username = Username;
                _client.Connect("127.0.0.1", 5000);
                _isConnected = true;

                _packetReader = new PacketReader(_client.GetStream());

                // Send username immidiately affter connecting to the server
                SendStrings(Username);
            }
        }

        public void NotifyMatchFind()
        {
            MatchFound.Release();
        }

        public void NotifyStateUpdate()
        {
            UpdateFromServer.Release();
        }



        public async Task RequestMatch()
        {
            var RequsetPacket = new PacketBuilder();
            RequsetPacket.WriteOPCode(5);
            byte[] payload = RequsetPacket.GetCompletePacket();
            _client.Client.Send(payload);

            // Check for confirmation form the server!
            await MatchFound.WaitAsync();

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

        public async Task SendTurn(bool turn)
        {
            var connectPacket = new PacketBuilder();
            connectPacket.WriteOPCode(1);
            connectPacket.WriteNumber4bytes(sizeof(bool));
            connectPacket.WriteBooleanValue(turn);

            byte[] payload = connectPacket.GetCompletePacket();
            await _client.Client.SendAsync(payload, SocketFlags.None);
        }

        public async Task SendNoOfAvilableGoats(int goats)
        {

            var testpacket = new PacketBuilder();
            testpacket.WriteOPCode(2);
            testpacket.WriteNumber4bytes(sizeof(int));
            testpacket.WriteNumber4bytes(goats);

            byte[] payload = testpacket.GetCompletePacket();
            await _client.Client.SendAsync(payload, SocketFlags.None); // Async send

        }

        public async Task SendGoatsInformation(Goat[] goats)
        {
            var testpacket = new PacketBuilder();
            testpacket.WriteOPCode(3);
            testpacket.WriteNumber4bytes(goats.Length * sizeof(int));
            testpacket.WriteGoats(goats);

            byte[] payload = testpacket.GetCompletePacket();
            await _client.Client.SendAsync(payload, SocketFlags.None); // Async send
        }

        public async Task SendTigersInformation(Tiger[] tigers)
        {
            var testpacket = new PacketBuilder();
            testpacket.WriteOPCode(4);
            testpacket.WriteNumber4bytes(tigers.Length * sizeof(int));
            testpacket.WriteTigers(tigers);

            byte[] payload = testpacket.GetCompletePacket();
            await _client.Client.SendAsync(payload, SocketFlags.None);
        }


    }
}
