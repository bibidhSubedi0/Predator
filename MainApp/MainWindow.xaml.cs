﻿using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

using Predator.GameApp;
using Predator.CoreEngine.Players;
using System.ComponentModel;
using Predator.CoreEngine.graphedBoard;

namespace UIPredator
{
    public partial class MainWindow : Window
    {
        private const int GridSize = 5;
        private const double CellSize = 100;
        private CoreControllers _core;

        private Dictionary<int, (double X, double Y)> NodePositions = new Dictionary<int, (double, double)>();

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
            var button = (Button)sender;
            try
            {
                button.IsEnabled = false;

                // This starts the game loop and control is tranferd back here when the game ends completely!
                await _core.StartGameAsync();
            }
            finally
            {
                button.IsEnabled = true;
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

        // This fucntion is taking the user input from UI -> Just a button for now -> and moving the tiger/goat according to the input
        private void BoardPosition_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int position = 12;

            // This is working perfectly fine!
            if (!_core.GetTurn()) // Goat's turn
            {

                _core.PlaceGoat(position);
            }
            else // Tiger's turn
            {
                _core.MoveTiger(1, 6);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _core.StopGame();
        }

        private void DrawBoard()
        {
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
            //BoardCanvas.Children.Clear();
            Board NewBoardInfo = _core.GetBoardState();
            Tiger[] NewTigersInfo = _core.GetTigers();
            Goat[] NewGoatInfo = _core.GetGoats();
            int NewAvilableGoats = _core.GetAvailableGoats();

            foreach(var t in NewTigersInfo)
            {
                LogMessageHandler(t.position.ToString());
                (double X,double Y) pos = NodePositions[t.position];
                DrawCircle(pos.X, pos.Y, Brushes.Red, 10);
            }

            //foreach (var g in NewGoatInfo)
            //{
            //    if(g.position !=0)
            //    {
            //        (double X, double Y) pos = NodePositions[g.position];
            //        DrawCircle(pos.X, pos.Y, Brushes.Red, 10);
            //    }
            //}
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