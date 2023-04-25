using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Zekzek.HexWorld;

public class HexWorldAreaLoader
{
    private static HexWorldAreaLoader _instance;
    public static HexWorldAreaLoader Instance => _instance == null ? _instance = new HexWorldAreaLoader() : _instance;

    // Scheduling
    private readonly Stopwatch _timer;
    private readonly Dictionary<WorldObjectType, List<uint>> _hideIdsByType;
    private readonly Dictionary<WorldObjectType, List<uint>> _showIdsByType;
    private readonly Dictionary<WorldObjectType, List<uint>> _usedIdsByType;
    private readonly Dictionary<WorldObjectType, List<WorldObjectBehaviour>> _usedObjectsByType;
    private readonly Dictionary<WorldObjectType, List<WorldObjectBehaviour>> _unusedObjectsByType;
    private IEnumerable<uint> _lastActiveIds;

    // GameObjects
    private readonly Dictionary<WorldObjectType, Transform> _containersByType;
    private readonly Dictionary<WorldObjectType, WorldObjectBehaviour> _prefabsByType;


    private HexWorldAreaLoader()
    {
        _timer = new Stopwatch();
        _timer.Start();
        _hideIdsByType = new Dictionary<WorldObjectType, List<uint>>();
        _showIdsByType = new Dictionary<WorldObjectType, List<uint>>();
        _usedIdsByType = new Dictionary<WorldObjectType, List<uint>>();
        _usedObjectsByType = new Dictionary<WorldObjectType, List<WorldObjectBehaviour>>();
        _unusedObjectsByType = new Dictionary<WorldObjectType, List<WorldObjectBehaviour>>();

        _containersByType = new Dictionary<WorldObjectType, Transform>();
        _prefabsByType = new Dictionary<WorldObjectType, WorldObjectBehaviour>();
    }

    public void AddType(WorldObjectType type, Transform container, WorldObjectBehaviour prefab)
    {
        _containersByType[type] = container;
        _prefabsByType[type] = prefab;

        // Init local dictionaries
        if (!_hideIdsByType.ContainsKey(type)) { _hideIdsByType.Add(type, new List<uint>()); }
        if (!_showIdsByType.ContainsKey(type)) { _showIdsByType.Add(type, new List<uint>()); }
        if (!_usedIdsByType.ContainsKey(type)) { _usedIdsByType.Add(type, new List<uint>()); }
        if (!_usedObjectsByType.ContainsKey(type)) { _usedObjectsByType.Add(type, new List<WorldObjectBehaviour>()); }
        if (!_unusedObjectsByType.ContainsKey(type)) { _unusedObjectsByType.Add(type, new List<WorldObjectBehaviour>()); }
    }

    public void ScheduleUpdateVisible(IEnumerable<uint> ids)
    {
        if (Equals(ids, _lastActiveIds)) { return; }
        _lastActiveIds = ids;

        foreach(KeyValuePair<WorldObjectType, List<uint>> pair in _usedIdsByType) {
            foreach(uint id in pair.Value) {
                if (!ids.Contains(id)) {
                    ScheduleHide(pair.Key, id);
                }
            }
        }

        foreach (uint id in ids) {
            WorldObjectType type = HexWorld.Instance.Get(id).Type;
            if (!_prefabsByType.ContainsKey(type)) {
                UnityEngine.Debug.LogWarning("Unknown type: " + type);
                continue;
            }

            if (!_usedIdsByType[type].Contains(id)) {
                ScheduleShow(type, id);
            }
        }
    }

    private void ScheduleHide(WorldObjectType type, uint id)
    {
        if (_hideIdsByType[type].Contains(id) || !_usedIdsByType[type].Contains(id)) {
            // already scheduled/hidden, do nothing
        } else if (_showIdsByType.ContainsKey(type) && _showIdsByType[type].Contains(id)) {
            // cancel scheduled 'show' so it remains hidden
            _showIdsByType[type].Remove(id);
        } else {
            // schedule 'hide'
            UnityEngine.Debug.Log("Schedule Hide " + id);
            _hideIdsByType[type].Add(id);
        }
    }

    private void ScheduleShow(WorldObjectType type, uint id)
    {
        if (_showIdsByType[type].Contains(id) || _usedIdsByType[type].Contains(id)) {
            // already scheduled/shown, do nothing
        } else if (_hideIdsByType.ContainsKey(type) && _hideIdsByType[type].Contains(id)) {
            // cancel scheduled 'hide' so it remains shown
            _hideIdsByType[type].Remove(id);
        } else {
            // schedule 'show'
            UnityEngine.Debug.Log("Schedule Show " + id);
            _showIdsByType[type].Add(id);
        }
    }

    public void Run(float durationMs)
    {
        float timeoutAtMs = _timer.ElapsedMilliseconds + durationMs;
        bool hideScheduled = true;
        bool showScheduled = true;

        // perform hide on everything scheduled, quit when timeout is reached
        while (hideScheduled && _timer.ElapsedMilliseconds < timeoutAtMs) {

            hideScheduled = false;
            foreach (KeyValuePair<WorldObjectType, List<uint>> pair in _hideIdsByType) {
                if (pair.Value.Count > 0) {
                    HideFirst(pair.Key);
                    hideScheduled = true;
                    break;
                }
            }
        }

        // perform show on everything scheduled, quit when timeout is reached
        while (showScheduled && _timer.ElapsedMilliseconds < timeoutAtMs) {
            showScheduled = false;
            foreach (KeyValuePair<WorldObjectType, List<uint>> pair in _showIdsByType) {
                if (pair.Value.Count > 0) {
                    ShowFirst(pair.Key);
                    showScheduled = true;
                    break;
                }
            }
        }
    }

    private void HideFirst(WorldObjectType type)
    {
        // Remove model with the first id from its GameObject
        uint id = _hideIdsByType[type][0];
        WorldObjectBehaviour worldObject = _usedObjectsByType[type].Find(x => x.Model.Id == id);
        worldObject.Model = null;
        worldObject.gameObject.SetActive(false);

        // Clean up
        _usedObjectsByType[type].Remove(worldObject);
        _usedIdsByType[type].Remove(id);
        _unusedObjectsByType[type].Add(worldObject);
        _hideIdsByType[type].RemoveAt(0);
    }

    private void ShowFirst(WorldObjectType type)
    {
        // Make new objects as necessary
        if (_unusedObjectsByType[type].Count == 0) { AllocatePrefab(type); }

        // Add model with the first id to an unused GameObject
        uint id = _showIdsByType[type][0];
        WorldObjectBehaviour worldObject = _unusedObjectsByType[type][0];
        worldObject.Model = HexWorld.Instance.Get(id);
        worldObject.gameObject.SetActive(true); // delay until after object updates with model?

        // Clean up
        _unusedObjectsByType[type].RemoveAt(0);
        _usedObjectsByType[type].Add(worldObject);
        _usedIdsByType[type].Add(id);
        _showIdsByType[type].RemoveAt(0);
    }

    private void AllocatePrefab(WorldObjectType type)
    {
        WorldObjectBehaviour behaviour = Object.Instantiate(_prefabsByType[type], _containersByType[type]);
        _unusedObjectsByType[type].Add(behaviour);
    }
}