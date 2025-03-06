using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace UIPredator
{
    public partial class MainWindow : Window
    {
        private const int GridSize = 5;
        private const double CellSize = 100;

        private List<Ellipse> tigers = new List<Ellipse>();
        private List<Ellipse> goats = new List<Ellipse>();

        public MainWindow()
        {
            InitializeComponent();
            DrawBoard();
            //PlaceInitialTigers();

            //BoardCanvas.MouseLeftButtonDown += PlaceGoat;
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

        private Ellipse CreatePiece(Brush color)
        {
            return new Ellipse
            {
                Width = 30,
                Height = 30,
                Fill = color,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
        }

        private void PositionPiece(Ellipse piece, int row, int col)
        {
            Canvas.SetLeft(piece, col * CellSize + (CellSize - piece.Width) / 2);
            Canvas.SetTop(piece, row * CellSize + (CellSize - piece.Height) / 2);
        }
    }
}