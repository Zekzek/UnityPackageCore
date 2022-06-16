using System.Collections.Generic;
using UnityEngine;

namespace Zekzek.HexWorld
{
    public class HexWorldDisplay : MonoBehaviour
    {
        [SerializeField] private float _playSpeed = 1f;
        [SerializeField] private int _screenHeight = 3;
        [SerializeField] private int _screenWidth = 3;

        [SerializeField] private Transform tileContainer;
        [SerializeField] private Transform entityContainer;

        [SerializeField] private HexTileBehaviour hexTilePrefab;
        //[SerializeField] private PlayerBehaviour playerPrefab;

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
            InitScreenSize();
            PreallocateAllTiles();
        }

        private void Update()
        {
            UpdateVisibleHexTiles();
            UpdateWorldObjects();
            UpdateTileHighlight();

            WorldScheduler.Instance.Time += _playSpeed * Time.deltaTime;
        }

        private void InitScreenSize()
        {
            World.Instance.SetScreenDimensions(_screenWidth, _screenHeight);
            maxVisibleTiles = (_screenWidth + 1) * (_screenHeight + 1);
        }

        private void PreallocateAllTiles()
        {
            while (allTiles.Count < maxVisibleTiles) {
                HexTileBehaviour tile = Instantiate(hexTilePrefab, tileContainer);
                tile.gameObject.SetActive(false);
                allTiles.Add(tile);
            }
        }

        private void UpdateVisibleHexTiles()
        {
            //if (!TilesDirty) { return; }

            // Clear all current tiles
            foreach (var tile in allTiles) { tile.gameObject.SetActive(false); }

            // Build new tiles from visible region
            int tileIndex = 0;
            foreach (HexTile tile in World.Instance.GetScreenTilesAround(centerTile)) {
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
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit)) {
                Transform objectHit = hit.transform;
                HexTileBehaviour tile = objectHit.gameObject.GetComponent<HexTileBehaviour>();
                if (tile != null && tile.Model != null) {
                    Highlight(tile.Model.GridIndex, Vector2Int.zero, 0);
                    tile.HandleInput();
                    if (Input.GetMouseButtonDown(0)) {
                        //WorldObject worldObject = allWorldObjects[0].WorldObject;
                        //worldObject.Location.NavigateTo(tile.Model.GridPos, worldObject.Speed);
                    }
                }
            }
        }

        private void Highlight(Vector2Int center, Vector2Int offset, float rotation)
        {
            Vector2Int rotated = FacingUtil.RotateAround(center + offset, center, rotation);

            HexTile tile = World.Instance.TileAt(rotated);

            if (tile != null) {
                tile.Highlight = true;
                highlightedTiles.Add(tile);
            }
        }
    }
}