using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using xMxLibary;

namespace X3UR_Prototype {
    class ZoomingGrid {
        public byte[,] bGrid;
        public ZoomingGridTile[,] grid;
        public ZoomingGridTile[,] gridPhase1;
        public ZoomingGridTile[,] gridPhase2;
        public ZoomingGridTile[,] gridPhase3;
        public short gridSize;
        public ListBox listBox;
        private const byte ALLRACES = 12;

        public byte stage = 0;
        public short overallSectorCount = 0;
        private readonly short[] maxRaceZones = { 0, 4, 4, 4, 4, 4, 3, 3, 2, 6, 1, 1 };
        public short[] raceSectorCount = new short[ALLRACES];
        public short[] raceZoneCount = new short[ALLRACES];
        public readonly float[] minZoneSizes = { 0, 0.0134f, 0.0107f, 0.0080f, 0.0294f, 0.0053f, 0.0107f, 0.0027f, 0.0160f, 0.0027f, 0.0535f, 0.0080f };
        //public readonly byte[] minZoneSize = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        public readonly float[] maxZoneSizes = { 0, 0.0428f, 0.0187f, 0.0348f, 0.0374f, 0.0241f, 0.0107f, 0.0027f, 0.0214f, 0.0053f, 0.0535f, 0.0080f };
        public readonly float[] maxRaceSizeF = { 0, 0.0856f, 0.0722f, 0.0829f, 0.0722f, 0.0802f, 0.0160f, 0.0080f, 0.0401f, 0.0214f, 0.0535f, 0.0080f };

        public ZoomingGrid(byte[,] grid, ListBox listBox) {
            bGrid = grid;
            this.listBox = listBox;

            Phase0();
            Phase1();
            ShowInfos("Phase 1");
            Phase2();
            ShowInfos("Phase 2");
            Phase3();
            ShowInfos("Phase 3");
        }

        public ZoomingGrid(ZoomingGridTile[,] grid, ListBox listBox, short[] raceSectorCount, short[] raceZoneCount, short overallSectorCount, byte stage) {
            this.grid = grid;
            this.listBox = listBox;
            this.raceSectorCount = raceSectorCount;
            this.raceZoneCount = raceZoneCount;
            this.overallSectorCount = overallSectorCount;
            this.stage = stage;

            Phase1();
            ShowInfos("Phase 1");
            Phase2();
            ShowInfos("Phase 2");
            Phase3();
            ShowInfos("Phase 3");
        }

        public void Phase0() {
            ZoomingGridTile[,] sourceGrid = new ZoomingGridTile[bGrid.GetLength(0), bGrid.GetLength(1)];

            for (byte y = 0; y < sourceGrid.GetLength(0); y++) {
                for (byte x = 0; x < sourceGrid.GetLength(1); x++) {

                    if (x != 0 && bGrid[y, x] == bGrid[y, x - 1]) {
                        sourceGrid[y, x] = new ZoomingGridTile(sourceGrid[y, x - 1]);
                    }
                    else if (y != 0 && bGrid[y, x] == bGrid[y - 1, x]) {
                        sourceGrid[y, x] = new ZoomingGridTile(sourceGrid[y - 1, x]);
                    }
                    else {
                        sourceGrid[y, x] = new ZoomingGridTile(bGrid[y, x]);
                        raceZoneCount[sourceGrid[y, x].race]++;
                    }

                    raceSectorCount[sourceGrid[y, x].race]++;
                    overallSectorCount++;
                }
            }

            grid = sourceGrid;
        }

        public void Phase1() { // TODO
            ZoomingGridTile[,] zoomedGrid = new ZoomingGridTile[grid.GetLength(0) * 2 - 1, grid.GetLength(1) * 2 - 1];
            gridSize = (short)(zoomedGrid.GetLength(0) * zoomedGrid.GetLength(1));

            for (byte y = 0; y < zoomedGrid.GetLength(0); y++) {
                for (byte x = 0; x < zoomedGrid.GetLength(1); x++) {
                    // Jedes Feld, mit einem Ungeraden x oder y Index
                    if (y % 2 == 0 && x % 2 == 0) {
                        zoomedGrid[y, x] = grid[y / 2, x / 2];
                    }
                }
            }

            gridPhase1 = zoomedGrid;
        }

