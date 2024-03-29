using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zekzek.CameraControl;
using Zekzek.Combat;
using static InputManager;

namespace Zekzek.HexWorld
{
    public class PlayerController
    {
        private LocationFollowCamera _camera;
        private List<WorldObject> _selected = new List<WorldObject>();

        // Singleton
        private static PlayerController _instance;
        public static PlayerController Instance { get { if (_instance == null) { _instance = new PlayerController(); } return _instance; } }
        private PlayerController() { 
            _camera = Camera.main.GetComponent<LocationFollowCamera>();
            //TODO: remove workaround to force a selected object
            AddSelection(HexWorld.Instance.GetAll(WorldObjectType.Entity).First());

            InputManager.Instance.AddListener<Vector2>(InputMode.WorldNavigation, PlayerAction.Move, InputWatchType.Constant, OnMove);
            InputManager.Instance.AddListener<float>(InputMode.WorldNavigation, PlayerAction.Action, InputWatchType.OnStart, OnAction);
        }

        public void AddSelection(params WorldObject[] targets) { _selected.AddRange(targets); }
        public void RemoveSelection(params WorldObject[] targets) { foreach (WorldObject target in targets) _selected.Remove(target); }
        public void ClearSelection() { _selected.Clear(); }
        public Vector3 GetSelectionPosition()
        {
            //TODO: duplicating logic between _camera targets and _selected - DRY
            return _camera.TargetPosition;
        }

        private void OnMove(Vector2 moveInput)
        {
            foreach (WorldObject worldObject in _selected) {
                // To avoid rescheduling every frame, only update if done with last move
                if (worldObject.Location.IsMoving) { continue; }
                
                WorldUtil.FindNeighbors(worldObject.Id, worldObject.Location.Current, worldObject.Location.Speed, WorldScheduler.Instance.Time, out NavStep forcedStep, out NavStep forwardStep, out NavStep backwardStep, out NavStep leftStep, out NavStep rightStep);
                if (forcedStep != null) {
                    worldObject.Location.Schedule(new List<NavStep> { forcedStep });
                } else if (moveInput.x > 0 && rightStep != null) {
                    worldObject.Location.Schedule(new List<NavStep> { rightStep });
                } else if (moveInput.x < 0 && leftStep != null) {
                    worldObject.Location.Schedule(new List<NavStep> { leftStep });
                } else if (moveInput.y > 0 && forwardStep != null) {
                    worldObject.Location.Schedule(new List<NavStep> { forwardStep });
                } else if (moveInput.y < 0 && backwardStep != null) {
                    worldObject.Location.Schedule(new List<NavStep> { backwardStep });
                }
            }
        }

        private void OnAction(float value)
        {
            if (value > 0.5f) {
                CombatCanvas.Show(_selected[0]);
                InputManager.Instance.PushMode(InputMode.CombatMenu);
            }
        }
    }
}