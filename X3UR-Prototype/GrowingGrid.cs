using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using xMxLibary;

namespace X3UR_Prototype {
    class GrowingGrid {
        public GrowingGridTile[,] map;
        private ListBox listBox;
        private const byte ALLRACES = 12;

        private List<GrowingGridTile> spaceList;
        private List<int> remainingZonesList;
        private readonly List<List<GrowingGridTile>> raceList = new List<List<GrowingGridTile>>();
        private readonly List<List<GrowingGridTile>> raceListForInfos = new List<List<GrowingGridTile>>();
        private readonly short[] raceSectorCount = new short[ALLRACES];
        private readonly short[] maxRaceZones = { 0, 4, 4, 4, 4, 4, 3, 1, 2, 14, 1, 1 };
        private readonly double[] maxZoneSizesF = { 0, 0.0428, 0.0187, 0.0348, 0.0374, 0.0241, 0.0107, 0.0080, 0.0214, 0.0053, 0.0561, 0.0080 };
        private readonly double[] maxRaceSizesF = { 0, 0.0856, 0.0722, 0.0829, 0.0722, 0.0802, 0.0267, 0.0080, 0.0374, 0.0535, 0.0561, 0.0080 };
        private int[] maxZoneSizes;
        private int[] maxRaceSizes;

        public GrowingGrid(ListBox listBox, int width, int height) {
            this.listBox = listBox;
            map = new GrowingGridTile[height, width];

            Init();
        }

        private void Init() {
            InitMaxSizes();
            SetStartZones();
            FindNearestZoneNeighbor();
            /*
            Grow();
            FillGaps();
            AddRemainingZones();
            ShowInfos();
            */
        }

        /// <summary>
        /// Berechnet die maximalen Größen von Rassen und Zonen,
        /// mit der Größe der Universum-Karte.
        /// </summary>
        private void InitMaxSizes() {
            maxZoneSizes = new int[ALLRACES];
            maxRaceSizes = new int[ALLRACES];

            for (int i = 0; i < ALLRACES; i++) {
                maxZoneSizes[i] = (int)Math.Round(maxZoneSizesF[i] * map.Length, 1);
                maxRaceSizes[i] = (int)Math.Round(maxRaceSizesF[i] * map.Length, 1);
            }
        }

        /// <summary>
        /// Setzt die ersten Zonen der Rassen mit eine bestimmten Abstand zueinandner
        /// </summary>
        private void SetStartZones() {
            List<GrowingGridTile> spaceListForAllDistance = new List<GrowingGridTile>();

            // Füllt die Map und die Liste mit "Space"
            for (int y = 0; y < map.GetLength(0); y++) {
                for (int x = 0; x < map.GetLength(1); x++) {
                    map[y, x] = new GrowingGridTile(0, x, y);
                    spaceListForAllDistance.Add(map[y, x]);
                }
            }

            // Geht alle Rassen durch
            for (int race = 0; race < ALLRACES; race++) {
                // Die Zonen dieser Rassen sollen nicht gesetzt werden.
                // Dennoch soll eine leere Liste als Lückenfüller für eine korrekte
                // Durchzählung der jeweiligen Rasse hinzugefügt werden.
                switch (race) {
                    case 0: // Space
                    //case 7: // Khaak
                    case 9: // Unbekannt
                    //case 11: // Yaki
                        raceListForInfos.Add(new List<GrowingGridTile>());
                        raceList.Add(new List<GrowingGridTile>());
                        continue;
                }
                
                List<GrowingGridTile> zoneList = new List<GrowingGridTile>();
                List<GrowingGridTile> zoneListForInfos = new List<GrowingGridTile>();
                List<GrowingGridTile> spaceListForZoneDistance = new List<GrowingGridTile>(spaceListForAllDistance);

                // Geht alle Zonen einer Rasse durch
                for (int zone = 0; zone < maxRaceZones[race]; zone++) {
                    int rndnumber = XMath.RandomNumber(0, spaceListForZoneDistance.Count);

                    GrowingGridTile rndZoneSpace = spaceListForZoneDistance[rndnumber];

                    map[rndZoneSpace.Y, rndZoneSpace.X].NewZone(race);
                    raceSectorCount[race]++;

                    FillFreeSpacesList(map[rndZoneSpace.Y, rndZoneSpace.X]);
                    zoneListForInfos.Add(map[rndZoneSpace.Y, rndZoneSpace.X]);
                    zoneList.Add(map[rndZoneSpace.Y, rndZoneSpace.X]);

                    if (maxRaceZones[race] > 1) {
                        RemoveSpacesFromSpaceListForZoneDistance(spaceListForZoneDistance, race, rndZoneSpace.Y, rndZoneSpace.X);
                    }
                    RemoveSpacesFromSpaceListForAllDistance(spaceListForAllDistance, rndZoneSpace.Y, rndZoneSpace.X);
                }

                raceListForInfos.Add(zoneListForInfos);
                raceList.Add(zoneList);
            }
        }

