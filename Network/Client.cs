using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    class Client
    {
        public string Username { get; set; }
        public Guid UserID {get;set;}

        public TcpClient ClientSocket { get; set; }

        PacketReader _pcaketReader;

        public Client(TcpClient client)
        {
            
            ClientSocket = client;
            UserID = Guid.NewGuid();
            _pcaketReader = new PacketReader(ClientSocket.GetStream());


            // We are assuming that when the client connects, the first thing it sends, is by deafult, a sting with its username
            Username = _pcaketReader.ReadMessage();
            Console.WriteLine($"{DateTime.Now}: Clinet Has connected with the username: {Username}");
        }

        public void ReadAnyData(TcpClient client)
        {
            _pcaketReader = new PacketReader(ClientSocket.GetStream());

            string someInt = _pcaketReader.ReadMessage();
            Console.WriteLine($"Yayyyy integer data also worksss {someInt}");
        }

    }
}
