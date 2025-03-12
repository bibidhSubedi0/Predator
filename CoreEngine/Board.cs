
using Predator.CoreEngine.Players;

    namespace Predator.CoreEngine.graphedBoard
    {

        public class Graph
        {
            private Dictionary<int, List<int>> adjList;

            public Graph()
            {
                adjList = new Dictionary<int, List<int>>();
            }

            public void addEdge(int u, int v)
            {
                if (!adjList.ContainsKey(u))
                {
                    adjList[u] = new List<int> { v };
                }

                if (!adjList.ContainsKey(v))
                {
                    adjList[v] = new List<int> { u };
                }

                if (!adjList[u].Contains(v))
                    adjList[u].Add(v);
                if (!adjList[v].Contains(u))
                    adjList[v].Add(u);
            }

            public void PrintGraph()
            {
                foreach (var kvp in adjList)
                {
                    Console.Write(kvp.Key + " -> ");
                    foreach (var neighbor in kvp.Value)
                        Console.Write(neighbor + " ");
                    Console.WriteLine();
                }
            }

            // Depth required to totally traverse the graph is 4!
            public void traverse(int initial, int max_depth = 0)
            {
                List<bool> visited = new List<bool>(new bool[26]);

                Queue<(int node, int depth)> check = new Queue<(int, int)>();

                check.Enqueue((initial, 0));
                visited[initial] = true;

                while (check.Count() > 0)
                {
                    var (pos, depth) = check.Dequeue(); // Say 1 ayo then 1->2,6,7
                    if (depth >= max_depth) continue;


                    List<int> adjcent_nodes = adjList[pos]; // adject_nodes now have 2,6,7
                    foreach (int p in adjcent_nodes)
                    {
                        if (visited[p] == false)
                        {
                            check.Enqueue((p, depth + 1)); // 2,6,7 are now in queue 7,6,2
                            visited[p] = true; // now 2,6,7 are marked visited
                        }
                    }
                }
            }

            public bool HasEdge(int x, int y)
            {
                return adjList[x].Contains(y);
            }

            public Dictionary<int, List<int>> getAdjList()
            {
                return adjList;
            }

        }

        public class Board
        {
            Graph boardGraph = new Graph();
            Dictionary<int, Player> ComponentPlacement = new Dictionary<int, Player>(26);
            public Board()
            {
                // Initilizing the edges of the graph
                {
                    // Horizontal edges
                    for (int i = 1; i <= 25; i += 5)
                    {
                        for (int j = i; j < i + 4; j++)
                        {

                            boardGraph.addEdge(j, j + 1);
                        }
                    }
                    // Vertical edges
                    for (int i = 1; i <= 5; i++)
                    {
                        for (int j = i; j <= 20; j += 5)
                        {

                            boardGraph.addEdge(j, j + 5);

                        }
                    }
                    // Diagonal edges
                    boardGraph.addEdge(1, 7); boardGraph.addEdge(7, 13); boardGraph.addEdge(13, 19); boardGraph.addEdge(19, 25);  // Left-to-right diagonal
                    boardGraph.addEdge(5, 9); boardGraph.addEdge(9, 13); boardGraph.addEdge(13, 17); boardGraph.addEdge(17, 21);  // Right-to-left diagonal

                    // Diamond edges (3, 7,11,17,23,19, 15, 9)
                    boardGraph.addEdge(3, 7); boardGraph.addEdge(7, 11); boardGraph.addEdge(11, 17); boardGraph.addEdge(17, 23); boardGraph.addEdge(23, 19); boardGraph.addEdge(19, 15); boardGraph.addEdge(15, 9); boardGraph.addEdge(3, 9);
                    //boardGraph.PrintGraph();
                }

                //// Putting the tigers in their respective position
                for (int i = 0; i <= 25; i++)
                {
                    ComponentPlacement[i] = null;

                }

            }
            public void boardMain()
            {
                // Handel all the board related stuff
            }

            public Graph getGraph()
            {
                return boardGraph;
            }
            public void PrintBoard()
            {
                Console.Clear();

                for (int row = 1; row <= 5; row++)
                {
                    // Print nodes and horizontal edges
                    for (int col = 1; col <= 5; col++)
                    {
                        int index = (row - 1) * 5 + col;

                        // Node (player's symbol or empty node '+')
                        if (ComponentPlacement[index] != null)
                            Console.Write(ComponentPlacement[index].iAm);
                        else
                            Console.Write("+");

                        // Horizontal edges
                        if (col < 5)
                            Console.Write("---");
                    }
                    Console.WriteLine();

                    // Print vertical edges (between rows)
                    if (row < 5)
                    {
                        for (int col = 1; col <= 5; col++)
                        {
                            int index = (row - 1) * 5 + col;
                            int belowIndex = index + 5;

                            // Vertical edge if both nodes are occupied
                            if (ComponentPlacement.ContainsKey(index) && ComponentPlacement.ContainsKey(belowIndex))
                                Console.Write("|   ");
                            else
                                Console.Write("    ");
                        }
                        Console.WriteLine();
                    }
                }
            }

            public void putComponentInBoard(Player comp, int pos)
            {
                ComponentPlacement[pos] = comp;

            }
            public void removeComponentFromBoard(int pos)
            {
                ComponentPlacement[pos] = null;
            }

            public Dictionary<int, Player> GetComponentPlacement()
            {
                return ComponentPlacement;
            }


            public List<int> searchFor(int initial, int max_depth, Player p)
            {
                List<int> locs = new List<int>();


                List<bool> visited = new List<bool>(new bool[26]);
                Queue<(int node, int depth)> check = new Queue<(int, int)>();
                check.Enqueue((initial, 0));
                visited[initial] = true;

                while (check.Count() > 0)
                {
                    var (pos, depth) = check.Dequeue(); // Say 1 ayo then 1->2,6,7
                    if (depth >= max_depth) continue;


                    List<int> adjcent_nodes = boardGraph.getAdjList()[pos]; // adject_nodes now have 2,6,7
                    foreach (int px in adjcent_nodes)
                    {
                        if (visited[px] == false)
                        {
                            check.Enqueue((px, depth + 1)); // 2,6,7 are now in queue 7,6,2
                            visited[px] = true; // now 2,6,7 are marked visited
                        }

                        if (ComponentPlacement[px].iAm == p.iAm)
                        {
                            locs.Append(px);
                        }
                    }

                }
                return locs;
            }


            public bool moveValidation(Player p, int to, int from)
            {
                if (ComponentPlacement[to] != null)
                {
                    return false;
                }

                if (boardGraph.HasEdge(to, from) && ComponentPlacement[to] == null)
                {
                    return true;
                }

                if (p.iAm == "T")
                {
                    // Determine mid point
                    int midPoint = (to + from) / 2;

                    if (boardGraph.HasEdge(from, midPoint) && boardGraph.HasEdge(midPoint, to) && (ComponentPlacement[midPoint].iAm == "G"))
                    {
                        return true;
                    }
                }

                return false;
            }
        }


    }
