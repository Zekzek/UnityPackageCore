using System.Collections.Generic;
using UnityEngine;

namespace Zekzek.HexWorld
{
    public class HexWorldBehaviour : MonoBehaviour
    {
        [SerializeField] private float _playSpeed = 1f;
        [SerializeField] private int _screenHeight = 3;
        [SerializeField] private int _screenWidth = 3;
        [SerializeField] private MonoBehaviour[] prefabList;

        private Dictionary<System.Type, Transform> containers = new Dictionary<System.Type, Transform>();
        private Dictionary<System.Type, MonoBehaviour> prefabs = new Dictionary<System.Type, MonoBehaviour>();

        private Camera _camera;

        private static int maxVisibleTiles;
        public static bool TilesDirty { get; set; } = true;
        public static bool WorldObjectsDirty { get; set; } = true;

        private readonly List<HexTileBehaviour> allTiles = new List<HexTileBehaviour>();
        //private List<LocationBehaviour> allWorldObjects = new List<LocationBehaviour>();

        private List<HexTile> highlightedTiles = new List<HexTile>();

        private static Vector2Int centerTile = new Vector2Int(0, 0);
        public static Vector2Int CenterTile {
            get { return centerTile; }
            set {
                if (value != centerTile) {
                    centerTile = value;
                    TilesDirty = true;
                }
            }
        }

        private void Awake()
        {
            _camera = Camera.main;
            PreallocateContainers();
            //PreallocateAllTiles();
        }

        private void Update()
        {
            UpdateVisibleHexTiles();
            UpdateWorldObjects();
            UpdateTileHighlight();

            WorldScheduler.Instance.Time += _playSpeed * Time.deltaTime;
        }

        private void PreallocateContainers()
        {
            if (containers == null) { containers = new Dictionary<System.Type, Transform>(); }
            if (prefabs == null) { prefabs = new Dictionary<System.Type, MonoBehaviour>(); }
            foreach (MonoBehaviour prefab in prefabList) {
                InitPrefabContainer(prefab);
            }
        }

        private void InitPrefabContainer<T>(T prefab) where T : MonoBehaviour
        {
            System.Type type = prefab.GetType();
            if (!prefabs.ContainsKey(type)) {
                prefabs[type] = prefab;
            }
            if (!containers.ContainsKey(type)) {
                containers[type] = new GameObject($"{type.Name}Container").transform;
                containers[type].parent = transform;
            }
        }

        private void PreallocateAllTiles()
        {
            while (allTiles.Count < maxVisibleTiles) {
                HexTileBehaviour tile = (HexTileBehaviour)Instantiate(prefabs[typeof(HexTileBehaviour)], containers[typeof(HexTileBehaviour)]);
                tile.gameObject.SetActive(false);
                allTiles.Add(tile);
            }
        }

        private void AllocatePrefab<T>(T prefab) where T : MonoBehaviour
        {
            System.Type type = prefab.GetType();

            MonoBehaviour tile = Instantiate(prefabs[type], containers[type]);
            tile.gameObject.SetActive(false);
            allTiles.Add((HexTileBehaviour)tile);
        }

        private void UpdateVisibleHexTiles()
        {
            if (!TilesDirty) { return; }

            // Clear all current tiles
            foreach (HexTileBehaviour tile in allTiles) { tile.gameObject.SetActive(false); }

            // Build new tiles from visible region
            int tileIndex = 0;
            foreach (HexTile tile in HexWorld.Instance.tiles.GetItemsAt(WorldUtil.GetIndicesAround(centerTile, _screenWidth, _screenHeight))) {
                if (tileIndex >= allTiles.Count) { AllocatePrefab(prefabs[typeof(HexTileBehaviour)]); }
                allTiles[tileIndex].Apply(tile);
                tileIndex++;
            }
            TilesDirty = false;
        }

        private void UpdateWorldObjects()
        {
            if (!WorldObjectsDirty) { return; }

            Debug.Log("updating world objects");

//            ICollection<WorldObject> worldObjects = World.Instance.GetWorldObjectsAround(centerTile);
//            foreach (WorldObject worldObject in worldObjects) {
//                // Check for exisitng location behaviours
//                bool found = false;
//                foreach (LocationBehaviour existingBehaviour in allWorldObjects) {
//                    if (existingBehaviour != null && existingBehaviour.WorldObject.Id == worldObject.Id) { found = true; break; }
//                }
//                if (found) { continue; }
//
//                // Create new behaviour
//                LocationBehaviour locationBehaviour = Instantiate(bush1Prefab, ResourceContainer);
//                locationBehaviour.transform.localPosition = worldObject.Location.Position;
//                locationBehaviour.WorldObject = worldObject;
//                allWorldObjects.Add(locationBehaviour);
//            }
            WorldObjectsDirty = false;
        }

        private void UpdateTileHighlight()
        {
            foreach (HexTile tile in highlightedTiles) { tile.Highlight = false; }
            highlightedTiles.Clear();


            RaycastHit hit;
            Ray ray = _camera.ScreenPointToRay(InputManager.Instance.GetCursorPosition());
            
            if (Physics.Raycast(ray, out hit)) {
                Transform objectHit = hit.transform;
                HexTileBehaviour tile = objectHit.gameObject.GetComponent<HexTileBehaviour>();
                if (tile != null && tile.Model != null) {
                    Highlight(tile.Model.Location.GridIndex, Vector2Int.zero, 0);
                    tile.HandleInput();
                    if (InputManager.Instance.Get<float>(InputManager.PlayerAction.Tap) > 0) {
                        //WorldObject worldObject = allWorldObjects[0].WorldObject;
                        //worldObject.Location.NavigateTo(tile.Model.GridPos, worldObject.Speed);
                    }
                }
            }
        }

        private void Highlight(Vector2Int center, Vector2Int offset, float rotation)
        {
            Vector2Int rotated = FacingUtil.RotateAround(center + offset, center, rotation);

            HexTile tile = HexWorld.Instance.tiles.GetFirstItemAt(rotated);

            if (tile != null) {
                tile.Highlight = true;
                highlightedTiles.Add(tile);
            }
        }
    }
}