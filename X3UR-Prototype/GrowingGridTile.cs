using System.Collections.Generic;
using System.Linq;

namespace X3UR_Prototype {
    class GrowingGridTile {
        private int race;
        private readonly int x;
        private readonly int y;
        private GrowingGridTile[] gates = new GrowingGridTile[4];
        private Dictionary<int, int> reverseDirection = new Dictionary<int, int>() { { 0, 1 }, { 1, 0 }, { 2, 3 }, { 3, 2 } };

        /////////////////////////
        /// TODO Zonen-Klasse ///
        /////////////////////////
        private GrowingGridTile parent;
        private List<GrowingGridTile> sectors;
        private List<GrowingGridTile> growableSectors;
        private readonly List<GrowingGridTile> freeSpaces = new List<GrowingGridTile>();
        private readonly List<GrowingGridTile> sectorsTryToClaimMe = new List<GrowingGridTile>();
        private readonly List<GrowingGridTile> nearestZoneNeighbors = new List<GrowingGridTile>();
        private readonly List<GrowingGridTile> raceNeighbors = new List<GrowingGridTile>();
        private readonly List<GrowingGridTile> sectorsOfAdjacentZoneNeighbor = new List<GrowingGridTile>();

        public int Race { get => race; }
        public int X { get => x; }
        public int Y { get => y; }
        public GrowingGridTile Parent { get => parent; }
        public int ZoneSize { get => parent.sectors.Count; }
        public List<GrowingGridTile> GrowableSectors { get => growableSectors; }
        public List<GrowingGridTile> FreeSpaces { get => freeSpaces; }
        public List<GrowingGridTile> SectorsTryToClaimMe { get => sectorsTryToClaimMe; }
        public List<GrowingGridTile> NearestZoneNeighbors { get => nearestZoneNeighbors; }
        public List<GrowingGridTile> RaceNeighbors { get => raceNeighbors; }

        public GrowingGridTile(int race, int x, int y) {
            this.race = race;
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Besetzt den Sektor mit einer neuen Zone
        /// </summary>
        /// <param name="race"></param>
        public void NewZone(int race) {
            this.race = race;
            parent = this;
            sectors = new List<GrowingGridTile> { this };
            growableSectors = new List<GrowingGridTile> { this };
        }

        /// <summary>
        /// Bestezt den Sektor und fügt ihn der Zone hinzu
        /// </summary>
        /// <param name="zone"></param>
        public void GrowZone(GrowingGridTile zone) {
            parent = zone;
            race = parent.race;
            parent.sectors.Add(this);
        }

        /// <summary>
        /// Fügt den Sektor als "growable" Sektor hinzu
        /// </summary>
        public void AddAsGrowableSectorToZone() {
            parent.GrowableSectors.Add(this);
        }

        /// <summary>
        /// Enternt den Sektor FreeSpaces
        /// </summary>
        /// <param name="newSector"></param>
        public void RemoveSectorAsFreeSpace(GrowingGridTile newSector) {
            freeSpaces.Remove(newSector);
        }

        /// <summary>
        /// Fügt den freien Platz in die Liste des Sktors hinzu
        /// und fügt den Sektor in die Liste des freien Platzes
        /// als "claimer" hinzu
        /// </summary>
        /// <param name="neighbor"></param>
        public void AddFreeSpacesAndSectorClaimer(GrowingGridTile neighbor) {
            freeSpaces.Add(neighbor);
            neighbor.sectorsTryToClaimMe.Add(this);
        }

        /// <summary>
        /// Bei Berührung entfernen sich die Zonen gegenseitig aus ihrer NearestZoneNeighbors-Liste
        /// und fügen sich dann gegenseitig zu ihrer RaceNeighbors-Liste hinzu.
        /// </summary>
        /// <param name="neighborZone">Die Zone (Parent) des Nachbarn</param>
        /// <param name="direction"></param>
        public void AddAndRemoveNearsetNeighbor(GrowingGridTile neighborZone, int direction) {
            if (!neighborZone.raceNeighbors.Contains(parent) && neighborZone.Race != 7) {
                neighborZone.raceNeighbors.Add(parent);
            }

            neighborZone.sectorsOfAdjacentZoneNeighbor.Add(parent);

            if (neighborZone.nearestZoneNeighbors.Any(neighbor => neighbor == parent)) {
                neighborZone.gates[reverseDirection[direction]] = parent;
                neighborZone.nearestZoneNeighbors.Remove(parent);
            }

            if (!parent.raceNeighbors.Contains(neighborZone) && neighborZone.Race != 7) {
                parent.raceNeighbors.Add(neighborZone);
            }

            parent.sectorsOfAdjacentZoneNeighbor.Add(neighborZone);

            if (parent.nearestZoneNeighbors.Any(neighbor => neighbor == neighborZone)) {
                parent.gates[direction] = neighborZone;
                parent.nearestZoneNeighbors.Remove(neighborZone);
            }
        }

        /// <summary>
        /// Lässt die Sektoren wissen, in welche Richtung
        /// ihre Verbindung zueinander geht
        /// </summary>
        /// <param name="neighbor"></param>
        /// <param name="direction"></param>
        public void ConnectSectors(GrowingGridTile neighbor, int direction) {
            parent.gates[direction] = neighbor;
            neighbor.gates[reverseDirection[direction]] = parent;
        }

        /// <summary>
        /// Dei Sektoren fügen sich gegenseitig als Nachbarn und als angrenzende NAchbarn hinzu
        /// </summary>
        /// <param name="neighbor"></param>
        public void AddRaceNeighborsAndSectorsOfAdjacentZoneNeighbor(GrowingGridTile neighbor) {
            raceNeighbors.Add(neighbor);
            sectorsOfAdjacentZoneNeighbor.Add(neighbor);
            neighbor.Parent.raceNeighbors.Add(this);
            neighbor.sectorsOfAdjacentZoneNeighbor.Add(this);
        }
    }
}