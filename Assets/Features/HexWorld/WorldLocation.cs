using System.Collections.Generic;
using UnityEngine;

namespace Zekzek.HexWorld
{
    public class WorldLocation
    {
        public uint WorldObjectId { get; private set; }

        private Vector3 position;
        public virtual Vector3 Position {
            get => position;
            set {
                position = value;
                gridPosition = null;
                gridIndex = null;
            }
        }

        private Vector3Int? gridPosition;
        public Vector3Int GridPosition {
            get {
                if (!gridPosition.HasValue) { gridPosition = WorldUtil.PositionToGridPos(position); }
                return gridPosition.Value;
            }
        }

        private Vector2Int? gridIndex;
        public Vector2Int GridIndex {
            get {
                if (!gridIndex.HasValue) { gridIndex = WorldUtil.PositionToGridIndex(position); }
                return gridIndex.Value;
            }
        }

        private float rotationAngle;
        public float RotationAngle {
            get => rotationAngle;
            set {
                rotationAngle = value;
                facing = null;
            }
        }

        private Vector2Int? facing;
        public Vector2Int Facing {
            get {
                if (!facing.HasValue) { facing = FacingUtil.GetFacing(rotationAngle); }
                return facing.Value;
            }
        }

        public WorldLocation(uint worldObjectId, Vector3 position, float rotationAngle)
        {
            WorldObjectId = worldObjectId;
            Position = position;
            RotationAngle = rotationAngle;
        }

        public void AddToWorld()
        {
            WorldScheduler.Instance.RegisterIn(0, this);
            //HexWorld.Instance.Add(this);
        }

        public void RemoveFromWorld()
        {
            WorldScheduler.Instance.Unregister(WorldObjectId);
            //HexWorld.Instance.Remove(this);
        }

        public static WorldLocation Lerp(WorldLocation previous, WorldLocation next, float percentComplete)
        {
            if (previous == null || next == null) { return previous; }
            Vector3 position = Vector3.Lerp(previous.Position, next.Position, percentComplete);
            float rotationAngle = FacingUtil.LerpRotationAroundUpAxis(previous.rotationAngle, next.rotationAngle, percentComplete);
            return new WorldLocation(previous.WorldObjectId, position, rotationAngle);
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
            WorldLocation currentLocation = Lerp(previous, next, percent);
            WorldScheduler.Instance.Unregister(WorldObjectId);
            WorldScheduler.Instance.RegisterIn(0, currentLocation);
            if (path == null) { return; }
            foreach (NavStep step in path) {
                WorldScheduler.Instance.RegisterAt(step.WorldTime, new WorldLocation(WorldObjectId, WorldUtil.GridPosToPosition(step.GridPos), FacingUtil.GetRotationAroundUpAxis(step.Facing)));
            }
        }
    }
}