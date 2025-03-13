using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PredatorApp.Net
{
    class ServerHandeling
    {
        TcpClient _client;
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
            }
        }
    }
}
