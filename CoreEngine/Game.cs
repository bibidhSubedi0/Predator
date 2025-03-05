using CoreEngine;
using graphedBoard;

public class Game
{
    public void inGame()
    {
        Board board = new Board();

        // Declare and initialize the array of Tiger objects
        Tiger[] tigers = new Tiger[4];
        int[] tigerPositions = { 1, 5, 21, 25 };

        for (int i = 0; i < tigers.Length; i++)
        {
            tigers[i] = new Tiger(tigerPositions[i]);
            board.putComponentInBoard(tigers[i], tigerPositions[i]);
        }





        bool turn = true; // turn = 1 for Goat and turn = 0 for Tiger
        int avilableGoats = 20;
        Goat[] goats = new Goat[20];

        while (true)
        {
            if (turn)
            {
                if (avilableGoats != 0)
                {
                    int pos = 0;
                    // Add goat
                    do
                    {
                        Console.WriteLine("Enter postion of goat: ");
                        string input = Console.ReadLine();
                        int.TryParse(input, out pos);

                        if (board.GetComponentPlacement()[pos] != null)
                        {
                            Console.WriteLine("Invalid Placement!");
                            pos = 0;
                        }
                    } while (pos == 0);

                    Goat goat = new Goat(pos);
                    goats[20 - avilableGoats] = goat;
                    board.putComponentInBoard(goat, pos);
                    avilableGoats -= 1;
                }
                else
                {
                    // Move goat

                }
            }
            else
            {
                bool flag = true;
                int currentPos = 0;
                int newPos = 0;

                do
                {
                    Console.WriteLine("Enter the current position of the tiger: ");
                    string currentPosInput = Console.ReadLine();
                    Console.WriteLine("Enter the new position of the tiger: ");
                    string newPosInput = Console.ReadLine();
                    if (int.TryParse(currentPosInput, out currentPos) && int.TryParse(newPosInput, out newPos))
                    {
                        var tigerMov = (currentPos, newPos);
                        // Deconstruct the tuple
                        (int from, int to) = tigerMov;

                        foreach (Tiger t in tigers)
                        {
                            if (t.position == currentPos)
                            {
                                if (!board.moveValidation(tigers[0], to, from))
                                {
                                    Console.WriteLine("Invalid move for tiger! ");
                                }
                                else
                                {
                                    flag = !flag;
                                }

                            }
                        }

                    }
                } while (flag);

                foreach (Tiger t in tigers)
                {
                    if (t.position == currentPos)
                    {
                        t.position = newPos;
                        // If there was a goat in between, remove it! 
                        //if((currentPos+newPos)/2)
                        if (!board.getGraph().HasEdge(currentPos, newPos))
                        {
                            goats[(currentPos + newPos) / 2] = null;
                            board.removeComponentFromBoard((currentPos + newPos) / 2);
                        }

                        board.putComponentInBoard(t, newPos);
                        board.removeComponentFromBoard(currentPos);
                        break;
                    }
                }
            }

            // Print board
            turn = !turn;
            board.PrintBoard();
            //board.getGraph().traverse(1,4);
        }
    }

}
