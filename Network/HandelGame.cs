using Predator.CoreEngine.Players;
using PredatorApp.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    class HandelGame
    {
        // For each game 
        /*
         *  Assign role to 2 players randomly [1 Goat and 1 Tiger]
         *  Store the necessery information of the game
         *  Give first turn to goat player
         *  Wait for goat move from the first player -> Lock the board to tiger player till then? IDK
         *  Goat Player Places a piece on the board -> Game state for that player changes -> Sendes the info to server -> 
         *  Server Switches turn for the tiger player and sends the info to Tiger Player -> Game State for that player chagnes -> Repeat
         *  
         */

        int[] _TigerPositons = new int[4];
        int[] _GoatPositions =  new int[20];
        bool Turn = false; // True =  Triger
        int avilableGoats;
        
        
        Tiger[] temptigers;
        Goat[] tempgoat;

        public SemaphoreSlim C1MoveRecived = new SemaphoreSlim(0);
        public SemaphoreSlim C2MoveRecived = new SemaphoreSlim(0);


        public HandelGame()
        {  
            
        }


        void UpdateObejects(int[] tigerPositions, int[] goatPositions, int remainingGoats)
        {
            // Construct tigers
            temptigers = new Tiger[4];
            for (int i = 0; i < temptigers.Length; i++)
            {
                temptigers[i] = new Tiger(tigerPositions[i]);
            }


            // Construct goats
            tempgoat = new Goat[20];
            for (int i = 0; i < goatPositions.Length; i++)
            {
                if (goatPositions[i] == 0)
                {
                    tempgoat[i] = null;
                }
                else
                {
                    tempgoat[i] = new Goat(goatPositions[i]);
                }
            }

            avilableGoats = remainingGoats;
        }

        public async Task MainGame(Client c1, Client c2)
        {

            Console.WriteLine("Handling the Game for " + c1.Username + " and " + c2.Username);
            c1.NewStateRecived += C1Move;
            c2.NewStateRecived += C2Move;


            // Initilize a new Game State for both of them

            int[] tigerPositions = { 1, 15, 21,20 };
            int[] goatPosition = new int[20];
            int remainingGoats = 20;
            UpdateObejects(tigerPositions, goatPosition, remainingGoats);



            // Send confirmation to both of the players along with their assigned turn

            // Send false =  Goat to c2
            var ConfirmationPacket1 = new PacketBuilder();
            ConfirmationPacket1.WriteOPCode(5);
            ConfirmationPacket1.WriteNumber4bytes(sizeof(bool));
            ConfirmationPacket1.WriteBooleanValue(Turn);
            byte[] payload1 = ConfirmationPacket1.GetCompletePacket();
            c2.ClientSocket.Client.Send(payload1);

            // Send true = Tigers to c1
            var ConfirmationPacket2 = new PacketBuilder();
            ConfirmationPacket2.WriteOPCode(5);
            ConfirmationPacket2.WriteNumber4bytes(sizeof(bool));
            ConfirmationPacket2.WriteBooleanValue(!Turn);
            byte[] payload2= ConfirmationPacket2.GetCompletePacket();
            c1.ClientSocket.Client.Send(payload2);

            // I probably need some kind of handshake here!? IDK, I do not know network programming that good yet

            //Also send all the remaining Information
            await Task.WhenAll(
                c1.SendTigersInformation(temptigers),
                c1.SendGoatsInformation(tempgoat),
                c1.SendNoOfAvilableGoats(avilableGoats),
                c2.SendTigersInformation(temptigers),
                c2.SendGoatsInformation(tempgoat),
                c2.SendNoOfAvilableGoats(avilableGoats)
            );


            c2.InMatch = true;
            c2.MatchRequested = false;
            c1.InMatch = true;
            c1.MatchRequested = false;

            // Now the initial statess



            // Now wait for moves from both the players
            // Modified MainGame loop

            await Task.Run(() => GamePlay(c1, c2));

        }

        private bool _isPlayer1Turn = false;
        public async Task GamePlay(Client c1, Client c2)
        {
            
                while (true)
                {
                    Console.WriteLine("Waiting for move from " + _isPlayer1Turn);
                    // Wait for ONLY the current player's move
                    if (_isPlayer1Turn)
                    {
                        await C1MoveRecived.WaitAsync();
                    }
                    else
                    {
                        await C2MoveRecived.WaitAsync();
                    }

                    // Update game state
                    UpdateObejects(_TigerPositons, _GoatPositions, avilableGoats);

                    // Broadcast to both clients
                    await Task.WhenAll(
                        c1.SendTigersInformation(temptigers),
                        c2.SendTigersInformation(temptigers),
                        c1.SendGoatsInformation(tempgoat),
                        c2.SendGoatsInformation(tempgoat),
                        c1.SendNoOfAvilableGoats(avilableGoats),
                        c2.SendNoOfAvilableGoats(avilableGoats),
                        c1.SendTurn(!_isPlayer1Turn), // Update turn state
                        c2.SendTurn(!_isPlayer1Turn)
                    );

                    // Switch turns
                    _isPlayer1Turn = !_isPlayer1Turn;
                    Console.WriteLine("Move recived and turn switched to " + _isPlayer1Turn);

                }
            
        }



        public void C1Move(int[] TigerPosServer, int[] GoatPosServer, int RemGoatsServer, bool TurnServer)
        {
            Console.WriteLine("New xxx New");
            _TigerPositons = TigerPosServer;
            _GoatPositions = GoatPosServer;
            avilableGoats = RemGoatsServer;
            Turn = TurnServer;
            C1MoveRecived.Release();
        }

        // C2 Moves first
        public void C2Move(int[] TigerPosServer, int[] GoatPosServer, int RemGoatsServer, bool TurnServer)
        {
            _TigerPositons = TigerPosServer;
            _GoatPositions = GoatPosServer;
            avilableGoats = RemGoatsServer;
            Turn = TurnServer;
            C2MoveRecived.Release();
        }

    }

}
