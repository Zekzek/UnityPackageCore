using System;
using UnityEngine;

namespace Zekzek.HexWorld
{
    public class HexTile
    {
        // Observables
        public Action OnHeightChanged;
        public Action OnHighlightChanged;

        // Define height and position limits for encoding (move to world?)
        public const int MAX_HEIGHT = (1 << 4) - 1;
        public const int MAX_POS = (1 << 10) - 1;

        private uint _id;
        public uint Id => _id;
        public readonly WorldLocation Location;

        private bool highlight;
        public bool Highlight {
            get => highlight;
            set {
                highlight = value;
                OnHighlightChanged?.Invoke();
            }
        }

        // Lazy load and cache neighbors. No need to update unless new tiles are created.
        private HexTile ne, e, se, sw, w, nw;
        public HexTile NE => ne ??= HexWorld.Instance.tiles.GetFirstItemAt(new Vector2Int(Location.GridIndex.x, Location.GridIndex.y + 1));
        public HexTile E => e ??= HexWorld.Instance.tiles.GetFirstItemAt(new Vector2Int(Location.GridIndex.x + 1, Location.GridIndex.y));
        public HexTile SE => se ??= HexWorld.Instance.tiles.GetFirstItemAt(new Vector2Int(Location.GridIndex.x + 1, Location.GridIndex.y - 1));
        public HexTile SW => sw ??= HexWorld.Instance.tiles.GetFirstItemAt(new Vector2Int(Location.GridIndex.x, Location.GridIndex.y - 1));
        public HexTile W => w ??= HexWorld.Instance.tiles.GetFirstItemAt(new Vector2Int(Location.GridIndex.x - 1, Location.GridIndex.y));
        public HexTile NW => nw ??= HexWorld.Instance.tiles.GetFirstItemAt(new Vector2Int(Location.GridIndex.x - 1, Location.GridIndex.y + 1));

        public ulong Encoding {
            get {
                ulong encoding = 0;
                encoding = (encoding << 10) + (ulong)Location.GridPosition.x; //max 1024
                encoding = (encoding << 4) + (ulong)Location.GridPosition.y; //max 16
                encoding = (encoding << 10) + (ulong)Location.GridPosition.z; //max 1024
                return encoding;
            }
        }

        public HexTile(ulong encoding)
        {
            int gridPosZ = (int)(encoding & ((1 << 10) - 1));
            encoding >>= 10;
            int gridPosY = (int)(encoding & ((1 << 4) - 1));
            encoding >>= 4;
            int gridPosX = (int)(encoding & ((1 << 10) - 1));
            encoding >>= 10;
            _id = (uint)encoding;

            Location = new WorldLocation(_id, WorldUtil.PositionToGridPos(new Vector3Int(gridPosX, gridPosY, gridPosZ)), 0);
        }

        public HexTile(int x, int y, int z)
        {
            _id = HexWorld.Instance.NextId;
            Location = new WorldLocation(_id, WorldUtil.PositionToGridPos(new Vector3Int(x, y, z)), 0);
        }

        public void Raise()
        {
            if (Location.GridPosition.y < MAX_HEIGHT) { Location.Position += Vector3.up; }
        }

        public void Lower()
        {
            if (Location.GridPosition.y > 0) { Location.Position -= Vector3.up; }
        }

        public override string ToString()
        {
            return string.Format("Grid Index:({0},{1}), Height:{2}", Location.GridPosition.x, Location.GridPosition.z, Location.GridPosition.y);
        }
    }
}