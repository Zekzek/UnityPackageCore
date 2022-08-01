using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zekzek.CameraControl;

namespace Zekzek.HexWorld
{
    public class HexWorldBehaviour : MonoBehaviour
    {
        [SerializeField] private float _playSpeed;
        [SerializeField] private int _screenHeight;
        [SerializeField] private int _screenWidth;
        [SerializeField] private BehaviourModelMapping[] prefabList;
        [SerializeField] private StatBlockBehaviour statBlockPrefab;

        private readonly Dictionary<WorldObjectType, Transform> _containersByType = new Dictionary<WorldObjectType, Transform>();
        private readonly Dictionary<WorldObjectType, WorldObjectBehaviour> _prefabsByType = new Dictionary<WorldObjectType, WorldObjectBehaviour>();
        
        private readonly Dictionary<WorldObjectType, Dictionary<uint, WorldObjectBehaviour>> _behavioursByType = new Dictionary<WorldObjectType, Dictionary<uint, WorldObjectBehaviour>>();
        private readonly Dictionary<WorldObjectType, List<WorldObjectBehaviour>> _unusedBehavioursByType = new Dictionary<WorldObjectType, List<WorldObjectBehaviour>>();
        private readonly Dictionary<WorldObjectType, Dictionary<uint, StatBlockBehaviour>> _statBlocksByType = new Dictionary<WorldObjectType, Dictionary<uint, StatBlockBehaviour>>();
        private readonly Dictionary<WorldObjectType, List<StatBlockBehaviour>> _unusedStatBlocksByType = new Dictionary<WorldObjectType, List<StatBlockBehaviour>>();
        private readonly Dictionary<WorldObjectType, bool> _dirtyByType = new Dictionary<WorldObjectType, bool>();

        private Vector2Int centerTile = new Vector2Int(0, 0);
        public Vector2Int CenterTile {
            get { return centerTile; }
            set {
                if (value != centerTile) {
                    centerTile = value;
                    foreach (WorldObjectType key in _dirtyByType.Keys.ToList()) {
                        _dirtyByType[key] = true;
                    }
                }
            }
        }

        private void Awake()
        {
            InitTypeCollections();
        }

        private void Update()
        {
            UpdateAllVisible();
            PlayerController.Instance.HandleInput();

            WorldScheduler.Instance.Time += _playSpeed * Time.deltaTime;
        }

        private void InitTypeCollections()
        {
            foreach (BehaviourModelMapping mapping in prefabList) {
                var type = mapping.type;
                _behavioursByType.Add(type, new Dictionary<uint, WorldObjectBehaviour>());
                _unusedBehavioursByType.Add(type, new List<WorldObjectBehaviour>());
                _statBlocksByType.Add(type, new Dictionary<uint, StatBlockBehaviour>());
                _unusedStatBlocksByType.Add(type, new List<StatBlockBehaviour>());
                _dirtyByType.Add(type, true);
                if (!_prefabsByType.ContainsKey(type)) {
                    _prefabsByType[type] = mapping.prefab;
                }
                if (!_containersByType.ContainsKey(type)) {
                    _containersByType[type] = new GameObject($"{type}Container").transform;
                    _containersByType[type].parent = transform;
                }
            }
        }

        private void AllocatePrefab(WorldObjectType type)
        {
            WorldObjectBehaviour behaviour = Instantiate(_prefabsByType[type], _containersByType[type]);
            _unusedBehavioursByType[type].Add(behaviour);
        }

        private void AllocateStatBlock(WorldObjectType type)
        {
            StatBlockBehaviour behaviour = Instantiate(statBlockPrefab);
            _unusedStatBlocksByType[type].Add(behaviour);
        }


        private void UpdateAllVisible()
        {
            CenterTile = WorldUtil.PositionToGridIndex(PlayerController.Instance.GetSelectionPosition());
            IEnumerable<Vector2Int> screenIndices = WorldUtil.GetRectangleIndicesAround(centerTile, _screenWidth, _screenHeight);
            IEnumerable<uint> currentActiveIds = HexWorld.Instance.GetIdsAt(screenIndices);

            foreach (WorldObjectType type in _behavioursByType.Keys) {
                if (!_dirtyByType[type]) { continue; }

                // Hide objects which are no longer visible
                List<uint> previousActveBehaviourIds = _behavioursByType[type].Keys.ToList();
                foreach (uint previousActiveId in previousActveBehaviourIds) {
                    if (!currentActiveIds.Contains(previousActiveId)) {
                        _behavioursByType[type][previousActiveId].gameObject.SetActive(false);
                        _unusedBehavioursByType[type].Add(_behavioursByType[type][previousActiveId]);
                        _behavioursByType[type].Remove(previousActiveId);
                    }
                }
                List<uint> previousActveStatBlockIds = _statBlocksByType[type].Keys.ToList();
                foreach (uint previousActiveId in previousActveStatBlockIds) {
                    if (!currentActiveIds.Contains(previousActiveId)) {
                        _statBlocksByType[type][previousActiveId].gameObject.SetActive(false);
                        _unusedStatBlocksByType[type].Add(_statBlocksByType[type][previousActiveId]);
                        _statBlocksByType[type].Remove(previousActiveId);
                    }
                }

                // Set up objects which have become visible
                foreach (WorldObject worldObject in HexWorld.Instance.GetAt(screenIndices, type)) {
                    if (previousActveBehaviourIds.Contains(worldObject.Id)) { continue; }
                    if (_unusedBehavioursByType[type].Count == 0) { AllocatePrefab(type); }
                    _behavioursByType[type].Add(worldObject.Id, _unusedBehavioursByType[type][0]);
                    _unusedBehavioursByType[type].RemoveAt(0);
                    _behavioursByType[type][worldObject.Id].Model = worldObject;
                    _behavioursByType[type][worldObject.Id].gameObject.SetActive(true);

                    if (worldObject.Stats == null) { continue; }
                    if (previousActveBehaviourIds.Contains(worldObject.Id)) { continue; }
                    if (_unusedStatBlocksByType[type].Count == 0) { AllocateStatBlock(type); }
                    _statBlocksByType[type].Add(worldObject.Id, _unusedStatBlocksByType[type][0]);
                    _unusedStatBlocksByType[type].RemoveAt(0);
                    _statBlocksByType[type][worldObject.Id].transform.parent = _behavioursByType[type][worldObject.Id].transform;
                    _statBlocksByType[type][worldObject.Id].Model = worldObject.Stats;
                    _statBlocksByType[type][worldObject.Id].gameObject.SetActive(true);
                }
                _dirtyByType[type] = false;
            }
        }
    }
}