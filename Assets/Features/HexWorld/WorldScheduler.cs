using System;
using System.Collections.Generic;
using UnityEngine;

namespace Zekzek.HexWorld
{
    public class WorldScheduler
    {
        public Action OnTimeReached;

        private float time = 0;
        public float Time {
            get => time;
            set {
                if (value > time) {
                    time = value;
                    while (callbacks.Count > 0 && callbacks.First.time <= time) {
                        TimedCallback callback = callbacks.First;
                        callbacks.RemoveAt(0);
                        callback.callback?.Invoke();
                        if (callback.period > 0) { InternalRegisterAt(callback.time + callback.period, callback.callback, callback.period); }
                    }
                }
            }
        }

        private readonly OrderedList<TimedCallback> callbacks = new OrderedList<TimedCallback>();
        private readonly IDictionary<uint, OrderedList<TimedLocation>> locationsById = new Dictionary<uint, OrderedList<TimedLocation>>();

        public static WorldScheduler Instance { get; } = new WorldScheduler();
        private WorldScheduler() { }

        public void RegisterAt(float atTime, Action callback, float period = -1f)
        {
            if (atTime >= time) {
                InternalRegisterAt(atTime, callback, period);
            } else {
                Debug.LogWarning($"Ignoring request to schedule an event in the past. {atTime} is before current {time}");
            }
        }

        public void RegisterAt(float atTime, uint id, WorldLocation location)
        {
            if (atTime >= time) {
                InternalRegisterAt(atTime, id, location);
            } else {
                Debug.LogWarning($"Ignoring request to schedule location in the past. {atTime} is before current {time}");
            }
        }

        public void RegisterIn(float delay, Action callback, float period = -1f)
        {
            RegisterAt(time + delay, callback, period);
        }

        public void RegisterIn(float delay, uint id, WorldLocation location)
        {
            RegisterAt(time + delay, id, location);
        }

        private void InternalRegisterAt(float atTime, Action callback, float period)
        {
            callbacks.Add(atTime, new TimedCallback() { time = atTime, callback = callback, period = period });
        }

        //TODO: find a better thread-safe solution than these clumsy locks
        private void InternalRegisterAt(float atTime, uint id, WorldLocation location)
        {
            lock (locationsById) {
                if (!locationsById.ContainsKey(id)) { locationsById.Add(id, new OrderedList<TimedLocation>()); }
                locationsById[id].Add(atTime, new TimedLocation() { time = atTime, location = location });
            }
            ForgetPastLocation(id);
        }

        public bool TryGetLocation(uint id, out WorldLocation location, float atTime = -1)
        {
            if (TryGetLocation(id, out WorldLocation previous, out WorldLocation next, out float percent, atTime)) {
                location = WorldLocation.Lerp(previous, next, percent);
                return true;
            }
            location = null;
            return false;
        }

        public bool TryGetLocation(uint id, out WorldLocation previous, out WorldLocation next, out float percentComplete, float atTime = -1)
        {
            if (atTime < time) { atTime = time; }
            ForgetPastLocation(id);

            lock (locationsById) {
                if (!locationsById.ContainsKey(id) || locationsById[id].First.time > atTime) {
                    Debug.LogWarning("No location found for " + id);
                    previous = next = null;
                    percentComplete = -1;
                    return false;
                } else {
                    bool result = locationsById[id].TryGetAround(atTime, out TimedLocation previousTimed, out TimedLocation nextTimed);
                    previous = previousTimed.location;
                    next = nextTimed.location ?? previous;
                    if (previous == null || previous == next) {
                        percentComplete = 0;
                    } else {
                        percentComplete = (atTime - previousTimed.time) / (nextTimed.time - previousTimed.time);
                    }
                    return result;
                }
            }
        }

        //TODO: make more efficient
        public bool TryGetObjectIn(Vector3Int gridPosition, float delay, out WorldObject result)
        {
            float atTime = time + delay;
            result = null;
            lock (locationsById) {
                foreach(uint id in locationsById.Keys) {
                    OrderedList<TimedLocation> locations = locationsById[id];
                    if (locations.TryGetAround(atTime, out TimedLocation previous, out TimedLocation next) && previous.location.GridPosition == gridPosition) {
                        result = HexWorld.Instance.Get(id);
                        return true;
                    }
                }
            }
            return false;
        }

        private void ForgetPastLocation(uint id)
        {
            lock (locationsById) {
                if (locationsById.ContainsKey(id)) {
                    while (locationsById[id].Count > 1 && locationsById[id][1].time < Time) { locationsById[id].RemoveAt(0); }
                }
            }
        }

        public void Reset()
        {
            Clear();
            time = 0;
        }

        public void Clear()
        {
            callbacks.Clear();
            lock (locationsById) {
                locationsById.Clear();
            }
        }

        public bool Unregister(Action callback)
        {
            if (callbacks.Remove(other => other.callback.Equals(callback))) {
                return true;
            } else {
                Debug.LogWarning($"Unable to unregister callback. No matching callback found.");
                return false;
            }
        }

        public bool Unregister(uint id)
        {
            lock (locationsById) {
                return locationsById.Remove(id);
            }
        }

        public void UnregisterAll(Action callback)
        {
            callbacks.RemoveAll(other => other.callback.Equals(callback));
        }

        private struct TimedCallback
        {
            public float time;
            public Action callback;
            public float period;
        }

        private struct TimedLocation
        {
            public float time;
            public WorldLocation location;
        }
    }
}