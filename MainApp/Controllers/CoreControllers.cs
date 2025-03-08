using Predator.CoreEngine.Game;
using Predator.CoreEngine.Players;
using Predator.CoreEngine.graphedBoard;

namespace Predator.GameApp
{
    public class CoreControllers
    {
        private Game _game;


        // Provides a mechanism for cancelling async operations
        private CancellationTokenSource _cts;

        // Action is a delegate type that represents a method that takes parameters but does not return a value
        // These are bascilly the function pointers equivalent to c++


        
        // It notiesfies the UI when the state of the game is changed
        public event Action<string> GameStatusChanged;

        // It notifies the UI wen an event is logged
        public event Action<string> GameEventLogged;

        // Notifies the UI when something changes in game, like placement or something
        public event Action GameStateUpdated;

        // Event to LogMessage into UI
        public event Action<string> LogMessage;

        public CoreControllers()
        {
            // Initilizes a new game
            _game = new Game();

            // Initilizes a new cancellation token to kill the game gracefully when needed 
            _cts = new CancellationTokenSource();

            // Forward game events to UI

            // Basically when the GameStateChanged is raised it invokes the GameStateUpdated event
            // basically gamestatechaged vanne event trigger vayo vane GameStateUpdaed vanne event ma subscribe gareko sab functions run gara
            _game.GameStateChanged += () => GameStateUpdated?.Invoke();

            // LogMessage vanne event trigger vasi, LogMessage vanne event ma subscribte gareko sab functions lai call garne
            _game.LogMessage += (msg) => LogMessage?.Invoke(msg);

            /*
            VUKAMPA{
                public event Action<string> VukampaAyo;

                public void VukampaAyooNOOOOOOO()
                {
                    Console.WriteLine("Bhaga vukampa ayooo")
                    VukampaAyo?.Invoke(); // Vukampa aauda aru kai garna parne vaya gara!
                }

            } 
            {
                VUKAMPA va = new  VUKAMPA();
                
                va.VukampaAyo += () => Console.WriteLine("Gas Banda Gara");
                
                va.VukampaAyooNOOOOOOO();

            }

            In this example, when VukampaAyooNOOOOOOO() then output will be
            -> Bhaga vukampa ayo
            -> Gas Banda Gara both



             */
        }

        public async Task StartGameAsync()
        {
            LogMessage?.Invoke("Game starting...");

            // As when the game is first starting, there is no function subscribed to GameStatusChanted, this will not be printed
            GameStatusChanged?.Invoke("Game Running");

            try
            {
                // Starts the game loop in a backround thread and passes a cancellationToken to allow graceful shutdown
                // Task.Run() is a method used to execute a piece of code asynchronously on a separate thread, typically from a thread pool
                // one cts token is passed to the inGame function and another to task itslef!
                //Task gameTask = Task.Run(() => _game.inGame(_cts.Token), _cts.Token);
                await _game.inGame(_cts.Token);
                Task GameTask = _game.inGame(_cts.Token);
                await GameTask;

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
            finally
            {
                GameStatusChanged?.Invoke("Game Stopped");
            }
        }

        public void LogGameEvent(string message)
        {
            GameEventLogged?.Invoke($"[{DateTime.Now:T}] {message}");
        }

        // UI Input Forwarding
        public void PlaceGoat(int position)
        {
            LogMessage?.Invoke($"Attempting to place goat at {position}");
            _game.NotifyGoatPlacement(position);
        }
        public async void MoveTiger(int from, int to){
            _game.NotifyTigerMove(from, to);
            // It diretly goes to update the UI from here
            // The tiger has not moved yet as UpdateUI is closer from here
            // So need to wait for move to be completed!

            // Waiting for tiger to complete moving




            //LogMessage?.Invoke("Waitinggggggggggggggggggggg");
            //await _game.tigerPlacementCompletionWaiter.WaitAsync();
            //// Lol this does not work because control is passed to UI without executing this remaning blocl!
            //LogMessage?.Invoke("Done! Waitinggggggggggggggggggggg");
        }
        



        public void StopGame() => _cts.Cancel();

        //public void StartGame()
        //{
        //    _game.inGame();  
        //}

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
    }
}