        public void Phase2() {
            ZoomingGridTile[,] zoomedGrid = (ZoomingGridTile[,])gridPhase1.Clone();
            stage++;

            for (byte y = 0; y < zoomedGrid.GetLength(0); y++) {
                for (byte x = 0; x < zoomedGrid.GetLength(1); x++) {
                    // Horizontal (x)
                    if (y % 2 == 0 && x % 2 != 0 && x != 0) {
                        ZoomingGridTile sectorLeft = zoomedGrid[y, x - 1];
                        ZoomingGridTile sectorRight = zoomedGrid[y, x + 1];

                        ZoomingGridTile[] neighbors = { sectorLeft, sectorRight };

                        listBox.Items.Add("");
                        listBox.Items.Add("[" + x + "][" + y + "]  " + raceSectorCount[sectorLeft.race] + " | " + raceSectorCount[sectorRight.race]);

                        if (AreRaces_UnderMinZoneSize(neighbors)) {
                            listBox.Items.Add("Beide unter minZoneSize: ");
                            if (AreSameNeighborRaces(neighbors)) {
                                listBox.Items.Add("\tBeide gleich: ");

                                if (sectorLeft.Parent != sectorRight.Parent)
                                    raceZoneCount[sectorLeft.race]--;

                                zoomedGrid[y, x] = new ZoomingGridTile(neighbors);
                            }
                            else
                                zoomedGrid[y, x] = new ZoomingGridTile(ChooseRace_UnderMinZoneSize(neighbors));
                        }
                        else if (IsRace_OneOfThemUnderMinZoneSize(neighbors)) {
                            listBox.Items.Add("Einer unter minZoneSize: ");
                            zoomedGrid[y, x] = new ZoomingGridTile(ChooseRace_OneOfThemUnderMinZoneSize(neighbors));
                        }
                        else if (AreRaces_OverMaxZoneSize(neighbors)) {
                            listBox.Items.Add("Beide über maxZoneSize: ");

                            if (AreNeighborsSameParents(neighbors))
                                zoomedGrid[y, x] = new ZoomingGridTile(neighbors);
                            else {
                                byte rndFiller = GetRandomFiller(neighbors);

                                if (stage == 1 && rndFiller == 0) {
                                    rndFiller = neighbors[(byte)XMath.RandomNumber(0, (short)(neighbors.Length - 1))].race;
                                }

                                if (!IsANeighborSameFiller(rndFiller, neighbors)) {
                                    zoomedGrid[y, x] = new ZoomingGridTile(rndFiller);
                                    raceZoneCount[zoomedGrid[y, x].race]++;
                                }
                                else { // JUST CHECK IF IT WORKS
                                    listBox.Items.Add("\tNachbar(n) mit selben Filler: ");
                                    ZoomingGridTile[] neighorsWithSameFiller = GetNeighorsWithSameFiller(rndFiller, neighbors);

                                    if (neighorsWithSameFiller.Length > 1) {
                                        byte sameNeighborsCount = CountNeighborsNotSameParent(neighorsWithSameFiller);
                                        raceZoneCount[neighorsWithSameFiller[0].race] -= sameNeighborsCount;
                                    }
                                    zoomedGrid[y, x] = new ZoomingGridTile(neighorsWithSameFiller);
                                }
                            }
                        }
                        else if (IsRace_OneOfThemOverMaxZoneSize(neighbors)) {
                            listBox.Items.Add("Einer über maxZoneSize: ");
                            zoomedGrid[y, x] = new ZoomingGridTile(ChooseRace_OneOfThemOverMaxZoneSize(neighbors));
                        }
                        else {
                            if (AreSameNeighborRaces(neighbors)) {
                                listBox.Items.Add("Beide gleich: ");

                                if (sectorLeft.Parent != sectorRight.Parent)
                                    raceZoneCount[sectorLeft.race]--;

                                zoomedGrid[y, x] = new ZoomingGridTile(neighbors);
                            }
                            else {
                                if (AreRaces_OverSectorsLimit(neighbors)) {
                                    listBox.Items.Add("Beide über Sektor Limit: ");

                                    byte rndFiller = GetRandomFiller(neighbors);

                                    if (stage == 1 && rndFiller == 0) {
                                        rndFiller = neighbors[(byte)XMath.RandomNumber(0, (short)(neighbors.Length - 1))].race;
                                    }

                                    if (!IsANeighborSameFiller(rndFiller, neighbors)) {
                                        zoomedGrid[y, x] = new ZoomingGridTile(rndFiller);
                                        raceZoneCount[zoomedGrid[y, x].race]++;
                                    }
                                    else {
                                        listBox.Items.Add("\tNachbar(n) mit selben Filler: ");
                                        ZoomingGridTile[] neighorsWithSameFiller = GetNeighorsWithSameFiller(rndFiller, neighbors);

                                        if (neighorsWithSameFiller.Length > 1) {
                                            byte sameNeighborsCount = CountNeighborsNotSameParent(neighorsWithSameFiller);
                                            raceZoneCount[neighorsWithSameFiller[0].race] -= sameNeighborsCount;
                                        }
                                        zoomedGrid[y, x] = new ZoomingGridTile(neighorsWithSameFiller);
                                    }
                                }
                                else {
                                    zoomedGrid[y, x] = new ZoomingGridTile(neighbors[XMath.RandomNumber(0, (short)(neighbors.Length - 1))]);
                                }
                            }
                        }

                        listBox.Items.Add("ZoneSize: " + zoomedGrid[y, x].Parent.CurrentZoneSize);
                        raceSectorCount[zoomedGrid[y, x].race]++;
                        overallSectorCount++;
                    }
                    // Vertikal (y)
                    else if (y % 2 != 0 && x % 2 == 0 && y != 0) {
                        ZoomingGridTile sectorTop = zoomedGrid[y - 1, x];
                        ZoomingGridTile sectorBottom = zoomedGrid[y + 1, x];

                        ZoomingGridTile[] neighbors = { sectorTop, sectorBottom };

                        listBox.Items.Add("");
                        listBox.Items.Add("[" + x + "][" + y + "]  " + raceSectorCount[sectorTop.race] + "|" + raceSectorCount[sectorBottom.race]);

                        if (AreRaces_UnderMinZoneSize(neighbors)) {
                            listBox.Items.Add("Beide unter minZoneSize: ");
                            if (AreSameNeighborRaces(neighbors)) {
                                listBox.Items.Add("\tBeide gleich: ");

                                if (sectorTop.Parent != sectorBottom.Parent)
                                    raceZoneCount[sectorTop.race]--;

                                zoomedGrid[y, x] = new ZoomingGridTile(neighbors);
                            }
                            else
                                zoomedGrid[y, x] = new ZoomingGridTile(ChooseRace_UnderMinZoneSize(neighbors));
                        }
                        else if (IsRace_OneOfThemUnderMinZoneSize(neighbors)) {
                            listBox.Items.Add("Einer unter minZoneSize: ");
                            zoomedGrid[y, x] = new ZoomingGridTile(ChooseRace_OneOfThemUnderMinZoneSize(neighbors));
                        }
                        else if (AreRaces_OverMaxZoneSize(neighbors)) {
                            listBox.Items.Add("Beide über maxZoneSize: ");

                            if (AreNeighborsSameParents(neighbors))
                                zoomedGrid[y, x] = new ZoomingGridTile(neighbors);
                            else {
                                byte rndFiller = GetRandomFiller(neighbors);

                                if (stage == 1 && rndFiller == 0) {
                                    rndFiller = neighbors[(byte)XMath.RandomNumber(0, (short)(neighbors.Length - 1))].race;
                                }

                                if (!IsANeighborSameFiller(rndFiller, neighbors)) {
                                    zoomedGrid[y, x] = new ZoomingGridTile(rndFiller);
                                    raceZoneCount[zoomedGrid[y, x].race]++;
                                }
                                else {
                                    listBox.Items.Add("\tNachbar(n) mit selben Filler: ");
                                    ZoomingGridTile[] neighorsWithSameFiller = GetNeighorsWithSameFiller(rndFiller, neighbors);

                                    if (neighorsWithSameFiller.Length > 1) {
                                        byte sameNeighborsCount = CountNeighborsNotSameParent(neighorsWithSameFiller);
                                        raceZoneCount[neighorsWithSameFiller[0].race] -= sameNeighborsCount;
                                    }
                                    zoomedGrid[y, x] = new ZoomingGridTile(neighorsWithSameFiller);
                                }
                            }
                        }
                        else if (IsRace_OneOfThemOverMaxZoneSize(neighbors)) {
                            listBox.Items.Add("Einer über maxZoneSize: ");
                            zoomedGrid[y, x] = new ZoomingGridTile(ChooseRace_OneOfThemOverMaxZoneSize(neighbors));
                        }
                        else {
                            if (AreSameNeighborRaces(neighbors)) {
                                listBox.Items.Add("Beide gleich: ");

                                if (sectorTop.Parent != sectorBottom.Parent)
                                    raceZoneCount[sectorTop.race]--;

                                zoomedGrid[y, x] = new ZoomingGridTile(neighbors);
                            }
                            else {
                                if (AreRaces_OverSectorsLimit(neighbors)) {
                                    listBox.Items.Add("Beide über Sektor Limit: ");

                                    byte rndFiller = GetRandomFiller(neighbors);

                                    if (stage == 1 && rndFiller == 0) {
                                        rndFiller = neighbors[(byte)XMath.RandomNumber(0, (short)(neighbors.Length - 1))].race;
                                    }

                                    if (!IsANeighborSameFiller(rndFiller, neighbors)) {
                                        zoomedGrid[y, x] = new ZoomingGridTile(rndFiller);
                                        raceZoneCount[zoomedGrid[y, x].race]++; 
                                    }
                                    else {
                                        listBox.Items.Add("\tNachbar(n) mit selben Filler: ");
                                        ZoomingGridTile[] neighorsWithSameFiller = GetNeighorsWithSameFiller(rndFiller, neighbors);

                                        if (neighorsWithSameFiller.Length > 1) {
                                            byte sameNeighborsCount = CountNeighborsNotSameParent(neighorsWithSameFiller);
                                            raceZoneCount[neighorsWithSameFiller[0].race] -= sameNeighborsCount;
                                        }
                                        zoomedGrid[y, x] = new ZoomingGridTile(neighorsWithSameFiller);
                                    }
                                }
                                else
                                    zoomedGrid[y, x] = new ZoomingGridTile(neighbors[XMath.RandomNumber(0, (short)(neighbors.Length - 1))]);
                            }
                        }

                        listBox.Items.Add("ZoneSize: " + zoomedGrid[y, x].Parent.CurrentZoneSize);
                        raceSectorCount[zoomedGrid[y, x].race]++;
                        overallSectorCount++;
                    }
                    listBox.Items.Add("");
                    if (zoomedGrid[y, x] == null) {
                        listBox.Items.Add(" --- [" + x + "][" + y + "]");
                    }
                    else {
                        float minZoneSize = (float)Math.Round(minZoneSizes[zoomedGrid[y, x].race] * gridSize, 4);
                        float maxZoneSize = (float)Math.Round(maxZoneSizes[zoomedGrid[y, x].race] * gridSize, 4);
                        listBox.Items.Add(" --- [" + x + "][" + y + "] Race: " + MainWindow.raceByNumber[zoomedGrid[y, x].race]);
                        listBox.Items.Add(" --- CZS: " + zoomedGrid[y, x].CurrentZoneSize + " | MinZS: " + minZoneSize + " | MaxZS: " + maxZoneSize);
                    }
                }
            }

            gridPhase2 = zoomedGrid;
        }

