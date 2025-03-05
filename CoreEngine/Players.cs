
namespace CoreEngine
{
    abstract public class Players
    {
        public int position;
        public string iAm;
        public Players() { }
    }

    public class Tiger : Players
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

    public class Goat : Players
    {
        public Goat(int pos)
        {
            iAm = "G";
            position = pos;
        }
    }

    public class Utils : Players
    {
        public void move(Players X, int pos)
        {
            X.position = pos;
        }
    }
}