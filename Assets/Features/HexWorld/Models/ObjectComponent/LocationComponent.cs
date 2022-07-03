using System;
using System.Collections.Generic;
using UnityEngine;

namespace Zekzek.HexWorld
{
    public class LocationComponent : WorldObjectComponent
    {
        public Action OnGridIndexChange; 

        public override WorldComponentType ComponentType => WorldComponentType.Location;
        public WorldLocation Current => _location;
        public MovementSpeed Speed { get; private set; }

        public virtual Vector3 Position {
            get => Current.Position;
            set { UpdateLocation(new WorldLocation(value, _location?.RotationAngle ?? 0f)); }
        }

        public Vector3Int GridPosition {
            get => Current.GridPosition;
            set { UpdateLocation(new WorldLocation(value, _location?.Facing ?? Vector2Int.right)); }
        }

        public Vector2Int GridIndex {
            get => Current.GridIndex;
            set { UpdateLocation(new WorldLocation(WorldUtil.GridIndexToPosition(value, _location?.GridPosition.y ?? 0), _location?.RotationAngle ?? 0f)); }
        }

        public float RotationAngle {
            get => Current.RotationAngle;
            set { UpdateLocation(new WorldLocation(_location?.Position ?? Vector3.zero, value)); }
        }

        public Vector2Int Facing {
            get => Current.Facing;
            set { UpdateLocation(new WorldLocation(_location?.Position ?? Vector3.zero, value)); }
        }

        private WorldLocation _location;

        public LocationComponent(uint worldObjectId, Vector3Int gridPosition, float rotationAngle = 0, MovementSpeed speed = null) : base(worldObjectId)
        {
            _location = new WorldLocation(gridPosition, rotationAngle);
            Speed = speed; 
            WorldScheduler.Instance.RegisterIn(0, WorldObjectId, Current);
        }

        private void UpdateLocation(WorldLocation location)
        {
            if (Equals(_location?.GridIndex, location?.GridIndex)) {
                _location = location;
            } else {
                _location = location;
                OnGridIndexChange.Invoke(); 
            }
        }

        public void NavigateTo(Vector3Int targetGridPos, MovementSpeed speed)
        {
            WorldScheduler.Instance.TryGetLocation(WorldObjectId, out WorldLocation previous, out WorldLocation next, out float percent);
            NavStep lastStep = new NavStep(MoveType.NONE, previous.GridPosition, FacingUtil.GetFacing(previous.RotationAngle), WorldScheduler.Instance.Time);
            WorldUtil.FindShortestPathAsync(lastStep, targetGridPos, speed, (path) => { UpdateGoalPath(path); });
        }

        private void UpdateGoalPath(List<NavStep> path)
        {
            WorldScheduler.Instance.TryGetLocation(WorldObjectId, out WorldLocation previous, out WorldLocation next, out float percent);
            WorldLocation currentLocation = WorldLocation.Lerp(previous, next, percent);
            WorldScheduler.Instance.Unregister(WorldObjectId);
            WorldScheduler.Instance.RegisterIn(0, WorldObjectId, currentLocation);
            if (path == null) { return; }
            foreach (NavStep step in path) {
                Debug.Log("Register " + WorldObjectId);
                WorldScheduler.Instance.RegisterAt(step.WorldTime, WorldObjectId, new WorldLocation(step.GridPos, step.Facing));
            }
        }

        public override string ToString()
        {
            return $"Position:{Current.GridPosition}, Facing:{Current.Facing}, Speed:{Speed}";
        }
    }
}