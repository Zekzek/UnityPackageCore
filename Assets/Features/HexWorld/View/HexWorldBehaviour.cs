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

        private readonly Dictionary<WorldObjectType, Transform> _containersByType = new Dictionary<WorldObjectType, Transform>();
        private readonly Dictionary<WorldObjectType, WorldObjectBehaviour> _prefabsByType = new Dictionary<WorldObjectType, WorldObjectBehaviour>();
        private readonly Dictionary<WorldObjectType, List<WorldObjectBehaviour>> _behavioursByType = new Dictionary<WorldObjectType, List<WorldObjectBehaviour>>();
        private readonly Dictionary<WorldObjectType, bool> _dirtyByType = new Dictionary<WorldObjectType, bool>();

        private TransformFollowCamera _camera;

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
            _camera = Camera.main.GetComponent<TransformFollowCamera>();
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

            if (type == WorldObjectType.Entity) {
                CameraController<Transform>.Priority.AddTarget(behaviour.transform);
            }
        }

        private void UpdateAllVisible()
        {
            CenterTile = WorldUtil.PositionToGridIndex(_camera.TargetPosition);
            IEnumerable<Vector2Int> screenIndices = WorldUtil.GetRectangleIndicesAround(centerTile, _screenWidth, _screenHeight);
            foreach (WorldObjectType type in _behavioursByType.Keys) {
                if (!_dirtyByType[type]) { continue; }

                // Clear all current
                foreach (WorldObjectBehaviour behaviour in _behavioursByType[type]) { behaviour.gameObject.SetActive(false); }

                // Build new behaviours from visible region
                int index = 0;
                foreach (WorldObject worldObject in HexWorld.Instance.GetAt(screenIndices, type)) {
                    if (index >= _behavioursByType[type].Count) { AllocatePrefab(type); }
                    _behavioursByType[type][index].Model = worldObject;
                    _behavioursByType[type][index].gameObject.SetActive(true);
                    index++;
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