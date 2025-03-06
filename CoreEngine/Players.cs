
namespace Predator.CoreEngine.Players
{
    public abstract class Player
    {
        public int position;
        public string iAm;
        public Player() { }
    }

    public class Tiger : Player
    {
        public Tiger(int pos)
        {
            iAm = "T";
            position = pos;
        }
        public void TigerCore(Tiger B)
        {

        }
    }

    public class Goat : Player
    {
        public Goat(int pos)
        {
            iAm = "G";
            position = pos;
        }
    }

    public class Utils : Player
    {
        public void move(Player X, int pos)
        {
            X.position = pos;
        }
    }
}