        public void Phase3() {
            ZoomingGridTile[,] zoomedGrid = (ZoomingGridTile[,])gridPhase2.Clone();

            listBox.Items.Add("");
            listBox.Items.Add(" ===== Fill =====");

            for (byte y = 0; y < zoomedGrid.GetLength(0); y++) {
                for (byte x = 0; x < zoomedGrid.GetLength(1); x++) {
                    if ((y % 2 != 0 && x % 2 != 0) && (y != 0 && x != 0)) {
                        ZoomingGridTile sectorLeft = zoomedGrid[y, x - 1];
                        ZoomingGridTile sectorRight = zoomedGrid[y, x + 1];
                        ZoomingGridTile sectorTop = zoomedGrid[y - 1, x];
                        ZoomingGridTile sectorBottom = zoomedGrid[y + 1, x];

                        ZoomingGridTile[] neighbors = { sectorLeft, sectorRight, sectorTop, sectorBottom };
                        listBox.Items.Add("");
                        listBox.Items.Add("[" + x + "][" + y + "]  " + raceSectorCount[sectorLeft.race] + "|" + raceSectorCount[sectorRight.race] + "|" + raceSectorCount[sectorTop.race] + "|" + raceSectorCount[sectorBottom.race]);

                        if (AreRaces_UnderMinZoneSize(neighbors)) {
                            listBox.Items.Add("Alle unter minZoneSize: ");
                            if (AreSameNeighborRaces(neighbors)) {
                                listBox.Items.Add("\tAlle gleich: ");

                                if (neighbors.All(neighbor => neighbors[0].Parent != neighbor.Parent)) {
                                    listBox.Items.Add("Alle selbe Eltern: ");
                                    raceZoneCount[neighbors[0].race]--;
                                }
                                zoomedGrid[y, x] = new ZoomingGridTile(neighbors);
                            }
                            else {
                                zoomedGrid[y, x] = new ZoomingGridTile(ChooseRace_UnderMinZoneSize(neighbors));
                            }
                        }
                        else if (IsRace_OneOfThemUnderMinZoneSize(neighbors)) {
                            listBox.Items.Add("Einer oder mehr unter minZoneSize: ");
                            zoomedGrid[y, x] = ChooseRace_OneOfThemUnderMinZoneSize(neighbors);
                        }
                        else if (AreRaces_OverMaxZoneSize(neighbors)) {
                            listBox.Items.Add("Alle über maxZoneSize: ");

                            if (AreNeighborsSameParents(neighbors)) {
                                listBox.Items.Add("\tAlle selbe Eltern: ");
                                zoomedGrid[y, x] = new ZoomingGridTile(neighbors);
                            }
                            else {
                                byte rndFiller = GetRandomFiller(neighbors);

                                if (stage == 1 && rndFiller == 0) {
                                    rndFiller = neighbors[(byte)XMath.RandomNumber(0, (short)(neighbors.Length - 1))].race;
                                }

                                if (!IsANeighborSameFiller(rndFiller, neighbors)) {
                                    listBox.Items.Add("\tKEIN Nachbar mit selben Filler: ");
                                    zoomedGrid[y, x] = new ZoomingGridTile(rndFiller);
                                    raceZoneCount[zoomedGrid[y, x].race]++;
                                }
                                else {
                                    listBox.Items.Add("\tNachbar(n) mit selben Filler: ");
                                    ZoomingGridTile[] neighorsWithSameFiller = GetNeighorsWithSameFiller(rndFiller, neighbors);

                                    if (neighorsWithSameFiller.Length > 1) {
                                        byte sameNeighborsCount = CountNeighborsNotSameParent(neighorsWithSameFiller);
                                        raceZoneCount[neighorsWithSameFiller[0].race] -= sameNeighborsCount;
                                    }
                                    zoomedGrid[y, x] = new ZoomingGridTile(neighorsWithSameFiller);
                                }
                            }
                        }
                        else if (IsRace_OneOfThemOverMaxZoneSize(neighbors)) {
                            listBox.Items.Add("Einer oder mehr über maxZoneSize: ");
                            zoomedGrid[y, x] = ChooseRace_OneOfThemOverMaxZoneSize(neighbors);
                        }
                        else {
                            // Wenn alle Nachbarn gleich sind
                            if (AreSameNeighborRaces(neighbors)) {
                                listBox.Items.Add("Alle gleich: ");

                                if (neighbors.All(neighbor => neighbors[0].Parent != neighbor.Parent)) {
                                    listBox.Items.Add("Alle selbe Eltern: ");
                                    raceZoneCount[neighbors[0].race]--;
                                }
                                zoomedGrid[y, x] = new ZoomingGridTile(neighbors);
                            }
                            else {
                                // Wenn alle Nachbarn über ihrer max. Anzahl an Sektoren sind
                                if (AreRaces_OverSectorsLimit(neighbors)) {
                                    listBox.Items.Add("Alle über Sektor Limit: ");

                                    byte rndFiller = GetRandomFiller(neighbors);

                                    if (stage == 1 && rndFiller == 0) {
                                        rndFiller = neighbors[(byte)XMath.RandomNumber(0, (short)(neighbors.Length - 1))].race;
                                    }

                                    if (!IsANeighborSameFiller(rndFiller, neighbors)) {
                                        listBox.Items.Add("\tKEIN Nachbar mit selben Filler: ");
                                        zoomedGrid[y, x] = new ZoomingGridTile(rndFiller);
                                        raceZoneCount[zoomedGrid[y, x].race]++;
                                    }
                                    else {
                                        listBox.Items.Add("\tNachbar(n) mit selben Filler: ");
                                        ZoomingGridTile[] neighorsWithSameFiller = GetNeighorsWithSameFiller(rndFiller, neighbors);

                                        if (neighorsWithSameFiller.Length > 1) {
                                            byte sameNeighborsCount = CountNeighborsNotSameParent(neighorsWithSameFiller);
                                            raceZoneCount[neighorsWithSameFiller[0].race] -= sameNeighborsCount;
                                        }
                                        zoomedGrid[y, x] = new ZoomingGridTile(neighorsWithSameFiller);
                                    }
                                }
                                else {
                                    listBox.Items.Add("Wähle einen der Nachbarn: ");
                                    zoomedGrid[y, x] = new ZoomingGridTile(neighbors[XMath.RandomNumber(0, (short)(neighbors.Length - 1))]);
                                }
                            }
                        }

                        listBox.Items.Add("ZoneSize: " + zoomedGrid[y, x].Parent.CurrentZoneSize);
                        raceSectorCount[zoomedGrid[y, x].race]++;
                        overallSectorCount++;
                    }
                }
            }

            gridPhase3 = zoomedGrid;
        }

