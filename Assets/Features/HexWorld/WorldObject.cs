using UnityEngine;

namespace Zekzek.HexWorld
{
    public class WorldObject
    {
        public readonly uint Id;
        public WorldLocation Location { get; protected set; }
        public MovementSpeed Speed { get; protected set; }


        public WorldObject(Vector3? location, float? rotationAngle)
        {
            Id = HexWorld.Instance.NextId;
            if (location.HasValue && rotationAngle.HasValue) {
                Location = new WorldLocation(Id, location.Value, rotationAngle.Value);
                Location.AddToWorld();
            }
            HexWorld.Instance.worldObjects.Add(Id, Location.GridIndex, this);
            Speed = new MovementSpeed(1, 1, 1, 1, 1, 1, 1, 1, 1);
        }

        public override string ToString()
        {
            return $"{GetType()}\nId: {Id}\nLocation: {Location.GridPosition}\nSpeed: {Speed}";
        }

        public virtual string ToSummaryString()
        {
            return $"{GetType()} #{Id} @ {Location.GridPosition}";
        }
    }
}