using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

class GameServer
{
    private TcpListener listener;
    private bool isRunning = false;
    List<TcpClient> _ClientsList;
    public GameServer()
    {
        listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 5000);
        _ClientsList = new List<TcpClient>();
    }

    public void Start()
    {
        isRunning = true;
        listener.Start();
        Console.WriteLine("Server started. Waiting for players...");

        while (isRunning)
        {
            TcpClient client = listener.AcceptTcpClient();
            _ClientsList.Add(client);
            Console.WriteLine("Player connected!");
        }
    }
}
