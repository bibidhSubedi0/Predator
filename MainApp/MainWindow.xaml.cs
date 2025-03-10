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

namespace UIPredator
{
    public partial class MainWindow : Window
    {
        private const int GridSize = 5;
        private const double CellSize = 100;
        private CoreControllers _core;

        private Dictionary<int, (double X, double Y)> NodePositions = new Dictionary<int, (double, double)>();
        int _selectedTigerPosition = -1;
        int _selectedGoatPosition = -1;

        public MainWindow()
        {
            InitializeComponent();
            _core = new CoreControllers();

            // Subscribe to the UupdateUI when instanciating the window
            // Subscribing = adding the UpdateUI methid to the list of methods that will be called when the event is raised
            // Note that this is not invoking the UpdateUI. can be unsubscibed using -=

            // GameStateUpdate Invoke huda Bittikai call UpdateUI!
            _core.GameStateUpdated += UpdateUI;

            // LogMessage Invoke huda bittikai call LogMessageHandler
            _core.LogMessage += LogMessageHandler;

            // ------------------------------------------------
            _core.GameStatusChanged += UpdateGameStatus;

            
            UpdateUI();
        }



        // On strat button click -> This function starts asynchronouly
        private async void StartGameButtonClick(object sender, RoutedEventArgs e)
        {
            UpdateUI();
            try
            {
                GameOverOverlay.Visibility = Visibility.Collapsed;
                StartButton.IsEnabled = false;

                StatusBorder.Background = (Brush)new BrushConverter().ConvertFrom("#4C566A"); 
                StatusText.Text = "Status: In Progress";
                StatusText.Foreground = (Brush)new BrushConverter().ConvertFrom("#A3BE8C");

                await _core.StartGameAsync();
            }
            finally // Game sakiyo
            {
                StartButton.IsEnabled = true;
                StatusBorder.Background = (Brush)new BrushConverter().ConvertFrom("#3B4252");
                StatusText.Text = "Status: Not Started";
                StatusText.Foreground = (Brush)new BrushConverter().ConvertFrom("#88C0D0");

            
                
                GameOverText.Text = "Game Over!";
                GameOverOverlay.Visibility = Visibility.Visible; // Show the overlay

                // Clean stuff up
                _core.StartProcedure();
                }

            
        }

        private void LogMessageHandler(string message)
        {
            Dispatcher.Invoke(() =>
            {
                LogTextBox.AppendText($"{DateTime.Now:HH:mm:ss} - {message}\n");
                LogTextBox.ScrollToEnd();
            });
        }

        private void UpdateGameStatus(string status)
        {
            Dispatcher.Invoke(() =>
            {

            });
        }

        private void UpdateUI()
        {
            Dispatcher.Invoke(() =>
            {
                BoardCanvas.Children.Clear();
                DrawBoard();
                DrawComponents();

            });
        }

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
                    LogMessageHandler("Selected goat positon "+_selectedGoatPosition.ToString());
                }
                else
                {
                    _core.MoveGoat(_selectedGoatPosition, boardPosition);
                    LogMessageHandler("Selected goat move position " + boardPosition.ToString());
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

        // Convert grid (x,y) to board's position index (e.g., 0-24 for 5x5)
        private int ConvertGridToBoardPosition(int x, int y)
        {
            return (y * GridSize + x) +1;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _core.StopGame();
        }

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

        private void DrawEdge(int startRow, int startCol, int endRow, int endCol)
        {
            double x1 = startCol * CellSize + CellSize / 2;
            double y1 = startRow * CellSize + CellSize / 2;
            double x2 = endCol * CellSize + CellSize / 2;
            double y2 = endRow * CellSize + CellSize / 2;
            DrawLine(x1, y1, x2, y2);
        }

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

        private void DrawComponents()
        {
            Board NewBoardInfo = _core.GetBoardState();
            Tiger[] NewTigersInfo = _core.GetTigers();
            Goat[] NewGoatInfo = _core.GetGoats();
            int NewAvilableGoats = _core.GetAvailableGoats();

            foreach(var component in NewTigersInfo)
            {
                (double X, double Y) pos = NodePositions[component.position];
                DrawCircle(pos.X, pos.Y, Brushes.Red, 10);
            }


            foreach (var g in NewGoatInfo)
            {
                if (g!=null)
                {
                    (double X, double Y) pos = NodePositions[g.position];
                    DrawCircle(pos.X, pos.Y, Brushes.Blue, 8);
                }
            }
        }

        private void DrawCircle( double x, double y, Brush color, int radius)
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



    }
}