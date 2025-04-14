using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using xMxLibary;

namespace X3UR_Prototype {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private int activeGrid;
        private GrowingGrid growingGrid;
        private TestZoomingGrid testZoomingGrid;
        private int tZGDistanceX;
        private int tZGDistanceY;
        private int tZGCount;

        public static Dictionary<byte, Brush> raceColors;
        public static Dictionary<byte, string> raceByNumber;

        public MainWindow() {
            InitializeComponent();

            InitRaceColor();
            InitRaceByNumber();
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.D1:
                    activeGrid = 1;
                    DrawTestZoomingGrid();
                    break;
                case Key.D2:
                    activeGrid = 2;
                    DrawZoomingGrid();
                    break;
                case Key.D3:
                    activeGrid = 3;
                    DrawGrowingGrid();
                    break;
                case Key.Space:
                    switch (activeGrid) {
                        case 1:
                            DrawTestZoomingGridAdd(1);
                            break;
                        case 3:
                            growingGrid.StepGrow();
                            DrawGrowingGridAdd();
                            break;
                        default:
                            break;
                    }
                    break;
                case Key.Enter:
                    switch (activeGrid) {
                        case 1:
                            DrawTestZoomingGrid();
                            DrawTestZoomingGridAdd(5);
                            break;
                        case 3:
                            growingGrid.FullGrow();
                            DrawGrowingGridAdd();
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        ///////////////////
        /// GrowingGrid ///
        ///////////////////

        private void DrawGrowingGrid() {
            int HEIGTH = 20;
            int WIDTH = 24;
            int GRIDSIZE = 460;

            canvas.Children.Clear();
            listBox_sectors.Items.Clear();
            growingGrid = new GrowingGrid(listBox_sectors, WIDTH, HEIGTH);
            DrawGridOnCanvas(growingGrid.map, GRIDSIZE / HEIGTH, 0, 0);
        }

        private void DrawGrowingGridAdd() {
            int HEIGTH = 20;
            int GRIDSIZE = 460;

            canvas.Children.Clear();
            listBox_sectors.Items.Clear();
            DrawGridOnCanvas(growingGrid.map, GRIDSIZE / HEIGTH, 0, 0);
        }

        ///////////////////////
        /// TestZoomingGrid ///
        ///////////////////////

        private void DrawTestZoomingGrid() {
            int GRIDSIZE = 225;
            tZGDistanceX = 0;
            tZGDistanceY = 0;
            tZGCount = 1;

            canvas.Children.Clear();
            listBox_sectors.Items.Clear();

            List<int> racesToAdd = new List<int> { 1, 2, 3, 4, 5, 6, 8, 9, 10 };

            int[,] srcGrid = new int[3, 3];

            for (int y = 0; y < srcGrid.GetLength(0); y++) {
                for (int x = 0; x < srcGrid.GetLength(1); x++) {
                    int rndNumber = XMath.RandomNumber(0, racesToAdd.Count);
                    srcGrid[y, x] = racesToAdd[rndNumber];
                    racesToAdd.RemoveAt(rndNumber);
                }
            }

            testZoomingGrid = new TestZoomingGrid(srcGrid);
            DrawGridOnCanvas(testZoomingGrid.grid, GRIDSIZE / testZoomingGrid.Size, 0, 0);
        }

        private void DrawTestZoomingGridAdd(int count) {
            int GRIDSIZE = 225;

            for (int i = 0; i < count; i++) {
                tZGDistanceX += 225 + 10;
                testZoomingGrid.ZoomIn();
                DrawGridOnCanvas(testZoomingGrid.grid, GRIDSIZE / testZoomingGrid.Size, tZGDistanceX, tZGDistanceY);
                tZGCount++;

                if (tZGCount == 3) {
                    tZGDistanceX = -235;
                    tZGDistanceY = 225 + 10;
                }
            }
        }

        ///////////////////
        /// ZoomingGrid ///
        ///////////////////

        private void DrawZoomingGrid() {
            canvas.Children.Clear();
            listBox_sectors.Items.Clear();

            List<byte> racesToAdd = new List<byte> { 1, 2, 3, 4, 5, 6, 8, 9, 10 };

            byte[,] srcGrid = new byte[3, 3];

            for (int y = 0; y < srcGrid.GetLength(0); y++) {
                for (int x = 0; x < srcGrid.GetLength(1); x++) {
                    byte rndNumber = (byte)XMath.RandomNumber(0, racesToAdd.Count);
                    srcGrid[y, x] = racesToAdd[rndNumber];
                    racesToAdd.RemoveAt(rndNumber);
                }
            }


            ZoomingGrid grid1 = new ZoomingGrid(srcGrid, listBox_sectors);
            short size = Size((short)grid1.grid.GetLength(0));

            DrawGridOnCanvas(grid1.grid, size, PosX(size, 0), PosY(size, 0));

            size = Size((short)grid1.gridPhase1.GetLength(0));
            short spacing = (short)(size * grid1.gridPhase1.GetLength(0));

            DrawGridOnCanvas(grid1.gridPhase1, size, PosX(spacing, 1), PosY(spacing, 0));
            DrawGridOnCanvas(grid1.gridPhase2, size, PosX(spacing, 1), PosY(spacing, 1));
            DrawGridOnCanvas(grid1.gridPhase3, size, PosX(spacing, 1), PosY(spacing, 2));

            ZoomingGrid grid2 = new ZoomingGrid(grid1.gridPhase3, listBox_sectors, grid1.raceSectorCount, grid1.raceZoneCount, grid1.overallSectorCount, grid1.stage);
            size = Size((short)grid2.gridPhase1.GetLength(0));

            DrawGridOnCanvas(grid2.gridPhase1, size, PosX(spacing, 2), PosY(spacing, 0));
            DrawGridOnCanvas(grid2.gridPhase2, size, PosX(spacing, 2), PosY(spacing, 1));
            DrawGridOnCanvas(grid2.gridPhase3, size, PosX(spacing, 2), PosY(spacing, 2));

            listBox_sectors.Items.Add("====================");

            ZoomingGrid grid3 = new ZoomingGrid(grid2.gridPhase3, listBox_sectors, grid2.raceSectorCount, grid2.raceZoneCount, grid2.overallSectorCount, grid2.stage);
            size = Size((short)grid3.gridPhase1.GetLength(0));

            DrawGridOnCanvas(grid3.gridPhase1, size, PosX(spacing, 3), PosY(spacing, 0));
            DrawGridOnCanvas(grid3.gridPhase2, size, PosX(spacing, 3), PosY(spacing, 1));
            DrawGridOnCanvas(grid3.gridPhase3, size, PosX(spacing, 3), PosY(spacing, 2));
        }

        private short Size(short gridSize) {
            listBox_sectors.Items.Add("");
            listBox_sectors.Items.Add("#=#=#=#=#=#=#=#=#=#=#" + (short)Math.Round((float)256 / gridSize, 1));
            listBox_sectors.Items.Add("");
            return (short)Math.Round((float)256 / gridSize, 1);
        }

        private short PosX(short size, byte phase) {
            return (short)((size * phase) + (16 * phase));
        }

        private short PosY(short size, byte phase) {
            return (short)((size * phase) + (16 * phase));
        }

        private void DrawGridOnCanvas(GrowingGridTile[,] growingGrid, int size, int spaceLeft, int spaceTop) {
            Rectangle rectangle = null;

            for (int y = 0; y < growingGrid.GetLength(0); y++) {
                for (int x = 0; x < growingGrid.GetLength(1); x++) {
                    if (growingGrid[y, x] != null) {
                        rectangle = DrawRectangle(size, raceColors[(byte)growingGrid[y, x].Race]);
                        canvas.Children.Add(rectangle);
                        Canvas.SetLeft(rectangle, x * (rectangle.Width + 2) + spaceLeft);
                        Canvas.SetTop(rectangle, y * (rectangle.Height + 2) + spaceTop);
                    }
                }
            }
        }

        private void DrawGridOnCanvas(int[,] zoomingGrid, int size, int spaceLeft, int spaceTop) {
            Rectangle rectangle = null;

            for (byte y = 0; y < zoomingGrid.GetLength(0); y++) {
                for (byte x = 0; x < zoomingGrid.GetLength(1); x++) {
                    if (zoomingGrid[y, x] >= 1 && zoomingGrid[y, x] <= 10) {
                        rectangle = DrawRectangle(size, raceColors[(byte)zoomingGrid[y, x]]);
                        canvas.Children.Add(rectangle);
                        Canvas.SetLeft(rectangle, x * rectangle.Width + spaceLeft);
                        Canvas.SetTop(rectangle, y * rectangle.Height + spaceTop);
                    }
                }
            }
        }

        private void DrawGridOnCanvas(ZoomingGridTile[,] zoomingGrid, short size, int spaceLeft, int spaceTop) {
            Rectangle rectangle = null;

            for (byte y = 0; y < zoomingGrid.GetLength(0); y++) {
                for (byte x = 0; x < zoomingGrid.GetLength(1); x++) {
                    if (zoomingGrid[y, x] != null) {
                        rectangle = DrawRectangle(size, raceColors[zoomingGrid[y, x].race]);
                        canvas.Children.Add(rectangle);
                        Canvas.SetLeft(rectangle, x * rectangle.Width + spaceLeft);
                        Canvas.SetTop(rectangle, y * rectangle.Height + spaceTop);
                    }
                }
            }
        }

        private Rectangle DrawRectangle(double size, Brush brush) {
            if (brush == null)
                brush = Brushes.Black;

            Rectangle rectangle = new Rectangle {
                Width = size,
                Height = size,
                Fill = brush,
                //Stroke = Brushes.Black,
                //StrokeThickness = 1
            };

            return rectangle;
        }

        private void InitRaceColor() {
            raceColors = new Dictionary<byte, Brush> {
                { 0, Brushes.Black },
                { 1, Brushes.Orange },
                { 2, Brushes.LightGreen },
                { 3, Brushes.LightYellow },
                { 4, Brushes.LightBlue },
                { 5, Brushes.LightCyan },
                { 6, Brushes.Red },
                { 7, Brushes.Violet },
                { 8, Brushes.Pink },
                { 9, Brushes.Gray },
                { 10, Brushes.Blue },
                { 11, Brushes.Gold }
            };
        }

        private void InitRaceByNumber() {
            raceByNumber = new Dictionary<byte, string> {
                { 0, "Space" },
                { 1, "Argon" },
                { 2, "Boron" },
                { 3, "Split" },
                { 4, "Paranid" },
                { 5, "Teladi" },
                { 6, "Xenon" },
                { 7, "Khaak" },
                { 8, "Pirate" },
                { 9, "Unknown" },
                { 10, "Terran" },
                { 11, "Yaki" }
            };
        }
    }
}