        private bool AreRaces_UnderMinZoneSize(ZoomingGridTile[] neighbors) {
            for (int i = 0; i < neighbors.Length; i++) {
                float minZoneSize = (float)Math.Round(minZoneSizes[neighbors[i].race] * gridSize, 4);

                if (neighbors[i].CurrentZoneSize >= minZoneSize && neighbors[i].race != 0) {
                    return false;
                }
            }

            return true;
        }

        private ZoomingGridTile ChooseRace_UnderMinZoneSize(ZoomingGridTile[] neighbors) {
            ZoomingGridTile neighbor = null;
            float value = 1;

            for (int i = 0; i < neighbors.Length; i++) {
                float minZoneSize = (float)Math.Round(minZoneSizes[neighbors[i].race] * gridSize, 4);
                float percentageSize = (float)Math.Round(neighbors[i].CurrentZoneSize / minZoneSize, 4);

                if (percentageSize < value && neighbors[i].race != 0) {
                    value = percentageSize;
                    neighbor = neighbors[i];
                }
            }

            return neighbor;
        }

        private bool IsRace_OneOfThemUnderMinZoneSize(ZoomingGridTile[] neighbors) {
            byte countTruths = 0;

            for (int i = 0; i < neighbors.Length; i++) {
                float minZoneSize = (float)Math.Round(minZoneSizes[neighbors[i].race] * gridSize, 4);

                if (neighbors[i].CurrentZoneSize < minZoneSize && neighbors[i].race != 0) {
                    countTruths++;
                }
            }

            if (countTruths > 0 && countTruths < 4)
                return true;
            return false;
        }

