using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Predator.CoreEngine;
using Predator.CoreEngine.Game;
using Predator.CoreEngine.graphedBoard;
using Predator.CoreEngine.Players;



namespace BaghchalApp
{
    public class CoreControllers
    {
        private Board _board;
        private Game _game;

        public CoreControllers()
        {
            _board = new Board();
            _game = new Game();
        }

        // Other code...
    }
}
