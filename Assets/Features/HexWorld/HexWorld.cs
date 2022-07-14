using System.Collections.Generic;
using UnityEngine;

namespace Zekzek.HexWorld
{
    public class HexWorld : PositionedCollection<WorldObject>
    {
        private static HexWorld instance;
        public static HexWorld Instance { get { if (instance == null) { instance = new HexWorld(); } return instance; } }

        private uint nextId = 1;
        public uint NextId { get { return nextId++; } }

        public bool Add(WorldObject worldObject) {
            return Add(worldObject.Id, worldObject.Location?.GridIndex, worldObject);
        }

        public IEnumerable<WorldObject> GetAll(WorldObjectType componentType)
        {
            List<WorldObject> items = new List<WorldObject>();
            lock (WorldUtil.SYNC_TARGET) {
                foreach (WorldObject worldObject in GetAll()) {
                    if (worldObject.Type == componentType) {
                        items.Add(worldObject);
                    }
                }
            }
            return items;
        }

        public IEnumerable<WorldObject> GetAt(Vector2Int gridIndex, WorldComponentType componentType, float worldTime = -1) { return GetAt(new[] { gridIndex }, componentType, worldTime); }

        public IEnumerable<WorldObject> GetAt(IEnumerable<Vector2Int> gridIndices, WorldComponentType componentType, float worldTime = -1)
        {
            if (worldTime < 0) { worldTime = WorldScheduler.Instance.Time; }
            List<WorldObject> items = new List<WorldObject>();
            lock (WorldUtil.SYNC_TARGET) {
                foreach (var gridIndex in gridIndices) {
                    foreach (uint id in GetIdsAt(gridIndex)) {
                        WorldObject worldObject = Get(id);
                        if (worldObject.HasComponent(componentType) && gridIndex.Equals(worldObject.Location.GetAt(worldTime).GridIndex)) {
                            items.Add(worldObject);
                        }
                    }
                }
            }
            return items;
        }

        public ICollection<WorldObject> GetAt(Vector2Int gridIndex, WorldObjectType objectType, float worldTime = -1) { return GetAt(new[] { gridIndex }, objectType, worldTime); }
        public ICollection<WorldObject> GetAt(IEnumerable<Vector2Int> gridIndices, WorldObjectType objectType, float worldTime = -1)
        {
            if (worldTime < 0) { worldTime = WorldScheduler.Instance.Time; }
            List<WorldObject> items = new List<WorldObject>();
            lock (WorldUtil.SYNC_TARGET) {
                foreach (Vector2Int gridIndex in gridIndices) {
                    bool tileFound = false;
                    foreach (uint id in GetIdsAt(gridIndex)) {
                        WorldObject worldObject = Get(id);
                        if (worldObject.Type == objectType && gridIndex.Equals(worldObject.Location.GetAt(worldTime).GridIndex)) {
                            items.Add(worldObject);
                        }
                        if (worldObject.Type == WorldObjectType.Tile) { tileFound = true; }
                    }
                    // There must always be a tile
                    if (!tileFound && objectType == WorldObjectType.Tile) {
                        items.Add(GenerationUtil.InstantiateTile(gridIndex.x, gridIndex.y));
                    }
                }
            }
            return items;
        }

        public WorldObject GetFirstAt(Vector2Int gridIndex, WorldComponentType componentType, float worldTime = -1)
        {
            if (worldTime < 0) { worldTime = WorldScheduler.Instance.Time; }
            lock (WorldUtil.SYNC_TARGET) {
                foreach (uint id in GetIdsAt(gridIndex)) {
                    WorldObject worldObject = Get(id);
                    if (worldObject.HasComponent(componentType) && gridIndex.Equals(worldObject.Location.GetAt(worldTime).GridIndex)) {
                        return worldObject;
                    }
                }
            }
            return default;
        }

        public WorldObject GetFirstAt(Vector2Int gridIndex, WorldObjectType objectType, float worldTime = -1)
        {
            if (worldTime < 0) { worldTime = WorldScheduler.Instance.Time; }
            lock (WorldUtil.SYNC_TARGET) {
                foreach (uint id in GetIdsAt(gridIndex)) {
                    WorldObject worldObject = Get(id);
                    if (worldObject.Type == objectType && gridIndex.Equals(worldObject.Location.GetAt(worldTime).GridIndex)) {
                        return worldObject;
                    }
                }
            }
            return default;
        }

        public bool IsOccupied(Vector2Int gridIndex, WorldObjectType objectType, float worldTime = -1)
        {
            if (worldTime < 0) { worldTime = WorldScheduler.Instance.Time; }
            lock (WorldUtil.SYNC_TARGET) {
                foreach (uint id in GetIdsAt(gridIndex)) {
                    WorldObject worldObject = Get(id);
                    if (worldObject.Type == objectType && gridIndex.Equals(worldObject.Location.GetAt(worldTime).GridIndex)) {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}