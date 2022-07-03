using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Zekzek.HexWorld
{
    public class PositionedCollection<T>
    {
        public int Count => items.Count;
        protected readonly IDictionary<uint, T> items = new Dictionary<uint, T>();
        protected readonly IDictionary<uint, Vector2Int> gridIndices = new Dictionary<uint, Vector2Int>();
        protected readonly IDictionary<Vector2Int, List<uint>> byGridIndex = new Dictionary<Vector2Int, List<uint>>();

        public bool Add(uint id, Vector2Int? gridIndex, T item)
        {
            if (Contains(id)) { return false; }

            InitGridIndex(gridIndex);
            items[id] = item;
            if (gridIndex.HasValue) {
                gridIndices[id] = gridIndex.Value;
                byGridIndex[gridIndex.Value].Add(id);
            }
            return true;
        }

        public bool Remove(uint id)
        {
            if (!Contains(id)) { return false; }

            if (gridIndices.ContainsKey(id)) {
                byGridIndex[gridIndices[id]].Remove(id);
                gridIndices.Remove(id);
            }
            items.Remove(id);
            return true;
        }

        public bool UpdatePosition(uint id, Vector2Int? gridIndex)
        {
            if (!Contains(id)) { return false; }

            if (gridIndices.ContainsKey(id)) {
                byGridIndex[gridIndices[id]].Remove(id);
                gridIndices.Remove(id);
            }
            if (gridIndex.HasValue) {
                gridIndices.Add(id, gridIndex.Value);
                byGridIndex[gridIndex.Value].Add(id);
            }
            return true;
        }

        public void Clear()
        {
            items.Clear();
            gridIndices.Clear();
            byGridIndex.Clear();
        }

        public Vector2Int? GetPosition(uint id)
        {
            if (gridIndices.ContainsKey(id)) { return gridIndices[id]; }
            return null;
        }

        public T Get(uint id)
        {
            return items.ContainsKey(id) ? items[id] : default;
        }

        public IEnumerable<uint> GetIdsAt(Vector2Int gridIndex)
        {
            return byGridIndex.ContainsKey(gridIndex) ? byGridIndex[gridIndex] : Enumerable.Empty<uint>();
        }

        public IEnumerable<T> GetAt(IEnumerable<Vector2Int> gridIndices)
        {
            List<T> items = new List<T>();
            foreach (var gridIndex in gridIndices) {
                foreach (uint id in GetIdsAt(gridIndex)) {
                    items.Add(Get(id));
                }
            }
            return items;
        }

        public T GetFirstAt(Vector2Int gridIndex)
        {
            foreach (uint id in GetIdsAt(gridIndex)) {
                return Get(id);
            }
            return default;
        }

        public IEnumerable<T> GetAll()
        {
            return items.Values;
        }

        public bool Contains(uint id)
        {
            return items.ContainsKey(id);
        }

        public bool IsOccupied(Vector2Int gridIndex)
        {
            return byGridIndex.ContainsKey(gridIndex) && byGridIndex[gridIndex].Count > 0;
        }

        private void InitGridIndex(Vector2Int? gridIndex)
        {
            if (gridIndex.HasValue && !byGridIndex.ContainsKey(gridIndex.Value)) {
                byGridIndex.Add(gridIndex.Value, new List<uint> { });
            }
        }
    }
}