        /// <summary>
        /// Der Abstand zu den anderen Zonen der selben Rasse.
        /// Entfernt die leeren Plätze aus der Liste,
        /// die sich in einem bestimmten Radius dieser Zone befinden.
        /// </summary>
        /// <param name="spaceListForZoneDistance">Die Liste für den Abstand der Zonen, der selben Rasse</param>
        /// <param name="race">Die aktuelle Rasse der Schleife</param>
        /// <param name="rndY">Die Y-Position der gewählten Zone</param>
        /// <param name="rndX">Die X-Position der gewählten Zone</param>
        private void RemoveSpacesFromSpaceListForZoneDistance(List<GrowingGridTile> spaceListForZoneDistance, int race, int rndY, int rndX) {
            const double BASEDISTANCE = 13.5d;
            for (int i = 0; i < spaceListForZoneDistance.Count; i++) {
                double distance = XMath.DistanceOfTwoPoints2D(rndX, rndY, spaceListForZoneDistance[i].X, spaceListForZoneDistance[i].Y);
                double minDistance = Math.Round((double)((double)2 / maxRaceZones[race] * BASEDISTANCE), 1);

                if (distance < minDistance) {
                    spaceListForZoneDistance.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// Der Abstand zu allen anderen Zonen.
        /// Entfernt die leeren Plätze aus der Liste,
        /// die sich in einem bestimmten Radius der Zone befinden.
        /// </summary>
        /// <param name="spaceListForAllDistance">Die Liste für den Abstand aller Zonen</param>
        /// <param name="rndY">Die Y-Position der gewählten Zone</param>
        /// <param name="rndX">Die X-Position der gewählten Zone</param>
        private void RemoveSpacesFromSpaceListForAllDistance(List<GrowingGridTile> spaceListForAllDistance, int rndY, int rndX) {
            for (int i = 0; i < spaceListForAllDistance.Count; i++) {
                double distance = XMath.DistanceOfTwoPoints2D(rndX, rndY, spaceListForAllDistance[i].X, spaceListForAllDistance[i].Y);
                double minDistance = 3d;

                if (distance < minDistance) {
                    spaceListForAllDistance.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// Sucht die Nachbarn einer Zone, die sich in einem bestimmten Radius befinden
        /// und fügt diese in die NearestNeighbors Liste der Zone hinzu.
        /// </summary>
        private void FindNearestZoneNeighbor() {
            for (int raceA = 1; raceA < raceList.Count; raceA++) {
                if (raceA == 7) {
                    continue;
                }
                for (int zoneA = 0; zoneA < raceList[raceA].Count; zoneA++) {
                    List<double> distances = new List<double>();

                    for (int raceB = 1; raceB < raceList.Count; raceB++) {
                        if (raceB == 7) {
                            continue;
                        }
                        if (raceB != raceA) {
                            for (int zoneB = 0; zoneB < raceList[raceB].Count; zoneB++) {
                                double distance = XMath.DistanceOfTwoPoints2D(raceList[raceA][zoneA].X, raceList[raceA][zoneA].Y, raceList[raceB][zoneB].X, raceList[raceB][zoneB].Y);
                                double minDistance = 8;

                                if (distance < minDistance) {
                                    raceList[raceA][zoneA].NearestZoneNeighbors.Add(raceList[raceB][zoneB]);
                                    distances.Add(distance);
                                }
                            }
                        }
                    }

                    double temp;
                    GrowingGridTile tempNeighbor;
                    
                    // Sortiere die "NearesetNeighbors" Liste nach dem geringsten Abstand
                    for (int i = 0; i < raceList[raceA][zoneA].NearestZoneNeighbors.Count - 1; i++) {
                        for (int j = i + 1; j < raceList[raceA][zoneA].NearestZoneNeighbors.Count; j++) {
                            if (distances[i] > distances[j]) {

                                temp = distances[i];
                                tempNeighbor = raceList[raceA][zoneA].NearestZoneNeighbors[i];

                                distances[i] = distances[j];
                                raceList[raceA][zoneA].NearestZoneNeighbors[i] = raceList[raceA][zoneA].NearestZoneNeighbors[j];

                                distances[j] = temp;
                                raceList[raceA][zoneA].NearestZoneNeighbors[j] = tempNeighbor;
                            }
                        }
                    }                    
                }
            }
        }

        /// <summary>
        /// Zufällige Zonen breiten sich so lange um ein Tile aus,
        /// bis die raceList leer ist.
        /// </summary>
        private void Grow() {
            for (int race = 0; race < raceList.Count; race++) {
                // Hat die Rasse keine ausbreitbaren Zonen, entferne die Rasse aus der Liste
                if (raceList[race].Count == 0) {
                    raceList.RemoveAt(race);
                    race--;
                    continue;
                }                    

                GrowingGridTile zone = GetZoneFromRace(race);
                GrowingGridTile sector = GetSectorFromZone(zone);
                GrowingGridTile newSector = GetNewSectorFromSectorNeighbors(zone, sector);

                map[newSector.Y, newSector.X].GrowZone(zone);
                raceSectorCount[newSector.Race]++;
                FillFreeSpacesList(newSector);
                RemoveNearestTouchingNeighborZone(newSector);

                if (newSector.FreeSpaces.Count > 0) {
                    newSector.AddAsGrowableSectorToZone();
                }

                // Befinden sich in der Liste des neuen Sektors,
                // Sektoren die ihn als leeren Platz sahen (sectorClaimer)
                if (newSector.SectorsTryToClaimMe.Count > 0) {
                    for (int i = 0; i < newSector.SectorsTryToClaimMe.Count; i++) {
                        // Entferne den neuen Sektor aus der Liste der "sectorClaimer"
                        GrowingGridTile sectorClaimer = newSector.SectorsTryToClaimMe[i];
                        sectorClaimer.RemoveSectorAsFreeSpace(newSector);

                        int raceIndex = GetIndexOfZoneList(sectorClaimer, newSector, race);

                        // Wenn die Liste den "sectorClaimer" nicht hatte
                        if (raceIndex == -1) {
                            continue;
                        }

                        newSector.SectorsTryToClaimMe.RemoveAt(i);
                        i--;

                        // Wenn der Sektor keine freien Plätze mehr hat
                        if (sectorClaimer.FreeSpaces.Count == 0) {
                            sectorClaimer.Parent.GrowableSectors.Remove(sectorClaimer);

                            // Wenn die Zone keine ausbreitbaren Sektoren mehr hat
                            if (sectorClaimer.Parent.GrowableSectors.Count == 0) {
                                raceList[raceIndex].Remove(sectorClaimer.Parent);

                                // Wenn die Rasse keine ausbreitbaren Zonen mehr hat
                                if (raceList[raceIndex].Count == 0 && raceIndex != race) {
                                    raceList.RemoveAt(raceIndex);
                                }
                            }
                        }
                    }
                }

                // Wenn die Zone keine ausbreitbaren Sektoren mehr hat oder ihre maximal Zonen-Größe erreicht hat
                if (newSector.Parent.GrowableSectors.Count == 0 || newSector.Parent.ZoneSize == maxZoneSizes[newSector.Race]) {
                    raceList[race].Remove(newSector.Parent);
                }

                // Wenn die Rasse keine ausbreitbaren Zonen mehr hat oder die maximale Anzahl an Sektoren erreicht ist
                if (raceList[race].Count == 0 || raceSectorCount[newSector.Race] >= maxRaceSizes[newSector.Race]) {
                    raceList.RemoveAt(race);
                    race--;
                }
            }
        }

        /// <summary>
        /// Gibt eine zufällige Zone wieder, die 
        /// sich in der Liste der raceList befindet
        /// </summary>
        /// <param name="race">Die aktuelle Rasse</param>
        /// <returns></returns>
        private GrowingGridTile GetZoneFromRace(int race) {
            int zone = 0;

            // Wenn die Rasse mehr wie 1 ausbreitbare Zone 
            // in der Liste hat, wähle eine Zufällige Zone
            if (raceList[race].Count > 1) {
                zone = XMath.RandomNumber(0, raceList[race].Count);
            }

            return raceList[race][zone];
        }

        /// <summary>
        /// Gibt den Sektor wieder, der sich am Nähesten zum nächsten Zonen Nachbarn befindet
        /// oder wenn es keine Nachbarn mehr gibt, einen zufälligen Sektor aus der Liste der Zone
        /// </summary>
        /// <param name="zone">Die zufällige Zone</param>
        /// <returns></returns>
        private GrowingGridTile GetSectorFromZone(GrowingGridTile zone) {
            int sector = 0;

            // Wenn die Zone mehr wie 1 ausbreitbaren Sektor in der Liste hat
            if (zone.GrowableSectors.Count > 1) {
                // Wenn die Zone noch nahe Nachbarn in seiner Liste, dann wähle den
                // ausbreitbaren Sektor, der den geringsten Abstand zum nahen Nachbarn hat.
                if (zone.NearestZoneNeighbors.Count > 0) {
                    sector = GetSectorWithClostestDistanceToNearestNeighbor(zone.GrowableSectors, zone.NearestZoneNeighbors[0]);
                }
                // Ansonsten wähle einen Zufälligen Sektor
                else {
                    sector = XMath.RandomNumber(0, zone.GrowableSectors.Count);
                }
            }

            return zone.GrowableSectors[sector];
        }

        /// <summary>
        /// Gibt den freien Platz wieder, der sich am Nähesten zum nächsten Zonen Nachbarn befindet
        /// oder wenn es keine Nachbarn mehr gibt, einen zufällige freien Platz aus der Liste des Sektors
        /// </summary>
        /// <param name="zone">Die zufällige Zone</param>
        /// <param name="sector">Der zufällige Sektor</param>
        /// <returns></returns>
        private GrowingGridTile GetNewSectorFromSectorNeighbors(GrowingGridTile zone, GrowingGridTile sector) {
            int freeSpace = 0;

            // Wenn der Sektor mehr wie 1 freien Platz in der Liste hat
            if (sector.FreeSpaces.Count > 1) {
                // Wenn die Zone noch nahe Nachbarn in seiner Liste, dann wähle den
                // freien Platz, der den geringsten Abstand zum nahen Nachbarn hat.
                if (zone.NearestZoneNeighbors.Count > 0) {
                    freeSpace = GetSectorWithClostestDistanceToNearestNeighbor(sector.FreeSpaces, zone.NearestZoneNeighbors[0]);
                }
                // Ansonsten wähle einen Zufälligen Platz
                else {
                    freeSpace = XMath.RandomNumber(0, sector.FreeSpaces.Count);
                }
            }

            return sector.FreeSpaces[freeSpace];
        }

        /// <summary>
        /// Erhalte den Space Sector, mit dem geringsten Abstand zum nahen Nachbarn
        /// </summary>
        /// <param name="growableSectors">Die möglichen Sektoren</param>
        /// <param name="nearestNeighbor">Der nahe Nachbar</param>
        /// <returns>Der Sektor mit den geringsten Abstand zum nahen Nachbarn</returns>
        private int GetSectorWithClostestDistanceToNearestNeighbor(List<GrowingGridTile> growableSectors, GrowingGridTile nearestNeighbor) {
            double value = 10;
            int neighbor = 0;

            for (int i = 0; i < growableSectors.Count; i++) {
                double distance = XMath.DistanceOfTwoPoints2D(nearestNeighbor.X, nearestNeighbor.Y, growableSectors[i].X, growableSectors[i].Y);

                if (distance < value) {
                    value = distance;
                    neighbor = i;
                }
            }

            return neighbor;
        }

        /// <summary>
        /// Füllt die "FreeSpaces"-Liste des neuen
        /// Sektors mit seinen umliegenden freien Plätzen
        /// und überprüft diese nach ihren umliegenden Nachbarn,
        /// damit keine Zonen der selben Rasse miteinander
        /// verschmelzen können.
        /// </summary>
        /// <param name="newSector">Der neue Sektor</param>
        private void FillFreeSpacesList(GrowingGridTile newSector) {
            if (newSector.Y > 0) {
                GrowingGridTile top = map[newSector.Y - 1, newSector.X];

                if (top.Race == 0) {
                    AddFreeSpacesToList(newSector, top);
                }
            }
            if (newSector.Y < map.GetLength(0) - 1) {
                GrowingGridTile bottom = map[newSector.Y + 1, newSector.X];

                if (bottom.Race == 0) {
                    AddFreeSpacesToList(newSector, bottom);
                }
            }
            if (newSector.X > 0) {
                GrowingGridTile left = map[newSector.Y, newSector.X - 1];

                if (left.Race == 0) {
                    AddFreeSpacesToList(newSector, left);
                }
            }
            if (newSector.X < map.GetLength(1) - 1) {
                GrowingGridTile right = map[newSector.Y, newSector.X + 1];

                if (right.Race == 0) {
                    AddFreeSpacesToList(newSector, right);
                }
            }
        }

        private void AddFreeSpacesToList(GrowingGridTile newSector, GrowingGridTile neighbor) {
            if (CheckNextNeighborsForSameZoneRace(newSector, neighbor)) {
                newSector.AddFreeSpacesAndSectorClaimer(neighbor);
            }
        }

        /// <summary>
        /// Überprüft die Nachbarn um zu verhindern, dass sie die
        /// selbe Rasse haben und in unterschiedlichen Zonen sind
        /// </summary>
        /// <param name="newSector"></param>
        /// <param name="neighbor"></param>
        /// <returns></returns>
        private bool CheckNextNeighborsForSameZoneRace(GrowingGridTile newSector ,GrowingGridTile neighbor) {
            if (neighbor.Y > 0) {
                GrowingGridTile top = map[neighbor.Y - 1, neighbor.X];

                if (!(top.Race == newSector.Race && top.Parent != newSector.Parent)) {
                    return true;
                }
            }
            if (neighbor.Y < map.GetLength(0) - 1) {
                GrowingGridTile bottom = map[neighbor.Y + 1, neighbor.X];

                if (!(bottom.Race == newSector.Race && bottom.Parent != newSector.Parent)) {
                    return true;
                }
            }
            if (neighbor.X > 0) {
                GrowingGridTile left = map[neighbor.Y, neighbor.X - 1];

                if (!(left.Race == newSector.Race && left.Parent != newSector.Parent)) {
                    return true;
                }
            }
            if (neighbor.X < map.GetLength(1) - 1) {
                GrowingGridTile right = map[neighbor.Y, neighbor.X + 1];

                if (!(right.Race == newSector.Race && right.Parent != newSector.Parent)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Überprüft, ob sich der neue Sektor mit einen der Sektoren seiner Nachbar Zone
        /// berührt. Bei Berührung entfernen sich die Zonen gegenseitig aus ihrer NearestZoneNeighbors-Liste
        /// und fügen sich dann gegenseitig zu ihrer RaceNeighbors-Liste hinzu.
        /// </summary>
        /// <param name="newSector"></param>
        private void RemoveNearestTouchingNeighborZone(GrowingGridTile newSector) {
            if (newSector.Race != 7) {
                if (newSector.Y > 0) {
                    GrowingGridTile top = map[newSector.Y - 1, newSector.X];
                    if (top.Race != 0 && top.Race != newSector.Race) {
                        newSector.AddAndRemoveNearsetNeighbor(top.Parent, (int)GateDirection.North);
                    }
                    else if (top.Parent == newSector.Parent) {
                        newSector.ConnectSectors(top.Parent, (int)GateDirection.North);
                    }
                }
                if (newSector.Y < map.GetLength(0) - 1) {
                    GrowingGridTile bottom = map[newSector.Y + 1, newSector.X];
                    if (bottom.Race != 0 && bottom.Race != newSector.Race) {
                        newSector.AddAndRemoveNearsetNeighbor(bottom.Parent, (int)GateDirection.South);
                    }
                    else if (bottom.Parent == newSector.Parent) {
                        newSector.ConnectSectors(bottom.Parent, (int)GateDirection.South);
                    }
                }
                if (newSector.X > 0) {
                    GrowingGridTile left = map[newSector.Y, newSector.X - 1];
                    if (left.Race != 0 && left.Race != newSector.Race) {
                        newSector.AddAndRemoveNearsetNeighbor(left.Parent, (int)GateDirection.West);
                    }
                    else if (left.Parent == newSector.Parent) {
                        newSector.ConnectSectors(left.Parent, (int)GateDirection.West);
                    }
                }
                if (newSector.X < map.GetLength(1) - 1) {
                    GrowingGridTile right = map[newSector.Y, newSector.X + 1];
                    if (right.Race != 0 && right.Race != newSector.Race) {
                        newSector.AddAndRemoveNearsetNeighbor(right.Parent, (int)GateDirection.East);
                    }
                    else if (right.Parent == newSector.Parent) {
                        newSector.ConnectSectors(right.Parent, (int)GateDirection.East);
                    }
                }
            }
        }

        /// <summary>
        /// Erhalte den Index der raceList, in dessen Liste sich der sectorClaimer befindet
        /// oder race, wenn die Rasse von sectorCalimer und newSector gleich waren
        /// </summary>
        /// <param name="sectorClaimer"></param>
        /// <param name="newSector"></param>
        /// <param name="race"></param>
        /// <returns>Den Index der Liste oder -1, wenn sectorClaimer nicht gefunden wurde</returns>
        private int GetIndexOfZoneList(GrowingGridTile sectorClaimer, GrowingGridTile newSector, int race) {
            int raceIndex = -1;

            if (sectorClaimer.Race != newSector.Race) {
                foreach (List<GrowingGridTile> zoneList in raceList) {
                    // Wenn der "sectorClaimer" sich in der "zoneList" befindet
                    if (zoneList.Contains(sectorClaimer.Parent)) {
                        raceIndex = raceList.IndexOf(zoneList);
                        break;
                    }
                }
            }
            else {
                raceIndex = race;
            }

            return raceIndex;
        }

        ///////////////////////////////////////////////////////////////////////////////
        ///// TODO Eventuell lieber den Abstand zweier Zonen überprüfen, die sich /////
        ///// zuletzt versuchten zu erreichen                                     /////
        ///////////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// Füllt die Lücke zwischen zwei Rassen
        /// </summary>
        private void FillGaps() {
            spaceList = new List<GrowingGridTile>();

            for (int y = 0; y < map.GetLength(0); y++) {
                for (int x = 0; x < map.GetLength(1); x++) {
                    if (map[y, x].Race == 0) {
                        spaceList.Add(map[y, x]);
                    }
                }
            }

            List<int> fillers = new List<int>();
            int pirateSize = (int)Math.Round(0.0508 * map.Length, 1);
            int pirateSectors = pirateSize - raceSectorCount[8];

            for (int i = 0; i < pirateSectors; i++) {
                fillers.Add(8);
            }

            for (int i = 0; i < maxRaceSizes[9]; i++) {
                fillers.Add(9);
            }

            int rndNumber;

            //////////////////////////////////////////////////////////////////////////////
            ///// TODO Piraten-Filler dürfen nicht an andere Piraten-Zonen enstehen  /////
            ///// Wenn bereits die Rassen verbunden sind, fülle nicht                /////
            //////////////////////////////////////////////////////////////////////////////

            for (int i = 0; i < spaceList.Count; i++) {
                if (spaceList[i].Y > 1 && spaceList[i].Y < map.GetLength(0) - 2 && spaceList[i].X > 1 && spaceList[i].X < map.GetLength(1) - 2) {
                    if (IsThereAGap(i)) {
                        rndNumber = XMath.RandomNumber(0, fillers.Count);
                        map[spaceList[i].Y, spaceList[i].X].NewZone(fillers[rndNumber]);
                        AddFillerAsNeighborZone(map[spaceList[i].Y, spaceList[i].X]);
                        raceListForInfos[fillers[rndNumber]].Add(map[spaceList[i].Y, spaceList[i].X]);
                        fillers.RemoveAt(rndNumber);
                        spaceList.RemoveAt(i);
                        i--;
                    }                    

                    if (fillers.Count == 0) {
                        break;
                    }
                }
            }

            remainingZonesList = new List<int>(fillers);
        }

        /// <summary>
        /// Überprüft, ob sich eine Lücke zwischen zwei Rassen befindet.
        /// </summary>
        /// <param name="i">Der Zähler der Schleife</param>
        /// <returns></returns>
        private bool IsThereAGap(int i) {
            int[] races = { 0, 7 };
            int top = map[spaceList[i].Y - 1, spaceList[i].X].Race;
            int bottom = map[spaceList[i].Y + 1, spaceList[i].X].Race;
            int left = map[spaceList[i].Y, spaceList[i].X - 1].Race;
            int right = map[spaceList[i].Y, spaceList[i].X + 1].Race;
            int topTop = map[spaceList[i].Y - 2, spaceList[i].X].Race;
            int bottomBottom = map[spaceList[i].Y + 2, spaceList[i].X].Race;
            int leftLeft = map[spaceList[i].Y, spaceList[i].X - 2].Race;
            int rightRight = map[spaceList[i].Y, spaceList[i].X + 2].Race;
            // Sind die vertikalen Nachbarn Space und die horizontalen Nachbarn verschiedene Rassen oder umgekehrt
            if ((races.Contains(top) && races.Contains(topTop) && races.Contains(bottom) && races.Contains(bottomBottom)) && (!races.Contains(left) && !races.Contains(right) && right != left) ||
                (races.Contains(left) && races.Contains(leftLeft) && races.Contains(right) && races.Contains(rightRight)) && (!races.Contains(top) && !races.Contains(bottom) && bottom != top)) {

                return true;
            }

            int topLeft = map[spaceList[i].Y - 1, spaceList[i].X - 1].Race;
            int topRight = map[spaceList[i].Y - 1, spaceList[i].X + 1].Race;
            int bottomLeft = map[spaceList[i].Y + 1, spaceList[i].X - 1].Race;
            int bottomRight = map[spaceList[i].Y + 1, spaceList[i].X + 1].Race;
            // Sind die Nachbarn oben, links und unten rechts Space und unten und rechts verschiedene Rassen usw.
            if ((races.Contains(top) && races.Contains(left) && races.Contains(bottomRight)) && (!races.Contains(bottom) && !races.Contains(right) && right != bottom) ||
                (races.Contains(top) && races.Contains(right) && races.Contains(bottomLeft)) && (!races.Contains(bottom) && !races.Contains(left) && left != bottom) ||
                (races.Contains(bottom) && races.Contains(left) && races.Contains(topRight)) && (!races.Contains(top) && !races.Contains(right) && right != top) ||
                (races.Contains(bottom) && races.Contains(right) && races.Contains(topLeft)) && (!races.Contains(top) && !races.Contains(left) && left != top)) {

                return true;
            }
            return false;
        }

        /// <summary>
        /// Geht jeden anliegenden Sektor durch
        /// </summary>
        /// <param name="fillerSector"></param>
        private void AddFillerAsNeighborZone(GrowingGridTile fillerSector) {
            if (fillerSector.Y > 0) {
                GrowingGridTile top = map[fillerSector.Y - 1, fillerSector.X];
                CheckFillerNeighborsAndAddToList(fillerSector, top);
            }
            if (fillerSector.Y < map.GetLength(0) - 1) {
                GrowingGridTile bottom = map[fillerSector.Y + 1, fillerSector.X];
                CheckFillerNeighborsAndAddToList(fillerSector, bottom);
            }
            if (fillerSector.X > 0) {
                GrowingGridTile left = map[fillerSector.Y, fillerSector.X - 1];
                CheckFillerNeighborsAndAddToList(fillerSector, left);
            }
            if (fillerSector.X < map.GetLength(1) - 1) {
                GrowingGridTile right = map[fillerSector.Y, fillerSector.X + 1];
                CheckFillerNeighborsAndAddToList(fillerSector, right);
            }
        }

        /// <summary>
        /// Überprüft ob der benachbarte Sektor zu einer Rasse gehört,
        /// die nicht die selbe ist, wie der zu füllende Sektor und
        /// lässt diese dann sich gegenseitig als Nachbarn hinzufügen
        /// </summary>
        /// <param name="fillerSector"></param>
        /// <param name="neighbor"></param>
        private void CheckFillerNeighborsAndAddToList(GrowingGridTile fillerSector, GrowingGridTile neighbor) {
            if (neighbor.Race != 0 && !fillerSector.RaceNeighbors.Contains(neighbor.Parent)) {
                fillerSector.AddRaceNeighborsAndSectorsOfAdjacentZoneNeighbor(neighbor);
            }
        }

        /// <summary>
        /// Fügt die noch verbleibenden Zonen hinzu
        /// </summary>
        private void AddRemainingZones() {
            int rndNumber;
            GrowingGridTile rndSpace;

            /////////////////////////////////////////////////////////////////////////////////
            ///// TODO die Schleife solange durchlaufen, bis eines der Listen leer ist. /////
            ///// Überprüfen, ob der zufällige freie Platz mit einer bestimmten Rasse   /////
            ///// benachbart ist. Wenn nicht, den freien Platz aus der Liste entfernen. /////
            /////////////////////////////////////////////////////////////////////////////////

            //while (remainingZonesList.Count != 0 || spaceList.Count != 0)
            for (int i = 0; i < remainingZonesList.Count; i++) {
                rndNumber = XMath.RandomNumber(0, spaceList.Count);
                rndSpace = spaceList[rndNumber];

                if (rndSpace.Y > 1 && rndSpace.Y < map.GetLength(0) - 2 && rndSpace.X > 1 && rndSpace.X < map.GetLength(1) - 2) {
                    if (CheckRaceNeighbor(rndSpace)) {
                        map[rndSpace.Y, rndSpace.X].NewZone(remainingZonesList[i]);
                        AddFillerAsNeighborZone(map[rndSpace.Y, rndSpace.X]);
                        raceListForInfos[remainingZonesList[i]].Add(map[rndSpace.Y, rndSpace.X]);
                        remainingZonesList.RemoveAt(i);
                        spaceList.RemoveAt(rndNumber);
                        i--;
                    }
                }
            }
        }

        /// <summary>
        /// überprüft ob der freie Platz eine bestimmte Rasse als Nachbarn hat
        /// </summary>
        /// <param name="space"></param>
        /// <returns></returns>
        private bool CheckRaceNeighbor(GrowingGridTile space) {
            int[] races = { 0, 7, 8, 9 };
            if (space.Y > 0) {
                GrowingGridTile top = map[space.Y - 1, space.X];

                if (!races.Contains(top.Race)) {
                    return true;
                }
            }
            if (space.Y < map.GetLength(0) - 1) {
                GrowingGridTile bottom = map[space.Y + 1, space.X];

                if (!races.Contains(bottom.Race)) {
                    return true;
                }
            }
            if (space.X > 0) {
                GrowingGridTile left = map[space.Y, space.X - 1];

                if (!races.Contains(left.Race)) {
                    return true;
                }
            }
            if (space.X < map.GetLength(1) - 1) {
                GrowingGridTile right = map[space.Y, space.X + 1];

                if (!races.Contains(right.Race)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Zeigt Infos zu der generierten Universum-Karte
        /// </summary>
        private void ShowInfos() {
            int occupiedSectors = 0;

            for (int race = 1; race < raceListForInfos.Count; race++) {
                int raceSectorsCount = 0;
                string raceNeighbors;

                for (int zone = 0; zone < raceListForInfos[race].Count; zone++) {
                    raceSectorsCount += raceListForInfos[race][zone].ZoneSize;
                }

                occupiedSectors += raceSectorsCount;
                ListBoxItem listBoxItem = new ListBoxItem {
                    Content = "[" + race + "] " + MainWindow.raceByNumber[(byte)race] + ": " + raceSectorsCount + " Sectors, " + raceListForInfos[race].Count + " Zones",
                    Background = MainWindow.raceColors[(byte)race]
                };
                listBox.Items.Add(listBoxItem);

                for (int zone = 0; zone < raceListForInfos[race].Count; zone++) {
                    raceNeighbors = "";
                    for (int i = 0; i < raceListForInfos[race][zone].RaceNeighbors.Count; i++) {
                        raceNeighbors += raceListForInfos[race][zone].RaceNeighbors[i].Race + " ";
                    }
                    listBox.Items.Add("    Zone " + zone + ": " + raceListForInfos[race][zone].ZoneSize + " - N: " + raceNeighbors);
                }

                listBox.Items.Add("");
            }

            listBox.Items.Add("GS: " + map.Length + ", BS: " + occupiedSectors + ", Übrig: " + (map.Length - occupiedSectors));
        }

        public void StepGrow() {
            if (raceList.Count != 0) {
                Grow();
            }
        }
    
        public void FullGrow() {
            while (raceList.Count != 0) {
                Grow();
                FillGaps();
                ShowInfos();
            }
        }
    }
}