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
        // initializes a SemaphoreSlim with an initial count of 0, meaning no threads can semaphore
        private SemaphoreSlim _goatPlacementWaiter = new SemaphoreSlim(0);
        private SemaphoreSlim _tigerMoveWaiter = new SemaphoreSlim(0);
        private int _pendingGoatPosition;
        private (int from, int to) _pendingTigerMove;

        public event Action GameStateChanged;
        public event Action<string> LogMessage;  // For debugging/logging


        // These semaphore are required for move completion to proceede to UI change
        //public SemaphoreSlim tigerPlacementCompletionWaiter = new SemaphoreSlim(0);
        // Lol does not work

        // Try using event

        
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
            LogMessage?.Invoke("Notifying Tiger move...");
            _pendingTigerMove = (from, to);
            _tigerMoveWaiter.Release();
            LogMessage?.Invoke("Tiger move notified.");
        }
        private async Task HandleGoatTurn(CancellationToken ct)
        {
            if (avilableGoats > 0)
            {
                LogMessage?.Invoke("Place a goat!");

                // Wait for UI input
                await _goatPlacementWaiter.WaitAsync(ct);

                LogMessage?.Invoke("Goat Placed!");

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
            // WaitAsync is an asynchronous method that blocks the calling thread until the semaphore’s count is greater than 0.
            // And the seamphore's count will be greater then 0 when it is rleased from some other thread somewhere
            // Which in our case is from "NotifyTigerMove"
            // So basicallt from whereever the "NotifyTigerMove" happens, only then this step will proceede
            try { 
            LogMessage?.Invoke("Waiting for Tiger move...");
            await _tigerMoveWaiter.WaitAsync(ct);
            LogMessage?.Invoke("Tiger move received.");
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
            LogMessage?.Invoke("Processing Tiger move...");
            var (from, to) = _pendingTigerMove;
            Tiger tiger = tigers.FirstOrDefault(t => t.position == from);

            if (tiger != null && board.moveValidation(tiger, to,from))
            {
                // Update tiger position
                tiger.position = to;
                board.putComponentInBoard(tiger, to);
                board.removeComponentFromBoard(from);

                LogMessage?.Invoke($"TIGER MOVED{tiger.position}");

               

                // Check for goat capture (if jumping over a node)
                if (!board.getGraph().HasEdge(from, to))
                {
                    int capturedGoatPos = (from + to) / 2;
                    board.removeComponentFromBoard(capturedGoatPos);
                }
            }

            // Now the tiger's turn is over, all the work is done
            // Now only rlease the control back to the MoveTiger() function
            //tigerPlacementCompletionWaiter.Release();

        }

        // Runs the main loop and uses the SemaphoreSlim to wait for user input
        public async Task inGame(CancellationToken cancellationToken)
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

                if (turn) // turn true = tiger's turn
                {
                    await HandleTigerTurn(cancellationToken);
                    LogMessage?.Invoke("Lord let me see sum");
                    }
                else
                {
                    await HandleGoatTurn(cancellationToken);
                }

                LogMessage?.Invoke("FUck this shit");

                turn = !turn;

                // NOW THE BOARD HAS CHANGED -> MAKE UI RERENDER THE WHOLE BOARD AGAIN
                GameStateChanged?.Invoke();

                // Add a small delay (non-blocking)
                //await Task.Delay(2000, cancellationToken);
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

            LogMessage?.Invoke("Lord let me see sum");

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