        private ZoomingGridTile ChooseRace_OneOfThemUnderMinZoneSize(ZoomingGridTile[] neighbors) {
            float value = 1;
            ZoomingGridTile neighbor = null;

            for (int i = 0; i < neighbors.Length; i++) {
                float minZoneSize = (float)Math.Round(minZoneSizes[neighbors[i].race] * gridSize, 4);

                if (neighbors[i].CurrentZoneSize < minZoneSize && neighbors[i].race != 0) {
                    float zoneSizePercentage = (float)Math.Round(neighbors[i].CurrentZoneSize / minZoneSize, 4);

                    if (zoneSizePercentage < value) {
                        value = zoneSizePercentage;
                        neighbor = neighbors[i];
                    }
                }
            }

            return neighbor;
        }

        private bool AreRaces_OverMaxZoneSize(ZoomingGridTile[] neighbors) {
            for (int i = 0; i < neighbors.Length; i++) {
                float maxZoneSize = (float)Math.Round(maxZoneSizes[neighbors[i].race] * gridSize, 4);

                if (neighbors[i].CurrentZoneSize < maxZoneSize && neighbors[i].race != 0) {
                    return false;
                }
            }

            return true;
        }

        private bool IsRace_OneOfThemOverMaxZoneSize(ZoomingGridTile[] neighbors) {
            byte countTruths = 0;

            for (int i = 0; i < neighbors.Length; i++) {
                float maxZoneSize = (float)Math.Round(maxZoneSizes[neighbors[i].race] * gridSize, 4);

                if (neighbors[i].CurrentZoneSize >= maxZoneSize && neighbors[i].race != 0) {
                    countTruths++;
                }
            }

            if (countTruths > 0 && countTruths < 4)
                return true;
            return false;
        }

