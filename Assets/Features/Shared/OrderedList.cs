using System;
using System.Collections.Generic;
using System.Text;

namespace Zekzek
{
    public class OrderedList<T>
    {
        private List<float> keys = new List<float>();
        private List<T> values = new List<T>();

        public int Count => keys.Count;
        public T First => Count > 0 ? values[0] : default;
        public T this[int key] => Count > key ? values[key] : default;

        //TODO: find a better thread-safe solution than these clumsy locks
        public void Add(float order, T value)
        {
            lock (this) {
                int insertIndex = BinarySearchInsertIndex(order, 0, keys.Count);
                keys.Insert(insertIndex, order);
                values.Insert(insertIndex, value);
            }
        }

        public void RemoveAt(int index)
        {
            lock (this) {
                keys.RemoveAt(index);
                values.RemoveAt(index);
            }
        }

        public bool Remove(T value) { return Remove(other => other.Equals(value)); }
        public bool Remove(Func<T, bool> compare)
        {
            lock (this) {
                for (int i = 0; i < values.Count; i++) {
                    if (compare(values[i])) {
                        RemoveAt(i);
                        return true;
                    }
                }
            }
            return false;
        }

        public bool TryGetAround(float target, out T before, out T after)
        {
            lock (this) {
                int insertIndex = BinarySearchInsertIndex(target, 0, keys.Count);
                after = insertIndex < values.Count ? values[insertIndex] : default;
                before = insertIndex > 0 ? values[insertIndex - 1] : default;
                return insertIndex > 0;
            }
        }

        public bool RemoveAll(T value) { return RemoveAll(other => other.Equals(value)); }
        public bool RemoveAll(Func<T, bool> compare)
        {
            bool matched = false;
            lock (this) {
                for (int i = values.Count - 1; i >= 0; i--) {
                    if (compare(values[i])) {
                        RemoveAt(i);
                        matched = true;
                    }
                }
            }
            return matched;
        }

        public void Clear()
        {
            lock (this) {

                keys.Clear();
                values.Clear();
            }
        }

        private int BinarySearchInsertIndex(float order, int lowIndex, int highIndex)
        {
            if (lowIndex == highIndex) { return lowIndex; }
            int mid = (lowIndex + highIndex) / 2;
            if (order < keys[mid]) { return BinarySearchInsertIndex(order, lowIndex, mid); } else { return BinarySearchInsertIndex(order, mid + 1, highIndex); }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"Ordered {typeof(T)} List [{Count}]:");
            for (int i = 0; i < Count; i++) {
                builder.Append($"\n\t{keys[i]}: {values[i]}");
            }
            return builder.ToString();
        }
    }
}