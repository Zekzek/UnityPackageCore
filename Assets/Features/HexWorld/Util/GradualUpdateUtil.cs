using System;
using System.Collections.Generic;
using System.Diagnostics;

public class GradualUpdateUtil<T>
{
    // Singleton instance
    private static GradualUpdateUtil<T> _instance;
    public static GradualUpdateUtil<T> Instance { get { _instance ??= new GradualUpdateUtil<T>(); return _instance; } }

    public enum Priority { Critical, High, Medium, Low, None}

    private readonly Dictionary<short, Action<T>> _callbacks = new Dictionary<short, Action<T>>();
    private readonly Dictionary<Priority, Dictionary<short, List<T>>> _queue = new Dictionary<Priority, Dictionary<short, List<T>>>();
    private readonly Stopwatch stopwatch = new Stopwatch();

    public void Run(long durationMs, params short[] keys) {
        // Initialize the timeout for this pass
        long timeoutMs = stopwatch.ElapsedMilliseconds + durationMs;

        // Iterate and callback by highest priority, key, and queued order
        foreach (Priority priority in Enum.GetValues(typeof(Priority))) {
            foreach (short key in keys) {
                while (_queue[priority][key].Count > 0) {
                    // Run the next highest priority callback
                    T thing = _queue[priority][key][0];
                    _queue[priority][key].RemoveAt(0);
                    _callbacks[key](thing);

                    // Bail out once beyond requested duration
                    if (stopwatch.ElapsedMilliseconds > timeoutMs) { return; }
                }
            }
        }
    }

    public void RegisterCallback(short key, Action<T> callback) {
        // Replace any existing callback
        _callbacks[key] = callback;
    }

    public void Queue(Priority priority, short key, List<T> entries) {
        // Refuse to queue if no callback exists
        if (!_callbacks.ContainsKey(key)) {
            UnityEngine.Debug.LogWarning("No callback set up for " + key + ". Refusing to queue entries!");
            return;
        }

        // Create required collections and queue
        if (!_queue.ContainsKey(priority)) { _queue.Add(priority, new Dictionary<short, List<T>>()); }
        if (!_queue[priority].ContainsKey(key)) { _queue[priority].Add(key, new List<T>()); }
        _queue[priority][key].AddRange(entries);
    }
}