        private ZoomingGridTile ChooseRace_OneOfThemOverMaxZoneSize(ZoomingGridTile[] neighbors) {
            List<ZoomingGridTile> underMaxZoneSizeNeighbors = new List<ZoomingGridTile>();

            for (int i = 0; i < neighbors.Length; i++) {
                float maxZoneSize = (float)Math.Round(maxZoneSizes[neighbors[i].race] * gridSize, 4);

                if (neighbors[i].CurrentZoneSize < maxZoneSize && neighbors[i].race != 0) {
                    underMaxZoneSizeNeighbors.Add(neighbors[i]);
                }
            }

            return underMaxZoneSizeNeighbors[XMath.RandomNumber(0, (short)(underMaxZoneSizeNeighbors.Count - 1))];
        }

        private bool AreRaces_OverSectorsLimit(params ZoomingGridTile[] neighbors) {
            for (int i = 0; i < neighbors.Length; i++) {
                float currentSectorSizes = (float)Math.Round((float)raceSectorCount[neighbors[i].race] / gridSize, 4);

                if (currentSectorSizes < maxRaceSizeF[neighbors[i].race] && neighbors[i].race != 0) {
                    return false;
                }
            }

            return true;
        }

        private bool AreSameNeighborRaces(params ZoomingGridTile[] neighbors) {
            for (int i = 1; i < neighbors.Length; i++) {
                if (neighbors[0].race != neighbors[i].race) {
                    return false;
                }
            }
            return true;
        }

