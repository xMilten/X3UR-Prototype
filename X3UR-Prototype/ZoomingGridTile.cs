using System.Collections.Generic;

namespace X3UR_Prototype {
    class ZoomingGridTile {
        public byte race;
        private ZoomingGridTile parent;
        public List<ZoomingGridTile> sectorsInZone;
        public byte CurrentZoneSize { get => (byte)parent.sectorsInZone.Count; }
        public ZoomingGridTile Parent { get => parent; }

        public ZoomingGridTile(params ZoomingGridTile[] neighbor) {
            race = neighbor[0].race;
            parent = neighbor[0].parent;
            parent.AddSector(this);

            if (neighbor.Length > 0) {
                for (int i = 1; i < neighbor.Length; i++) {
                    if (neighbor[i].parent != parent) {
                        neighbor[i].parent = parent;
                        parent.AddSector(neighbor[i]);

                        if (neighbor[i].sectorsInZone != null) {
                            foreach (ZoomingGridTile sector in neighbor[i].sectorsInZone) {
                                sector.parent = parent;
                                parent.AddSector(sector);
                            }
                            neighbor[i].sectorsInZone = null;
                        }
                    }
                }
            }
        }

        public ZoomingGridTile(byte race) {
            this.race = race;
            parent = this;
            sectorsInZone = new List<ZoomingGridTile> { this };
        }

        public void AddSector(ZoomingGridTile sector) {
            sectorsInZone.Add(sector);
        }
    }
}
