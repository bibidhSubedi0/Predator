using Predator.CoreEngine.Players;
using Predator.CoreEngine.graphedBoard;

namespace Predator.CoreEngine.Game
{
    public class Game
    {
        // Whenever a new "Game" is instantiated, these things need to happen
        public Board board = new Board();
        public Tiger[] tigers = new Tiger[4];
        public int[] tigerPositions = { 1, 5, 21, 25 };
        public bool turn = true;
        public int avilableGoats = 20;
        public Goat[] goats = new Goat[20];
        

        // Async Input Handling
        private SemaphoreSlim _goatPlacementWaiter = new SemaphoreSlim(0);
        private SemaphoreSlim _tigerMoveWaiter = new SemaphoreSlim(0);
        private int _pendingGoatPosition;
        private (int from, int to) _pendingTigerMove;

        public event Action GameStateChanged;
        public event Action<string> LogMessage;  // For debugging/logging


        public Game()
        {
            for (int i = 0; i < tigers.Length; i++)
            {
                tigers[i] = new Tiger(tigerPositions[i]);
                board.putComponentInBoard(tigers[i], tigerPositions[i]);
            }   
        }

        public void NotifyGoatPlacement(int position)
        {
            _pendingGoatPosition = position;
            _goatPlacementWaiter.Release();
        }

        public void NotifyTigerMove(int from, int to)
        {
            _pendingTigerMove = (from, to);
            _tigerMoveWaiter.Release();
        }
        private async Task HandleGoatTurn(CancellationToken ct)
        {
            if (avilableGoats > 0)
            {
                LogMessage?.Invoke("Place a goat!");

                // Wait for UI input
                await _goatPlacementWaiter.WaitAsync(ct);

                // Validate position
                if (board.GetComponentPlacement()[_pendingGoatPosition] == null)
                {
                    Goat goat = new Goat(_pendingGoatPosition);
                    goats[20 - avilableGoats] = goat;
                    board.putComponentInBoard(goat, _pendingGoatPosition);
                    avilableGoats--;
                }
            }
            else
            {
                // TODO: Implement goat movement logic
            }
        }

        private async Task HandleTigerTurn(CancellationToken ct)
        {
            LogMessage?.Invoke("Tiger's turn!");

            // Wait for UI input
            await _tigerMoveWaiter.WaitAsync(ct);

            var (from, to) = _pendingTigerMove;
            Tiger tiger = tigers.FirstOrDefault(t => t.position == from);

            if (tiger != null && board.moveValidation(tiger, to,from))
            {
                // Update tiger position
                tiger.position = to;
                board.putComponentInBoard(tiger, to);
                board.removeComponentFromBoard(from);

                // Check for goat capture (if jumping over a node)
                if (!board.getGraph().HasEdge(from, to))
                {
                    int capturedGoatPos = (from + to) / 2;
                    board.removeComponentFromBoard(capturedGoatPos);
                }
            }
        }

        public async void inGame(CancellationToken cancellationToken)
        {
            LogMessage?.Invoke("Stared the game loop!");
            int loopCount = 0;

            try { 
            // Main Game Loop
            while (!cancellationToken.IsCancellationRequested)
            {
                loopCount++;
                LogMessage?.Invoke($"Game loop iteration#{loopCount}");
                LogMessage?.Invoke($"Current turn: {(turn ? "Tiger" : "Goat")}");

                if (turn)
                {
                    await HandleTigerTurn(cancellationToken);
                }
                else
                {
                    await HandleGoatTurn(cancellationToken);
                }

                turn = !turn;
                GameStateChanged?.Invoke();

                // Add a small delay (non-blocking)
                await Task.Delay(500, cancellationToken);
            }
            }
            catch (OperationCanceledException)
            {
                LogMessage?.Invoke("Game loop was cancelled.");
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"Error in game loop: {ex.Message}");
            }
            finally
            {
                LogMessage?.Invoke("Game loop exited.");
            }

        }

        public bool GetTurn()
        {
            return turn;
        }


        public Board GetBoard()
        {
            return board;
        }

        public Tiger[] GetTigers()
        {
            return tigers;
        }

        public Goat[] GetGoats()
        {
            return goats;
        }

        public int GetAvailableGoats()
        {
            return avilableGoats;
        }


    }

}