namespace X3UR_Prototype {
    class TestZoomingGrid {
        public int[,] grid;
        public int Size { get => grid.GetLength(0); }

        public TestZoomingGrid(int[,] grid) {
            this.grid = grid;
        }

        public void ZoomIn() {
            int[,] zoomedGrid = new int[grid.GetLength(0) * 2 - 1, grid.GetLength(1) * 2 - 1];

            for (int y = 0; y < zoomedGrid.GetLength(0); y++) {
                for (int x = 0; x < zoomedGrid.GetLength(1); x++) {
                    // Jedes Feld, mit einem Ungeraden x oder y Index
                    if (y % 2 == 0 && x % 2 == 0) {
                        zoomedGrid[y, x] = grid[y / 2, x / 2];
                    }
                }
            }

            grid = zoomedGrid;

            for (int y = 0; y < grid.GetLength(0); y++) {
                for (int x = 0; x < grid.GetLength(1); x++) {
                    if (y % 2 == 0 && x % 2 != 0 && x != 0) {
                        // Überprüfe Horizontal
                        switch (xMxLibary.XMath.RandomNumber(0, 1)) {
                            case 0:
                                grid[y, x] = grid[y, x - 1];
                                break;
                            case 1:
                                grid[y, x] = grid[y, x + 1];
                                break;
                        }
                    }
                    // Überprüfe Vertikal
                    else if (y % 2 != 0 && x % 2 == 0 && y != 0) {
                        switch (xMxLibary.XMath.RandomNumber(0, 1)) {
                            case 0:
                                grid[y, x] = grid[y - 1, x];
                                break;
                            case 1:
                                grid[y, x] = grid[y + 1, x];
                                break;
                        }
                    }
                    // Überprüfe Diagonal
                    else if (((y % 2 != 0 && x % 2 != 0) && (y != 0 && x != 0))) {
                        switch (xMxLibary.XMath.RandomNumber(0, 3)) {
                            case 0:
                                grid[y, x] = grid[y - 1, x - 1];
                                break;
                            case 1:
                                grid[y, x] = grid[y - 1, x + 1];
                                break;
                            case 2:
                                grid[y, x] = grid[y + 1, x - 1];
                                break;
                            case 3:
                                grid[y, x] = grid[y + 1, x + 1];
                                break;
                        }
                    }
                }
            }
        }
    }
}
