using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zekzek.CameraControl;

namespace Zekzek.HexWorld
{
    public class PlayerController
    {
        private LocationFollowCamera _camera;
        private List<WorldObject> _selected = new List<WorldObject>();
        private List<TargetableComponent> _highlighted = new List<TargetableComponent>();

        // Singleton
        private static PlayerController _instance;
        public static PlayerController Instance { get { if (_instance == null) { _instance = new PlayerController(); } return _instance; } }
        private PlayerController() { 
            _camera = Camera.main.GetComponent<LocationFollowCamera>();
            //TODO: remove workaround to force a selected object
            AddSelection(HexWorld.Instance.GetAll(WorldObjectType.Entity).First());

        }


        public void AddSelection(params WorldObject[] targets) { _selected.AddRange(targets); }
        public void RemoveSelection(params WorldObject[] targets) { foreach (WorldObject target in targets) _selected.Remove(target); }
        public void ClearSelection() { _selected.Clear(); }
        public Vector3 GetSelectionPosition()
        {
            //TODO: duplicating logic between _camera targets and _selected - DRY
            return _camera.TargetPosition;
        }

        public void HandleInput()
        {
            foreach (TargetableComponent targetable in _highlighted) { targetable.Highlight = false; }
            _highlighted.Clear();

            RaycastHit hit;
            Ray ray = _camera.Camera.ScreenPointToRay(InputManager.Instance.GetCursorPosition());

            if (Physics.Raycast(ray, out hit)) {
                Transform objectHit = hit.transform;
                HexTileBehaviour tile = objectHit.gameObject.GetComponent<HexTileBehaviour>();
                if (tile != null && tile.Model != null) {
                    Highlight(tile.Model.Location.GridIndex, Vector2Int.zero, 0);
                    if (InputManager.Instance.Get<float>(InputManager.PlayerAction.Tap) > 0) {
                        foreach (WorldObject worldObject in _selected) {
                            worldObject.Location.NavigateTo(tile.Model.Location.GridPosition, worldObject.Location.Speed);
                        }
                    }
                }
            }

            Vector2 rotateAmount = InputManager.Instance.Get<Vector2>(InputManager.PlayerAction.Move);

            if (rotateAmount.x > 0) {
                foreach (WorldObject worldObject in _selected) {
                    worldObject.Location.Schedule(worldObject.Location.Current.RotateRight(), worldObject.Location.Speed.Rotate);
                }
            } else if (rotateAmount.x < 0) {
                foreach (WorldObject worldObject in _selected) {
                    worldObject.Location.Schedule(worldObject.Location.Current.RotateLeft(), worldObject.Location.Speed.Rotate);
                }
            }

            if (rotateAmount.y > 0) {
                foreach (WorldObject worldObject in _selected) {
                    LocationComponent forwardTileLocation = (LocationComponent)HexWorld.Instance.GetFirstAt(
                        worldObject.Location.GridIndex + worldObject.Location.Facing,
                        WorldObjectType.Tile)?.GetComponent(WorldComponentType.Location);
                    worldObject.Location.NavigateTo(forwardTileLocation.GridPosition, worldObject.Location.Speed);
                }
            } else if (rotateAmount.y < 0) {
                foreach (WorldObject worldObject in _selected) {
                    LocationComponent backwardTileLocation = (LocationComponent)HexWorld.Instance.GetFirstAt(
                        worldObject.Location.GridIndex - worldObject.Location.Facing,
                        WorldObjectType.Tile)?.GetComponent(WorldComponentType.Location);
                    worldObject.Location.NavigateTo(backwardTileLocation.GridPosition, worldObject.Location.Speed);
                }
            }

            if (InputManager.Instance.IsStarted(InputManager.PlayerAction.Action)) {
                TestFrontalAttack();
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
                    _highlighted.Add(targetable);
                }
            }
        }

        private void TestFrontalAttack()
        {
            foreach (WorldObject selection in _selected) {
                Vector2Int frontGridIndex = selection.Location.Current.GridIndex + selection.Location.Facing;
                IEnumerable<WorldObject> opponents = HexWorld.Instance.GetAt(frontGridIndex, WorldComponentType.Stats);
                foreach (WorldObject opponent in opponents) {
                    opponent.Stats.StatBlock.AddDelta(Stats.StatType.Health, -10f);
                }
            }
        }
    }
}