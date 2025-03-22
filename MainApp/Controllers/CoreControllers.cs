using Predator.CoreEngine.Game;
using Predator.CoreEngine.Players;
using Predator.CoreEngine.graphedBoard;
using System.Threading;
using System.Timers;
using System.Windows.Documents;

namespace Predator.GameApp
{
    public class CoreControllers
    {

        // Ganeral Game Properties and Event Listners
        private Game _game;

        // Provides a mechanism for cancelling async operations
        private CancellationTokenSource _cts;
        public event Action<string> LogMessage;
        public event Action GameStateUpdated;
        

        
        public CoreControllers()
        {
            // Initilizes a new game 
            _game = new Game();

            // Initilizes a new cancellation token to kill the game gracefully when needed 
            _cts = new CancellationTokenSource();


            // When GameStateChanted(Game) is triggered, it, in turn, triggers all the functions assoicated to GameStateUdated(CoreController) which then finally invoked UpdateUI(Main Window)
            _game.GameStateChanged += () => GameStateUpdated?.Invoke();

            // When LogMessage is triggered, it invokes MessageHandler in Main Window
            _game.LogMessage += (msg) => LogMessage?.Invoke(msg);

        }


        // Starts the main game loop 
        public async Task StartGameAsync()
        {
            LogMessage?.Invoke("Game starting...");

            try
            {
                //Task GameTask = _game.inGame(_cts.Token);

                // Blocking call to let the game complete
                //await GameTask;
                //_cts.Cancel();
                LogMessage?.Invoke("Game loop completed successfully.");
            }
            catch (OperationCanceledException)
            {
                LogMessage?.Invoke("Game was cancelled.");
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"Error in StartGameAsync: {ex.Message}");
            }

            
        }


        // UI Input Forwarding
        // Take input from UI and pass it to the game loop
        public void PlaceGoat(int position)
        {
            _game.PutGoatInBoard(position);
        }

        public void MoveGoat(int from, int to)
        {
            _game.MoveGoat(from,to);
        }
        
        public void SetTurn(bool turn)
        {
            _game.turn = turn;
        }
        
        public void MoveTiger(int from, int to){
            _game.MoveTiger(from, to);
        }

        public void UpdateStateExplicit(int[] TigerPosServer, int[] GoatPosServer, int RemGoatsServer, bool TurnServer)
        {
            _game.turn = TurnServer;
            _game.avilableGoats = RemGoatsServer;

            //
            Tiger[] temptigers = new Tiger[4];

            for (int i = 0; i < temptigers.Length; i++)
            {
                temptigers[i] = new Tiger(TigerPosServer[i]);
            }
            _game.tigers = temptigers;

            Goat[] tempgoat = new Goat[20];
            for(int i=0;i< tempgoat.Length; i++)
            {
                if (GoatPosServer[i] != 0) { 
                tempgoat[i] = new Goat(GoatPosServer[i]);
                }
                else
                {
                    tempgoat[i] = null;
                }
            }

            _game.goats = tempgoat;
            GameStateUpdated?.Invoke();

        }


        // If controller explictly needs to kill the game, just cancel the token
        public void StopGame() => _cts.Cancel();



        // Relay's form Game to UI
        public Board GetBoardState()
        {
            return _game.GetBoard();
        }

        public Tiger[] GetTigers()
        {
            return _game.GetTigers();
        }

        public Goat[] GetGoats()
        {
            return _game.GetGoats();
        }

        public int GetAvailableGoats()
        {
            return _game.GetAvailableGoats();
        }

        public bool GetTurn()
        {
            return _game.GetTurn();
        }

        //public bool GetGameStatus()
        //{
        //    return _game.GetGameStatus();
        //}
    }
}
