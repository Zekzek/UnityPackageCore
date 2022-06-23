using System.Collections.Generic;

namespace Zekzek.HexWorld
{
    public class HexWorld
    {
        private static HexWorld instance;
        public static HexWorld Instance { get { if (instance == null) { instance = new HexWorld(); } return instance; } }

        private uint nextId = 1;
        public uint NextId { get { return nextId++; } }

        public List<ulong> EncodedWorld {
            get {
                List<ulong> encodings = new List<ulong>();
                foreach (HexTile tile in tiles.GetAllItems()) { encodings.Add(tile.Encoding); }
                return encodings;
            }
            set {
                tiles.Clear();
                foreach (ulong encodedTile in value) {
                    HexTile tile = new HexTile(encodedTile);
                    tiles.Add(tile.Id, tile.Location.GridIndex, tile);
                }
            }
        }

        // Containers
        public readonly PositionedCollection<HexTile> tiles = new PositionedCollection<HexTile>();
        public readonly PositionedCollection<WorldObject> worldObjects = new PositionedCollection<WorldObject>();
    }
}