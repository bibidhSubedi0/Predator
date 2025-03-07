using Predator.CoreEngine.Game;
using Predator.CoreEngine.Players;
using Predator.CoreEngine.graphedBoard;

namespace Predator.GameApp
{
    public class CoreControllers
    {
        private Game _game;
        private CancellationTokenSource _cts;


        public event Action<string> GameStatusChanged;
        public event Action<string> GameEventLogged;

        public CoreControllers()
        {
            _game = new Game();
            _cts = new CancellationTokenSource();

            // Forward game events to UI
            _game.GameStateChanged += () => GameStateUpdated?.Invoke();
            _game.LogMessage += (msg) => LogMessage?.Invoke(msg);
        }

        public async Task StartGameAsync()
        {
            LogMessage?.Invoke("Game starting...");
            GameStatusChanged?.Invoke("Game Running");

            try
            {
                await Task.Run(() => _game.inGame(_cts.Token), _cts.Token);
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
        public void MoveTiger(int from, int to) => _game.NotifyTigerMove(from, to);

        // Events
        public event Action GameStateUpdated;
        public event Action<string> LogMessage;


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
