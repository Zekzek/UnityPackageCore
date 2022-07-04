using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Zekzek.HexWorld
{
    public class PositionedCollection<T>
    {
        public int Count => itemsById.Count;
        protected readonly object _lockTarget = new object();
        protected readonly IDictionary<uint, T> itemsById = new ConcurrentDictionary<uint, T>();
        protected readonly IDictionary<uint, List<Vector2Int>> positionsById = new ConcurrentDictionary<uint, List<Vector2Int>>();
        protected readonly IDictionary<Vector2Int, List<uint>> idsByPosition = new ConcurrentDictionary<Vector2Int, List<uint>>();

        public bool Add(uint id, Vector2Int? gridIndex, T item)
        {
            lock (_lockTarget) {
                if (Contains(id)) { return false; }

                InitCollection(id);
                InitCollection(gridIndex);
                itemsById[id] = item;
                if (gridIndex.HasValue) {
                    positionsById[id].Add(gridIndex.Value);
                    idsByPosition[gridIndex.Value].Add(id);
                }
                return true;
            }
        }

        public bool AddPositionToExistingItem(uint id, Vector2Int gridIndex)
        {
            lock (_lockTarget) {
                if (!Contains(id)) { return false; }

                InitCollection(gridIndex);
                positionsById[id].Add(gridIndex);
                idsByPosition[gridIndex].Add(id);
                return true;
            }
        }

        public bool Remove(uint id)
        {
            lock (_lockTarget) {
                if (!Contains(id)) { return false; }

                foreach (Vector2Int position in positionsById[id]) {
                    idsByPosition[position].Remove(id);
                }
                positionsById.Remove(id);
                itemsById.Remove(id);
                return true;
            }
        }

        public bool RemovePositionFromExistingItem(uint id, Vector2Int gridIndex)
        {
            lock (_lockTarget) {
                if (!Contains(id)) { return false; }

                idsByPosition[gridIndex].Remove(id);
                positionsById[id].Remove(gridIndex);
                return true;
            }
        }

        public void Clear()
        {
            lock (_lockTarget) {
                itemsById.Clear();
                positionsById.Clear();
                idsByPosition.Clear();
            }
        }

        public List<Vector2Int> GetPositionsFor(uint id)
        {
            lock (_lockTarget) {
                if (positionsById.ContainsKey(id)) { return new List<Vector2Int>(positionsById[id]); }
                return null;
            }
        }

        public T Get(uint id)
        {
            lock (_lockTarget) {
                return itemsById.ContainsKey(id) ? itemsById[id] : default;
            }
        }

        public IEnumerable<uint> GetIdsAt(Vector2Int gridIndex)
        {
            lock (_lockTarget) {
                return idsByPosition.ContainsKey(gridIndex) ? idsByPosition[gridIndex] : Enumerable.Empty<uint>();
            }
        }

        public IEnumerable<T> GetAt(IEnumerable<Vector2Int> gridIndices)
        {
            lock (_lockTarget) {
                List<T> items = new List<T>();
                foreach (var gridIndex in gridIndices) {
                    foreach (uint id in GetIdsAt(gridIndex)) {
                        items.Add(Get(id));
                    }
                }
                return items;
            }
        }

        public T GetFirstAt(Vector2Int gridIndex)
        {
            lock (_lockTarget) {
                foreach (uint id in GetIdsAt(gridIndex)) {
                    return Get(id);
                }
                return default;
            }
        }

        public IEnumerable<T> GetAll()
        {
            lock (_lockTarget) {
                return itemsById.Values;
            }
        }

        public bool Contains(uint id)
        {
            lock (_lockTarget) {
                return itemsById.ContainsKey(id);
            }
        }

        public bool IsOccupied(Vector2Int gridIndex)
        {
            lock (_lockTarget) {
                return idsByPosition.ContainsKey(gridIndex) && idsByPosition[gridIndex].Count > 0;
            }
        }

        private void InitCollection(uint id)
        {
            lock (_lockTarget) {
                if (!positionsById.ContainsKey(id)) {
                    positionsById.Add(id, new List<Vector2Int>());
                }
            }
        }

        private void InitCollection(Vector2Int? gridIndex)
        {
            lock (_lockTarget) {
                if (gridIndex.HasValue && !idsByPosition.ContainsKey(gridIndex.Value)) {
                    idsByPosition.Add(gridIndex.Value, new List<uint>());
                }
            }
        }

    }
}