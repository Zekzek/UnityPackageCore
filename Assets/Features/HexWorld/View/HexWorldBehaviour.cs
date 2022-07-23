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
        private readonly Dictionary<WorldObjectType, List<WorldObjectBehaviour>> _behavioursByType = new Dictionary<WorldObjectType, List<WorldObjectBehaviour>>();
        private readonly Dictionary<WorldObjectType, List<StatBlockBehaviour>> _statBlocksByType = new Dictionary<WorldObjectType, List<StatBlockBehaviour>>();
        private readonly Dictionary<WorldObjectType, bool> _dirtyByType = new Dictionary<WorldObjectType, bool>();

        private LocationFollowCamera _camera;

        private List<TargetableComponent> highlighted = new List<TargetableComponent>();

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
            _camera = Camera.main.GetComponent<LocationFollowCamera>();
            InitTypeCollections();
        }

        private void Update()
        {
            UpdateAllVisible();
            UpdateTileHighlight();

            WorldScheduler.Instance.Time += _playSpeed * Time.deltaTime;
        }

        private void InitTypeCollections()
        {
            foreach (BehaviourModelMapping mapping in prefabList) {
                var type = mapping.type;
                _behavioursByType.Add(type, new List<WorldObjectBehaviour>());
                _statBlocksByType.Add(type, new List<StatBlockBehaviour>());
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
            _behavioursByType[type].Add(behaviour);
        }

        private void AllocateStatBlock(WorldObjectType type)
        {
            StatBlockBehaviour behaviour = Instantiate(statBlockPrefab);
            _statBlocksByType[type].Add(behaviour);
        }


        private void UpdateAllVisible()
        {
            CenterTile = WorldUtil.PositionToGridIndex(_camera.TargetPosition);
            IEnumerable<Vector2Int> screenIndices = WorldUtil.GetRectangleIndicesAround(centerTile, _screenWidth, _screenHeight);
            
            foreach (WorldObjectType type in _behavioursByType.Keys) {
                if (!_dirtyByType[type]) { continue; }

                // Clear all current
                foreach (WorldObjectBehaviour behaviour in _behavioursByType[type]) { behaviour.gameObject.SetActive(false); }
                foreach (StatBlockBehaviour behaviour in _statBlocksByType[type]) { behaviour.gameObject.SetActive(false); }

                // Build new behaviours from visible region
                int behaviourIndex = 0;
                int statBlockIndex = 0;
                foreach (WorldObject worldObject in HexWorld.Instance.GetAt(screenIndices, type)) {
                    if (behaviourIndex >= _behavioursByType[type].Count) { AllocatePrefab(type); }
                    _behavioursByType[type][behaviourIndex].Model = worldObject;
                    _behavioursByType[type][behaviourIndex].gameObject.SetActive(true);
                    if (worldObject.HasComponent(WorldComponentType.Stats)) {
                        if (statBlockIndex >= _statBlocksByType[type].Count) { AllocateStatBlock(type); }
                        _statBlocksByType[type][statBlockIndex].transform.parent = _behavioursByType[type][behaviourIndex].transform;
                        _statBlocksByType[type][statBlockIndex].Model = worldObject.Stats;
                        _statBlocksByType[type][statBlockIndex].gameObject.SetActive(true);
                        statBlockIndex++;
                    }
                    behaviourIndex++;
                }
                _dirtyByType[type] = false;
            }
        }

        private void UpdateTileHighlight()
        {
            foreach (TargetableComponent targetable in highlighted) { targetable.Highlight = false; }
            highlighted.Clear();

            RaycastHit hit;
            Ray ray = _camera.Camera.ScreenPointToRay(InputManager.Instance.GetCursorPosition());
            
            if (Physics.Raycast(ray, out hit)) {
                Transform objectHit = hit.transform;
                HexTileBehaviour tile = objectHit.gameObject.GetComponent<HexTileBehaviour>();
                if (tile != null && tile.Model != null) {
                    Highlight(tile.Model.Location.GridIndex, Vector2Int.zero, 0);
                    if (InputManager.Instance.Get<float>(InputManager.PlayerAction.Tap) > 0) {
                        WorldObject worldObject = HexWorld.Instance.GetAll(WorldObjectType.Entity).First();
                        worldObject.Location.NavigateTo(tile.Model.Location.GridPosition, worldObject.Location.Speed);
                    }
                }
            }
        }

        private void Highlight(Vector2Int center, Vector2Int offset, float rotation)
        {
            Vector2Int rotated = FacingUtil.RotateAround(center + offset, center, rotation);

            IEnumerable<WorldObject> targetableObjects = HexWorld.Instance.GetAt(new[] { rotated }, WorldComponentType.Targetable);

            foreach (WorldObject targetableObject in targetableObjects) {
                if (targetableObject != null) {
                    TargetableComponent targetable = (TargetableComponent)targetableObject.GetComponent(WorldComponentType.Targetable);
                    targetable.Highlight = true;
                    highlighted.Add(targetable);
                }
            }
        }
    }
}