using Predator.CoreEngine.Game;
using Predator.CoreEngine.Players;
using Predator.CoreEngine.graphedBoard;

namespace Predator.GameApp
{
    public class CoreControllers
    {
        private Game _game;

        public CoreControllers()
        {
            _game = new Game();
        }

        public void StartGame()
        {
            _game.inGame();  
        }

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
