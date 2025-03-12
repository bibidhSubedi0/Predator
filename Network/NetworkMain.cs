using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    class NetworkMain
    {
        static void Main()
        {
            GameServer server = new GameServer();
            server.Start();
        }
    }
}
