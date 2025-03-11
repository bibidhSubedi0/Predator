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
        public bool turn = false;
        public int avilableGoats = 20;
        public Goat[] goats = new Goat[20];
        public bool GameOn = false;

        // Async Input Handling
        // initializes a SemaphoreSlim with an initial count of 0, meaning no threads can semaphore
        private SemaphoreSlim _goatPlacementWaiter = new SemaphoreSlim(0);
        private SemaphoreSlim _goatMovementWaiter = new SemaphoreSlim(0);
        private SemaphoreSlim _tigerMoveWaiter = new SemaphoreSlim(0);
        private int _pendingGoatPosition;
        private (int from, int to) _pendingTigerMove;
        private (int from, int to) _pendingGoatMove;

        public event Action GameStateChanged;
        public event Action<string> LogMessage;  // For debugging/logging

        public Game()
        {
            for (int i = 0; i < tigers.Length; i++)
            {
                tigers[i] = new Tiger(tigerPositions[i]);
                board.putComponentInBoard(tigers[i], tigerPositions[i]);
            }
            for (int i = 0; i < 20; i++)
            {
                goats[i] = null;
            }
        }

        public bool checkGameOver()
        {
            // Check if Tigers have won
            if (avilableGoats == 0)
            {
                bool allGoatsEaten = true;
                foreach (var goat in goats)
                {
                    if (goat != null) // If any goat is still on the board
                    {
                        allGoatsEaten = false;
                        break;
                    }
                }

                if (allGoatsEaten)
                {
                    // Tigers have won because no goats are left to place or on the board
                    return true;
                }
            }

            // Check if Goats have won (no valid moves for any tiger)
            bool canAnyTigerMove = false;
            foreach (var tiger in tigers)
            {
                for (int i = 1; i <= 25; i++)
                {
                    if (board.moveValidation(tiger, i, tiger.position))
                    {
                        canAnyTigerMove = true; // At least one tiger can move
                        break;
                    }
                }

                if (canAnyTigerMove)
                {
                    break; // No need to check further if a tiger can move
                }
            }

            if (!canAnyTigerMove)
            {
                // Goats have won because no tiger can move
                return true;
            }

            // If neither condition is met, the game is not over
            return false;
        }

        public void NotifyGoatPlacement(int position)
        {
            _pendingGoatPosition = position;
            _goatPlacementWaiter.Release();
        }

        public void NotifyGoatMove(int from, int to)
        {
            _pendingGoatMove = (from, to);
            _goatMovementWaiter.Release();
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
                LogMessage?.Invoke("Move a goat!");

                // Wait for UI input
                await _goatMovementWaiter.WaitAsync(ct);

                var (from, to) = _pendingGoatMove;
                Goat goat = goats.FirstOrDefault(g => g.position == from);

                if (goat != null && board.moveValidation(goat, to, from))
                {
                    // Update tiger position
                    goat.position = to;
                    board.putComponentInBoard(goat, to);
                    board.removeComponentFromBoard(from);
                }

            }
        }

        private async Task HandleTigerTurn(CancellationToken ct)
        {
            LogMessage?.Invoke("Move a tiger!");

            // Wait for UI input
            // WaitAsync is an asynchronous method that blocks the calling thread until the semaphore’s count is greater than 0.
            // And the seamphore's count will be greater then 0 when it is rleased from some other thread somewhere
            // Which in our case is from "NotifyTigerMove"
            // So basicallt from whereever the "NotifyTigerMove" happens, only then this step will proceede
            try { 
            await _tigerMoveWaiter.WaitAsync(ct);
            }
            catch (OperationCanceledException)
            {
                LogMessage?.Invoke("Tiger turn was cancelled.");
                throw; // Re-throw to exit the game loop
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"Error in HandleTigerTurn: {ex.Message}");
                LogMessage?.Invoke($"Stack Trace: {ex.StackTrace}");
                throw; // Re-throw to exit the game loop
            }

            // Process Tiger's move
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
                    // Check for this goat and remove it from array as well!
                    for(int i = 0; i < goats.Length; i++)
                    {
                        if (goats[i]!=null)
                        {
                            if (goats[i].position == capturedGoatPos)
                            {

                                goats[i] = null;
                                break;
                            }
                        }
                    }
                    board.removeComponentFromBoard(capturedGoatPos);

                }
            }

        }

        // Runs the main loop and uses the SemaphoreSlim to wait for user input
        public async Task inGame(CancellationToken cancellationToken)
        {
            LogMessage?.Invoke("Stared the game loop!");
            GameOn = true;
            
            try { 
            // Main Game Loop
                while (!cancellationToken.IsCancellationRequested)
                {
                    LogMessage?.Invoke($"Current turn: {(turn ? "Tiger" : "Goat")}");

                    if (turn) // turn true = tiger's turn
                    {
                        await HandleTigerTurn(cancellationToken);
                        }
                    else
                    {
                        await HandleGoatTurn(cancellationToken);
                    }

                    turn = !turn;

                    // NOW THE BOARD HAS CHANGED -> MAKE UI RERENDER THE WHOLE BOARD AGAIN
                    GameStateChanged?.Invoke();

                    if (checkGameOver())
                    {
                        GameOn = false;
                        _goatMovementWaiter.Release();
                        _goatPlacementWaiter.Release();
                        _tigerMoveWaiter.Release();

                        break;
                    }

                    

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

        public bool GetGameStatus()
        {
            return GameOn;
        }
        

    }

}