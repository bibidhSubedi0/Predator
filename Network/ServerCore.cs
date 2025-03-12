using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

class GameServer
{
    private TcpListener listener;
    private bool isRunning = false;

    public GameServer()
    {
        listener = new TcpListener(IPAddress.Any, 5000);
    }

    public void Start()
    {
        isRunning = true;
        listener.Start();
        Console.WriteLine("Server started. Waiting for players...");

        while (isRunning)
        {
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Player connected!");
            Thread clientThread = new Thread(() => HandleClient(client));
            clientThread.Start();
        }
    }

    private void HandleClient(TcpClient client)
    {
        Console.WriteLine("Handling new player...");
    }
}
