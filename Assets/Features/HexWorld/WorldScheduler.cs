using System;
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

        public void RegisterIn(float delay, Action callback, float period = -1f)
        {
            RegisterAt(time + delay, callback, period);
        }

        private void InternalRegisterAt(float atTime, Action callback, float period)
        {
            callbacks.Add(atTime, new TimedCallback() { time = atTime, callback = callback, period = period });
        }

        public void Reset()
        {
            Clear();
            time = 0;
        }

        public void Clear()
        {
            callbacks.Clear();
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
    }
}