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

        public IEnumerable<WorldObject> GetAt(IEnumerable<Vector2Int> gridIndices, WorldComponentType componentType)
        {
            lock (_lockTarget) {
                List<WorldObject> items = new List<WorldObject>();
                foreach (var gridIndex in gridIndices) {
                    foreach (uint id in GetIdsAt(gridIndex)) {
                        WorldObject worldObject = Get(id);
                        if (worldObject.HasComponent(componentType)) {
                            items.Add(worldObject);
                        }
                    }
                }
                return items;
            }
        }

        public IEnumerable<WorldObject> GetAt(IEnumerable<Vector2Int> gridIndices, WorldObjectType componentType)
        {
            lock (_lockTarget) {
                List<WorldObject> items = new List<WorldObject>();
                foreach (var gridIndex in gridIndices) {
                    foreach (uint id in GetIdsAt(gridIndex)) {
                        WorldObject worldObject = Get(id);
                        if (worldObject.Type == componentType) {
                            items.Add(worldObject);
                        }
                    }
                }
                return items;
            }
        }

        public WorldObject GetFirstAt(Vector2Int gridIndex, WorldComponentType componentType)
        {
            lock (_lockTarget) {
                foreach (uint id in GetIdsAt(gridIndex)) {
                    WorldObject worldObject = Get(id);
                    if (worldObject.HasComponent(componentType)) {
                        return worldObject;
                    }
                }
                return default;
            }
        }

        public WorldObject GetFirstAt(Vector2Int gridIndex, WorldObjectType objectType)
        {
            lock (_lockTarget) {
                foreach (uint id in GetIdsAt(gridIndex)) {
                    WorldObject worldObject = Get(id);
                    if (worldObject.Type == objectType) {
                        return worldObject;
                    }
                }
                return default;
            }
        }

        public bool IsOccupied(Vector2Int gridIndex, WorldObjectType objectType, float worldTime)
        {
            lock (_lockTarget) {
                foreach (uint id in GetIdsAt(gridIndex)) {
                    WorldObject worldObject = Get(id);
                    if (worldObject.Type == objectType && gridIndex.Equals(worldObject.Location.GetAt(worldTime).GridIndex)) {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}