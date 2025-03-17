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


        public HandelGame()
        {  

        }

        public void MainGame(Client c1, Client c2)
        {

            Console.WriteLine("Handling the Game for " + c1.Username + " and " + c2.Username);
            // Initilize a new Game State for both of them
            
            Tiger[] temptigers = new Tiger[4];
            int[] tigerPositions = { 1, 5, 21,20 };

            for (int i = 0; i < temptigers.Length; i++)
            {
                temptigers[i] = new Tiger(tigerPositions[i]);
            }

            Goat[] tempgoat = new Goat[20];
            for (int i = 0; i < tempgoat.Length; i++)
            {
                    tempgoat[i] = null;
            }

            int RemainingGoats = 20;




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
            c1.SendTigersInformation(temptigers);
            c1.SendGoatsInformation(tempgoat);
            c1.SendNoOfAvilableGoats(RemainingGoats);
            //c1.SendTurn(Turn);


            c2.SendTigersInformation(temptigers);
            c2.SendGoatsInformation(tempgoat);
            c2.SendNoOfAvilableGoats(RemainingGoats);
            //c2.SendTurn(!Turn);


            c2.InMatch = true;
            c2.MatchRequested = false;
            c1.InMatch = true;
            c1.MatchRequested = false;

        }

    }
}
