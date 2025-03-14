using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

using Predator.GameApp;
using Predator.CoreEngine.Players;
using System.ComponentModel;
using Predator.CoreEngine.graphedBoard;
using static System.Net.Mime.MediaTypeNames;
using PredatorApp.Net;

namespace UIPredator
{
    public partial class MainWindow : Window
    {
        // General Gird Properties
        private const int GridSize = 5;
        private const double CellSize = 100;
        private CoreControllers _core;

        // Basically holds the Actual coordinates of the node position for each node poition [1,25]
        private Dictionary<int, (double X, double Y)> NodePositions = new Dictionary<int, (double, double)>();

        // For handling turns of tiger and goats
        int _selectedTigerPosition = -1;
        int _selectedGoatPosition = -1;

        // For Network handling, idk if it works, but a cline having one server handling mechansim sounds intuative
        ServerHandeling networkManager;

        // Player's Network informations
        String _Username;


        public MainWindow()
        {
            // Initilize the game through a sepeate component so that game can be easily restarted
            InitGame();

            // Initilize the network component 
             networkManager = new ServerHandeling();
        }


        // Iitilizize the core controller to the a new game!
        private void InitGame()
        {
            InitializeComponent();
            _core = new CoreControllers();
            
            // _core ma vako, GameStateUpdate Invoke huda Bittikai Update the UI
            // Now when game status is updated, Also send the information to the server
            _core.GameStateUpdated += UpdateUI;
            _core.GameStateUpdated += SendPacketsToServer;


            // LogMessage Invoke huda bittikai call LogMessageHandler
            _core.LogMessage += LogMessageHandler;

        }

        // Accept Username
        private void SubmitUsernameButtonClick(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();


            if (string.IsNullOrEmpty(username))
            {
                // Show an error message if the username is empty
                UsernameStatusText.Text = "Status: Username cannot be empty!";
            }
            else
            {
                // Process the username (e.g., send it to the server or store it locally)
                UsernameStatusText.Text = $"Status: Username '{username}' submitted!";
                _Username = username;

                // Optionally, you can disable the username input after submission
                UsernameTextBox.IsEnabled = false;
                SubmitUsernameButton.IsEnabled = false;
            }
        }


        // Connect To Server
        private void ConnectToNetworkButtonClick(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(_Username))
            {
                NetworkStatusText.Text = "Enter a Valid Username!";
                return;
            }

            // Update UI to show connection attempt
            NetworkStatusText.Text = "Status: Connecting to Server...";

            // Run the connection logic asynchronously to avoid blocking the UI
            Task.Run(() =>
            {
                networkManager.ConnectToServer(_Username);
                bool isConnected = networkManager._isConnected;

                // Update UI based on the connection result
                Dispatcher.Invoke(() =>
                {
                    if (isConnected)
                    {
                        NetworkStatusText.Text = "Status: Connected to Server";
                    }
                    else
                    {
                        NetworkStatusText.Text = "Status: Connection Failed";
                    }
                });

            });
        }


        // Now everytime the game is updated send all the relevent information to the server
        private void SendPacketsToServer()
        {
            Tiger[] NewTigersInfo = _core.GetTigers();
            Goat[] NewGoatInfo = _core.GetGoats();
            int NewAvilableGoats = _core.GetAvailableGoats();
            bool turn = _core.GetTurn();

            try { 
            if (networkManager._isConnected)
                {
                    // Send all the relevent infromations
                    /*
                     *  1 -> Turn
                     *  2 -> No. of avilable goats
                     *  3 -> all the aviable goats Goat[] 
                     *  4 -> Tiger position Tiger[]
                     */

                    networkManager.SendGoatsInformation(NewGoatInfo);
                    networkManager.SendTurn(turn);
                    networkManager.SendTigersInformation(NewTigersInfo);
                    networkManager.SendNoOfAvilableGoats(NewAvilableGoats);

                }
            }
            finally
            {

            }
        }


        // On strat button click -> This function starts asynchronouly
        private async void StartGameButtonClick(object sender, RoutedEventArgs e)
        {
            // Initilize a new _core that creates a completely new game
            InitGame();

            // Redraw the UI based on the new game
            UpdateUI();

            try
            {

                // General Stuff
                GameOverOverlay.Visibility = Visibility.Collapsed;
                StartButton.IsEnabled = false;
                GameStatusText.Background = (Brush)new BrushConverter().ConvertFrom("#4C566A");
                GameStatusText.Text = "Status: In Progress";
                GameStatusText.Foreground = (Brush)new BrushConverter().ConvertFrom("#A3BE8C");


                // Strart the actual game loop
                await _core.StartGameAsync();
            }

            // After/Regradless of completion of game Loop
            finally
            {

                // More general stuff
                StartButton.IsEnabled = true;
                GameStatusText.Background = (Brush)new BrushConverter().ConvertFrom("#3B4252");
                GameStatusText.Text = "Status: Not Started";
                GameStatusText.Foreground = (Brush)new BrushConverter().ConvertFrom("#88C0D0");
                GameOverText.Text = "Game Over!";
                GameOverOverlay.Visibility = Visibility.Visible; // Show the overlay

            }
        }

        // Prints the Loged Message to the UI
        private void LogMessageHandler(string message)
        {
            Dispatcher.Invoke(() =>
            {
                LogTextBox.AppendText($"{DateTime.Now:HH:mm:ss} - {message}\n");
                LogTextBox.ScrollToEnd();
            });
        }


        // Well, as the name suggests, Updates the UI, mostly based on the gamestatuschanged event
        private void UpdateUI()
        {
            Dispatcher.Invoke(() =>
            {
                BoardCanvas.Children.Clear();
                DrawBoard();
                DrawComponents();

            });
        }


