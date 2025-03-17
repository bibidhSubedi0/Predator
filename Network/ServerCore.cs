using Network;
using PredatorApp.Net;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

class GameServer
{
    private TcpListener listener;
    private bool isRunning = false;
    List<Client> _ClientsList;
    public GameServer()
    {
        listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 5000);
        _ClientsList = new List<Client>();


    }

    public void Start()
    {
        isRunning = true;
        listener.Start();
        Console.WriteLine("Server started. Waiting for players...");

        while (isRunning)
        {
            TcpClient client = listener.AcceptTcpClient();
            var user = new Client(client);
            _ClientsList.Add(user);

            // Handel The Clinet Asynchronously
            // Now server can read data independintly from the clients
            Task.Run(() => user.HandelClient());
            user.MatchRequestedAction += HandelMatchmaking;

            // Display all the connected Clinets
            Console.WriteLine("All Avilable clinets ATM");
            foreach (Client c in _ClientsList)
            {
                Console.WriteLine("Username : " + c.Username + "\t UserID : " + c.UserID);
            }
        }
    }

    public void HandelMatchmaking(Guid UID)
    {
        Client requester = _ClientsList.Find(c => c.UserID == UID);

        Console.WriteLine("Match Request by " + requester.Username);
        
        while ((_ClientsList.Find(c => c.UserID == UID))?.InMatch == false)
        { 
            foreach(Client c in _ClientsList)
            {
                if(c.UserID!=UID && c.InMatch == false && c.MatchRequested==true)
                {
                    // Match them up!
                    // Idk how lol but match them up
                    Console.WriteLine(requester.Username+" :Match Foumd with: " + c.Username);
                    Console.WriteLine(c.Username + " :Match Foumd with: " + requester.Username);

                    Task.Run(() => (new HandelGame()).MainGame(requester, c));
                    return;

                }
            }

        }

        // Now goto handel Game
        
    }





}
