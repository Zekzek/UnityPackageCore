using System.Collections.Generic;
using UnityEngine;

namespace Zekzek.HexWorld
{
    public class World
    {
        private static World instance;
        public static World Instance { get { if (instance == null) { instance = new World(); } return instance; } }

        private uint nextId = 1;
        public uint NextId { get { return nextId++; } }

        public List<ulong> EncodedWorld {
            get {
                List<ulong> encodings = new List<ulong>();
                foreach (HexTile tile in tileMap.Values) { encodings.Add(tile.Encoding); }
                return encodings;
            }
            set {
                tileMap.Clear();
                foreach (ulong encodedTile in value) {
                    HexTile tile = new HexTile(encodedTile);
                    tileMap[tile.GridIndex] = tile;
                }
            }
        }

        // Containers
        private readonly IList<Vector2Int> relativeScreenIndices = new List<Vector2Int>();
        private readonly IDictionary<Vector2Int, HexTile> tileMap = new Dictionary<Vector2Int, HexTile>();
        private readonly IDictionary<uint, WorldObject> worldObjects = new Dictionary<uint, WorldObject>();

        // Categorized
        private readonly IDictionary<Vector2Int, ICollection<uint>> objectIdsBySector = new Dictionary<Vector2Int, ICollection<uint>>();
        private readonly IDictionary<string, IDictionary<uint, float>> allegienceByGroup = new Dictionary<string, IDictionary<uint, float>>();

        // Init
        public void SetScreenDimensions(int width, int height)
        {
            relativeScreenIndices.Clear();
            int halfWidth = width / 2;
            int halfHeight = height / 2;

            for (int y = -halfHeight; y <= halfHeight; y++) {
                for (int x = -halfWidth; x <= halfWidth; x++) {
                    relativeScreenIndices.Add(new Vector2Int(x - y / 2, y));
                }
            }
        }

        // Add
        public void Add(WorldObject item)
        {
            Add(item.Location);
        }
        public void Add(WorldLocation location)
        {
            if (location == null) { return; }
            location.OnSectorChange += UpdateSector;
            UpdateSector(Vector2Int.zero, location.Sector, location.WorldObjectId);
        }
        public void Add(HexTile tile)
        {
            if (tileMap.ContainsKey(tile.GridIndex)) { Debug.LogError(string.Format("Attempt to add duplicate HexTile at {0}", tile.GridIndex)); return; }
            tileMap[tile.GridIndex] = tile;
        }

        // Remove
        public void Remove(uint id)
        {
            if (worldObjects.ContainsKey(id)) {
                Remove(worldObjects[id]);
            }
        }
        public void Remove(WorldObject item) { worldObjects.Remove(item.Id); Remove(item.Location); }

        public void Remove(WorldLocation location)
        {
            if (location != null) {
                objectIdsBySector[location.Sector].Remove(location.WorldObjectId);
            }
        }

        public void ClearTiles()
        {
            tileMap.Clear();
        }

        public void ClearAll()
        {
            ClearTiles();
            worldObjects.Clear();
        }

        // Get
        public WorldObject GetObject(uint id)
        {
            if (worldObjects.ContainsKey(id)) {
                return worldObjects[id];
            }
            return null;
        }

        // On Screen
        public ICollection<Vector2Int> GetScreenIndicesAround(Vector2Int center)
        {
            List<Vector2Int> indices = new List<Vector2Int>();
            foreach (Vector2Int relativeIndex in relativeScreenIndices) { indices.Add(center + relativeIndex); }
            return indices;
        }
        public ICollection<HexTile> GetScreenTilesAround(Vector2Int center)
        {
            List<HexTile> tiles = new List<HexTile>();
            foreach (Vector2Int gridIndex in GetScreenIndicesAround(center)) {
                HexTile tile = TileAt(gridIndex);
                if (tile != null) { tiles.Add(tile); }
            }
            return tiles;
        }
        public ICollection<WorldObject> GetWorldObjectsAround(Vector2Int center)
        {
            ICollection<Vector2Int> gridIndices = GetScreenIndicesAround(center);
            List<WorldObject> foundObjects = new List<WorldObject>();
            foreach (WorldObject worldObject in worldObjects.Values) {
                if (gridIndices.Contains(WorldUtil.GridPosToGridIndex(worldObject.Location.GridPosition))) {
                    foundObjects.Add(worldObject);
                }
            }
            return foundObjects;
        }
        public ICollection<WorldObject> GetAllObjects()
        {
            List<WorldObject> foundObjects = new List<WorldObject>();
            foreach (WorldObject worldObject in worldObjects.Values) {
                foundObjects.Add(worldObject);
            }
            return foundObjects;
        }

        // At Position
        public HexTile TileAt(Vector2Int gridIndex) { return tileMap.ContainsKey(gridIndex) ? tileMap[gridIndex] : null; }
        public HexTile TileAt(int x, int y) { return TileAt(new Vector2Int(x, y)); }
        public HexTile TileAt(Vector3Int gridPos) { return TileAt(WorldUtil.GridPosToGridIndex(gridPos)); }

        public ICollection<WorldObject> GetSectorObjects(Vector2Int sector)
        {
            List<WorldObject> objects = new List<WorldObject>();
            foreach (WorldObject worldObject in worldObjects.Values) { objects.Add(worldObject); }
            if (objectIdsBySector.ContainsKey(sector)) { foreach (uint id in objectIdsBySector[sector]) { objects.Add(GetObject(id)); } }
            return objects;
        }

        private void UpdateSector(Vector2Int originalSector, Vector2Int updatedSector, uint id)
        {
            //if (objectIdsBySector.ContainsKey(originalSector)) { objectIdsBySector[originalSector]?.Remove(id); }
            //if (!objectIdsBySector.ContainsKey(updatedSector)) { objectIdsBySector.Add(updatedSector, new List<uint>()); }
            //objectIdsBySector[updatedSector].Add(id);
        }
        public void UpdateAllegience(string group, uint id, int amount)
        {
            if (!allegienceByGroup.ContainsKey(group)) { allegienceByGroup.Add(group, new Dictionary<uint, float>()); }
            allegienceByGroup[group][id] = amount;
        }
    }
}