        private bool AreNeighborsSameParents(params ZoomingGridTile[] neighbors) {
            if (neighbors.All(neighbor => neighbors[0].Parent == neighbor.Parent))
                return true;
            return false;
        }

        private byte GetRandomFiller(params ZoomingGridTile[] neighbors) { // TOTO Schauen, wie es sich verhält
            List<byte> racesUnderMaxSectorSize = new List<byte>();

            for (byte race = 1; race < ALLRACES; race++) {
                short maxSectors = (short)Math.Round(gridSize * maxRaceSizeF[race], 1);
                short raceSectorsLeft = (short)(maxSectors - raceSectorCount[race]);
                if (raceZoneCount[race] < 1) {
                    raceSectorsLeft++;
                }

                for (short number = 0; number < raceSectorsLeft; number++) {
                    if (IsANeighborSameTheRaceAndUnderMaxZoneSize(race, neighbors) || raceZoneCount[race] < maxRaceZones[race])
                        racesUnderMaxSectorSize.Add(race);
                }
            }

            short sectorsLeft = (short)(gridSize - overallSectorCount - racesUnderMaxSectorSize.Count);

            if (stage != 1) {
                for (short i = 0; i < sectorsLeft; i++) {
                    racesUnderMaxSectorSize.Add(0);
                }
            }

            if (racesUnderMaxSectorSize.Count > 0) {
                short rndNumber = (short)XMath.RandomNumber(0, racesUnderMaxSectorSize.Count - 1);
                byte choosenRacdFromList = racesUnderMaxSectorSize[rndNumber];

                racesUnderMaxSectorSize.RemoveAt(rndNumber);

                return choosenRacdFromList;
            }
            else {
                return GetPercentageSmallestRace();
            }         
        }

