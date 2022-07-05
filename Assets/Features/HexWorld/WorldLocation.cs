using UnityEngine;

namespace Zekzek.HexWorld
{
    // Immutable
    public class WorldLocation
    {
        public const int MAX_HEIGHT = (1 << 4) - 1;
        public const int MAX_POS = (1 << 10) - 1;

        public virtual Vector3 Position { get; private set; }

        private Vector3Int? gridPosition;
        public Vector3Int GridPosition {
            get {
                if (!gridPosition.HasValue) { gridPosition = WorldUtil.PositionToGridPos(Position); }
                return gridPosition.Value;
            }
        }

        private Vector2Int? gridIndex;
        public Vector2Int GridIndex {
            get {
                if (!gridIndex.HasValue) { gridIndex = WorldUtil.PositionToGridIndex(Position); }
                return gridIndex.Value;
            }
        }

        public int GridHeight => GridPosition.y;

        public float RotationAngle { get; private set; }

        private Vector2Int? facing;
        public Vector2Int Facing {
            get {
                if (!facing.HasValue) { facing = FacingUtil.GetFacing(RotationAngle); }
                return facing.Value;
            }
        }

        //Note: Vector3Int will silently convert to Vector3 if a perfect match is not present
        public WorldLocation(Vector3 position, float rotationAngle)
        {
            Position = position;
            RotationAngle = rotationAngle;
        }

        public WorldLocation(Vector3 position, Vector2Int facing)
        {
            Position = position;
            RotationAngle = FacingUtil.GetRotationAroundUpAxis(facing);
        }

        public WorldLocation(Vector3Int gridPosition, float rotationAngle)
        {
            Position = WorldUtil.GridPosToPosition(gridPosition);
            RotationAngle = rotationAngle;
        }

        public WorldLocation(Vector3Int gridPosition, Vector2Int facing)
        {
            Position = WorldUtil.GridPosToPosition(gridPosition);
            RotationAngle = FacingUtil.GetRotationAroundUpAxis(facing);
        }

        public static WorldLocation Lerp(WorldLocation previous, WorldLocation next, float percentComplete)
        {
            if (previous == null || next == null) { return previous; }
            Vector3 position = Vector3.Lerp(previous.Position, next.Position, percentComplete);
            float rotationAngle = FacingUtil.LerpRotationAroundUpAxis(previous.RotationAngle, next.RotationAngle, percentComplete);
            return new WorldLocation(position, rotationAngle);
        }

        public WorldLocation MoveForward(int amount)
        {
            return new WorldLocation(GridPosition + amount * new Vector3Int(Facing.x, 0, Facing.y), Facing);
        }

        public WorldLocation MoveBack(int amount)
        {
            return new WorldLocation(GridPosition - amount * new Vector3Int(Facing.x, 0, Facing.y), Facing);
        }

        public WorldLocation MoveUp(int amount)
        {
            return new WorldLocation(GridPosition + amount * Vector3Int.up, Facing);
        }

        public WorldLocation MoveDown(int amount)
        {
            return new WorldLocation(GridPosition + amount * Vector3Int.down, Facing);
        }

        public WorldLocation MoveHeightTo(int height)
        {
            return new WorldLocation(new Vector3(GridPosition.x, height, GridPosition.z), Facing);
        }

        public WorldLocation RotateLeft()
        {
            return new WorldLocation(Position, FacingUtil.GetLeft(Facing));
        }

        public WorldLocation RotateRight()
        {
            return new WorldLocation(Position, FacingUtil.GetRight(Facing));
        }

    }
}