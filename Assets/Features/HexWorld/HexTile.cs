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

        // Grid Index is fixed once created, but height and other details may change.
        public readonly Vector2Int GridIndex;
        private int height;
        public int Height {
            get => height;
            private set {
                height = value;
                GridPos = WorldUtil.GridIndexToGridPos(GridIndex, height);
                OnHeightChanged?.Invoke();
            }
        }
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
        public HexTile NE => ne ??= World.Instance.TileAt(GridIndex.x, GridIndex.y + 1);
        public HexTile E => e ??= World.Instance.TileAt(GridIndex.x + 1, GridIndex.y);
        public HexTile SE => se ??= World.Instance.TileAt(GridIndex.x + 1, GridIndex.y - 1);
        public HexTile SW => sw ??= World.Instance.TileAt(GridIndex.x, GridIndex.y - 1);
        public HexTile W => w ??= World.Instance.TileAt(GridIndex.x - 1, GridIndex.y);
        public HexTile NW => nw ??= World.Instance.TileAt(GridIndex.x - 1, GridIndex.y + 1);

        // Cached convenience variables
        public Vector3Int GridPos { get; private set; }
        public ulong Encoding {
            get {
                ulong encoding = 0;
                encoding = (encoding << 10) + (ulong)GridPos.x; //max 1024
                encoding = (encoding << 4) + (ulong)GridPos.y; //max 16
                encoding = (encoding << 10) + (ulong)GridPos.z; //max 1024
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
            //encoding >>= 10;

            GridIndex = new Vector2Int(gridPosX, gridPosZ);
            Height = gridPosY;
        }

        public HexTile(int x, int y, int z)
        {
            GridIndex = new Vector2Int(x, z);
            Height = y;
        }

        public void Raise()
        {
            if (Height < MAX_HEIGHT) { Height++; }
        }

        public void Lower()
        {
            if (Height > 0) { Height--; }
        }

        public override string ToString()
        {
            return string.Format("Grid Index:({0},{1}), Height:{2}", GridPos.x, GridPos.z, GridPos.y);
        }
    }
}