        private byte GetPercentageSmallestRace() {
            float value = 1;
            byte smallestRace = 0;

            listBox.Items.Add("");
            listBox.Items.Add("###### Smallest ######");
            listBox.Items.Add("");

            for (byte race = 1; race < ALLRACES; race++) {
                float maxSectors = (float)Math.Round(gridSize * maxRaceSizeF[race], 4);
                float percentageSectors = (float)Math.Round(raceSectorCount[race] / maxSectors, 4);
                listBox.Items.Add("maxS; " + maxSectors + " | pS: " + percentageSectors);

                if (percentageSectors < value) {
                    value = percentageSectors;
                    smallestRace = race;
                    listBox.Items.Add("Value: " + value);
                }
            }
            listBox.Items.Add("######################");

            return smallestRace;
        }

        private bool IsANeighborSameTheRaceAndUnderMaxZoneSize(byte race, params ZoomingGridTile[] neighbors) {
            byte countTrueths = 0;

            foreach (ZoomingGridTile neighbor in neighbors) {
                float maxZoneSize = (float)Math.Round(maxZoneSizes[neighbor.race] * gridSize, 4);

                if (race == neighbor.race) {
                    if (neighbor.CurrentZoneSize < maxZoneSize) {
                        countTrueths++;
                    }
                }
            }

            if (countTrueths == neighbors.Length)
                return true;
            return false;
        }

        private bool IsANeighborSameFiller(byte race, params ZoomingGridTile[] neighbors) {
            foreach (ZoomingGridTile neighbor in neighbors) {
                if (neighbor.race == race) {
                    return true;
                }
            }
            return false;
        }

        private ZoomingGridTile[] GetNeighorsWithSameFiller(byte race, params ZoomingGridTile[] neighbors) {
            List<ZoomingGridTile> neighborsWithSameFillerList = new List<ZoomingGridTile>();

            foreach (ZoomingGridTile neighbor in neighbors) {
                if (neighbor.race == race) {
                    neighborsWithSameFillerList.Add(neighbor);
                }
            }

            ZoomingGridTile[] neighborsWithSameFiller = new ZoomingGridTile[neighborsWithSameFillerList.Count];

            for (int i = 0; i < neighborsWithSameFillerList.Count; i++) {
                neighborsWithSameFiller[i] = neighborsWithSameFillerList[i];
            }

            return neighborsWithSameFiller;
        }

        private byte CountNeighborsNotSameParent(params ZoomingGridTile[] neighbors) {
            byte sameNeighborsCount = 0;

            for (int i = 1; i < neighbors.Length; i++) {
                if (neighbors[0].Parent != neighbors[i].Parent) {
                    sameNeighborsCount++;
                }
            }

            return sameNeighborsCount;
        }

        private void ShowInfos(string phase) {
            listBox.Items.Add("");
            listBox.Items.Add("=== " + phase + " =========");
            listBox.Items.Add("");
            listBox.Items.Add("CurrentSectorCount: " + overallSectorCount);
            listBox.Items.Add("");

            for (byte i = 0; i < raceSectorCount.GetLength(0); i++) {
                listBox.Items.Add(MainWindow.raceByNumber[i] + ": " + raceSectorCount[i] + " - " + maxRaceSizeF[i] * gridSize);
            }

            listBox.Items.Add("");

            for (byte i = 0; i < raceZoneCount.GetLength(0); i++) {
                listBox.Items.Add(MainWindow.raceByNumber[i] + ": " + raceZoneCount[i]);
            }

            listBox.Items.Add("====================");
        }
    }
}