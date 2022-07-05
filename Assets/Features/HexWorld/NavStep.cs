using UnityEngine;

namespace Zekzek.HexWorld
{
    public class NavStep
    {
        public static NavStep ZERO = new NavStep(MoveType.NONE, new WorldLocation(Vector3.zero, 0), 0);

        public readonly MoveType MoveType;
        public readonly WorldLocation Location;
        public readonly float WorldTime;

        public NavStep(MoveType moveType, WorldLocation location, float worldTime)
        {
            MoveType = moveType;
            Location = location;
            WorldTime = worldTime;
        }

        public override string ToString()
        {
            return "Type:" + MoveType + ", Location:" + Location;
        }

        public static NavStep GetInverse(NavStep from, NavStep to)
        {
            float time = Mathf.Max(from.WorldTime, to.WorldTime) + Mathf.Abs(from.WorldTime - to.WorldTime);
            return new NavStep(to.MoveType.GetInverse(), from.Location, time);
        }

        public bool EqualsIgnoreType(NavStep step)
        {
            return Location.Equals(step.Location);
        }

        public override bool Equals(object obj)
        {
            return obj is NavStep step &&
                   MoveType == step.MoveType &&
                   Location.Equals(step.Location);
        }

        public override int GetHashCode()
        {
            int hashCode = -1677463178;
            hashCode = hashCode * -1521134295 + MoveType.GetHashCode();
            hashCode = hashCode * -1521134295 + Location.GetHashCode();
            return hashCode;
        }
    }
}