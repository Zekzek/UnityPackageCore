using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zekzek.Combat;
using Zekzek.UnityModelMaker;

namespace Zekzek.HexWorld
{
    public class HexWorldBehaviour : MonoBehaviour
    {
        [SerializeField] private float _playSpeed;
        [SerializeField] private int _screenHeight;
        [SerializeField] private int _screenWidth;
        [SerializeField] private BehaviourModelMapping[] prefabList;
        [SerializeField] private StatBlockBehaviour statBlockPrefab;

        private static HexWorldBehaviour _instance;
        public static HexWorldBehaviour Instance => _instance;

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        
        private List<TargetableComponent> _highlighted = new List<TargetableComponent>();

        private Vector2Int centerTile = new Vector2Int(0, 0);
        public Vector2Int CenterTile {
            get { return centerTile; }
            set {
                if (value != centerTile) {
                    centerTile = value;
                }
            }
        }

        private void Awake()
        {
            _instance = this;
            foreach(BehaviourModelMapping mapping in prefabList) {
                HexWorldAreaLoader.Instance.AddType(mapping.type, transform, mapping.prefab);
            }
        }

        private void Start()
        {
            if (_meshRenderer == null) { _meshRenderer = GetComponent<MeshRenderer>(); }
            if (_meshFilter == null) { _meshFilter = GetComponent<MeshFilter>(); }

            _meshRenderer.material = MaterialMaker.Instance.Get(0.2f * Vector3.one);

        }

        private void Update()
        {
            UpdateAllVisible();
            WorldScheduler.Instance.Time += _playSpeed * Time.deltaTime;
        }

        private void UpdateAllVisible()
        {
            CenterTile = WorldUtil.PositionToGridIndex(PlayerController.Instance.GetSelectionPosition());
            IEnumerable<Vector2Int> screenIndices = WorldUtil.GetRectangleIndicesAround(CenterTile, _screenWidth, _screenHeight).Where(i=>WorldUtil.IsGridIndexCenter(i));
            IEnumerable<uint> currentActiveIds = HexWorld.Instance.GetIdsAt(screenIndices);

            HexWorldAreaLoader.Instance.ScheduleUpdateVisible(currentActiveIds);
            HexWorldAreaLoader.Instance.Run(0.0005f);
        }

        public void UpdateHighlight(IEnumerable<Vector2Int> gridIndices)
        {
            ClearHighlight();
            Highlight(gridIndices);
        }

        public void ClearHighlight()
        {
            foreach (TargetableComponent targetable in _highlighted) { targetable.Highlight = false; }
            _highlighted.Clear();
        }

        private void Highlight(IEnumerable<Vector2Int> gridIndices)
        {
            IEnumerable<WorldObject> targetableObjects = HexWorld.Instance.GetAt(gridIndices, WorldComponentType.Targetable);

            foreach (WorldObject targetableObject in targetableObjects) {
                if (targetableObject != null) {
                    TargetableComponent targetable = (TargetableComponent)targetableObject.GetComponent(WorldComponentType.Targetable);
                    targetable.Highlight = true;
                    _highlighted.Add(targetable);
                }
            }
        }
    }
}