        // Helper to UpdateUI -> There is a bug with placements but not harmful enough for me to fix it
        private void BoardCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(!_core.GetGameStatus())
            {
                return;
            }

            // Get the mouse click position relative to the board canvas
            Point clickPosition = e.GetPosition(BoardCanvas);

            
            // Map the coordinates to your game's grid position
            int gridX = (int)(clickPosition.X / CellSize);
            int gridY = (int)(clickPosition.Y / CellSize);
            int boardPosition = ConvertGridToBoardPosition(gridX, gridY);


            // Call game logic based on the current turn
            if (!_core.GetTurn()) // Goat's turn
            {
                if(_core.GetAvailableGoats() >0)
                {
                    _core.PlaceGoat(boardPosition);
                    
                }
                else if(_selectedGoatPosition ==-1)
                {
                    _selectedGoatPosition = boardPosition;
                }
                else
                {
                    _core.MoveGoat(_selectedGoatPosition, boardPosition);
                    _selectedGoatPosition = -1;
                }
            }
            else // Tiger's turn
            {
                if (_selectedTigerPosition == -1)
                {
                    _selectedTigerPosition = boardPosition; // First click: select tiger
                }
                else
                {
                    _core.MoveTiger(_selectedTigerPosition, boardPosition); // Second click: move
                    _selectedTigerPosition = -1; // Reset selection
                }
            }
        }


        // Helper to UpdateUI -> Draws the board's graph
        private void DrawBoard()
        {
            var background = new Rectangle
            {
                Width = BoardCanvas.ActualWidth,
                Height = BoardCanvas.ActualHeight,
                Fill = new SolidColorBrush(Color.FromRgb(59, 66, 82))
            };
            BoardCanvas.Children.Add(background);

            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    double x = j * CellSize + CellSize / 2;
                    double y = i * CellSize + CellSize / 2;

                    int nodeNumber = i * GridSize + j + 1;
                    NodePositions[nodeNumber] = (x, y);

                    // Draw horizontal and vertical lines
                    if (i < GridSize - 1)
                        DrawLine(x, y, x, y + CellSize);

                    if (j < GridSize - 1)
                        DrawLine(x, y, x + CellSize, y);

                    // Removed the diagonal lines in each cell
                }
            }


            // L-R main diagonal (1-7-13-19-25)
            DrawEdge(0, 0, 1, 1);
            DrawEdge(1, 1, 2, 2);
            DrawEdge(2, 2, 3, 3);
            DrawEdge(3, 3, 4, 4);

            // R-L main diagonal (5-9-13-17-21)
            DrawEdge(0, 4, 1, 3);
            DrawEdge(1, 3, 2, 2);
            DrawEdge(2, 2, 3, 1);
            DrawEdge(3, 1, 4, 0);

            // diamond pattern edges
            DrawEdge(0, 2, 1, 1);  // 3-7
            DrawEdge(1, 1, 2, 0);  // 7-11
            DrawEdge(2, 0, 3, 1);  // 11-17
            DrawEdge(3, 1, 4, 2);  // 17-23
            DrawEdge(4, 2, 3, 3);  // 23-19
            DrawEdge(3, 3, 2, 4);  // 19-15
            DrawEdge(2, 4, 1, 3);  // 15-9
            DrawEdge(1, 3, 0, 2);  // 9-3
            DrawEdge(0, 2, 1, 3);  // 3-9
        }

        
        // Helper to DrawBoard -> To draw the edges of the grpah
        private void DrawEdge(int startRow, int startCol, int endRow, int endCol)
        {
            double x1 = startCol * CellSize + CellSize / 2;
            double y1 = startRow * CellSize + CellSize / 2;
            double x2 = endCol * CellSize + CellSize / 2;
            double y2 = endRow * CellSize + CellSize / 2;
            DrawLine(x1, y1, x2, y2);
        }


        // Helper to DdawEdge -> To draw the lines
        private void DrawLine(double x1, double y1, double x2, double y2)
        {
            Line line = new Line()
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            BoardCanvas.Children.Add(line);
        }


        // Helper to UpdateUI -> To draw all the components of the game
        private void DrawComponents()
        {
            Board NewBoardInfo = _core.GetBoardState();
            Tiger[] NewTigersInfo = _core.GetTigers();
            Goat[] NewGoatInfo = _core.GetGoats();
            int NewAvilableGoats = _core.GetAvailableGoats();

            foreach (var component in NewTigersInfo)
            {
                (double X, double Y) pos = NodePositions[component.position];
                DrawCircle(pos.X, pos.Y, Brushes.Red, 10);
            }


            foreach (var g in NewGoatInfo)
            {
                if (g != null)
                {
                    (double X, double Y) pos = NodePositions[g.position];
                    DrawCircle(pos.X, pos.Y, Brushes.Blue, 8);
                }
            }
        }


        // Helper to DrawCompoent -> To draw the circular figures
        private void DrawCircle(double x, double y, Brush color, int radius)
        {
            Ellipse ellipse = new Ellipse()
            {
                //Width = 2 * radius,
                Width = 2 * radius,
                Height = 2 * radius,
                Fill = color,
                Stroke = Brushes.Black,
                StrokeThickness = 1


            };

            Canvas.SetLeft(ellipse, x - radius);
            Canvas.SetTop(ellipse, y - radius);
            BoardCanvas.Children.Add(ellipse);
        }



        // Convert grid (x,y) to board's position index [1,25] bit buggy, but mostly works
        private int ConvertGridToBoardPosition(int x, int y)
        {
            return (y * GridSize + x) +1;
        }

        
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _core.StopGame();
        }


    }
}