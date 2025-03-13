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

        public ServerHandeling()
        {
            _client = new TcpClient();
        }

        public void ConnectToServer()
        {
            if (!_isConnected) { 
                _client.Connect("127.0.0.1", 5000);
                _isConnected = true;
            }
        }
    }
}
