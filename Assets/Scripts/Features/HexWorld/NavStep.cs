using UnityEngine;

namespace Zekzek.HexWorld
{
    public class NavStep
    {
        public static NavStep ZERO = new NavStep(MoveType.NONE, Vector3Int.zero, Vector2Int.zero, 0);

        public readonly MoveType MoveType;
        public readonly Vector3Int GridPos;
        public readonly Vector2Int Facing;
        public readonly Vector2Int GridIndex;
        public readonly int Height;
        public readonly float WorldTime;

        public NavStep(MoveType moveType, Vector3Int gridPos, Vector2Int facing, float worldTime)
        {
            MoveType = moveType;
            GridPos = gridPos;
            Facing = facing;
            GridIndex = WorldUtil.GridPosToGridIndex(gridPos);
            Height = gridPos.y;
            WorldTime = worldTime;
        }

        public NavStep(MoveType moveType, Vector2Int gridIndex, int height, Vector2Int facing, float delay = 0) : this(moveType, new Vector3Int(gridIndex.x, height, gridIndex.y), facing, delay) { }

        public override string ToString()
        {
            return "Type:" + MoveType + ", Grid Index:" + GridIndex + ", Height:" + Height + ", Facing:" + Facing;
        }

        public static NavStep GetInverse(NavStep from, NavStep to)
        {
            float time = Mathf.Max(from.WorldTime, to.WorldTime) + Mathf.Abs(from.WorldTime - to.WorldTime);
            return new NavStep(to.MoveType.GetInverse(), from.GridPos, from.Facing, time);
        }

        public bool EqualsIgnoreType(NavStep step)
        {
            return GridPos.Equals(step.GridPos) && Facing.Equals(step.Facing);
        }

        public override bool Equals(object obj)
        {
            return obj is NavStep step &&
                   MoveType == step.MoveType &&
                   GridPos.Equals(step.GridPos) &&
                   Facing.Equals(step.Facing);
        }

        public override int GetHashCode()
        {
            int hashCode = -1677463178;
            hashCode = hashCode * -1521134295 + MoveType.GetHashCode();
            hashCode = hashCode * -1521134295 + GridPos.GetHashCode();
            hashCode = hashCode * -1521134295 + Facing.GetHashCode();
            return hashCode;
        }
    }
}