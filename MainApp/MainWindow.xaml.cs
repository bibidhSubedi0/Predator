using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

using Predator.GameApp;
using Predator.CoreEngine.Players;
using System.ComponentModel;

namespace UIPredator
{
    public partial class MainWindow : Window
    {
        private const int GridSize = 5;
        private const double CellSize = 100;
        private CoreControllers _core;

        public MainWindow()
        {
            InitializeComponent();
            _core = new CoreControllers();
            _core.GameStateUpdated += UpdateUI;

            // Use only the method handler
            _core.LogMessage += LogMessageHandler;

            _core.GameStatusChanged += UpdateGameStatus;
            DrawBoard();
        }
        private async void StartGameButtonClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            try
            {
                button.IsEnabled = false;
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
            //Dispatcher.Invoke(() =>
            //{
            //    // Update board visualization
            //    BoardPanel.Children.Clear();
            //    foreach (var component in _core.GetBoardState().GetComponents())
            //    {
            //        DrawComponent(component); // Your custom rendering logic
            //    }

            //    // Update turn indicator
            //    TurnText.Text = _core.GetTurn() ? "Tiger's Turn" : "Goat's Turn";
            //});
        }

        private void BoardPosition_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int position = (int)button.Tag;

            if (!_core.GetTurn()) // Goat's turn
            {
                _core.PlaceGoat(position);
            }
            else // Tiger's turn
            {
                // For tiger move: first click selects tiger, second selects destination
                // (Implement your own logic for two-step selection)
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

                    // Draw horizontal and vertical lines
                    if (i < GridSize - 1)
                        DrawLine(x, y, x, y + CellSize);

                    if (j < GridSize - 1)
                        DrawLine(x, y, x + CellSize, y);

                    // Diagonal lines for corners
                    if (i < GridSize - 1 && j < GridSize - 1)
                    {
                        DrawLine(x, y, x + CellSize, y + CellSize);
                        DrawLine(x + CellSize, y, x, y + CellSize);
                    }
                }
            }
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






    }
}