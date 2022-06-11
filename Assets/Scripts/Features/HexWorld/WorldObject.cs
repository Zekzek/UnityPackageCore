using Newtonsoft.Json;
using UnityEngine;

namespace Zekzek.HexWorld
{
    public abstract class WorldObject
    {
        public readonly uint Id;
        public WorldLocation Location { get; protected set; }
        public MovementSpeed Speed { get; protected set; }


        public WorldObject(Vector3? location, float? rotationAngle)
        {
            Id = World.Instance.NextId;
            if (location.HasValue && rotationAngle.HasValue) {
                Location = new WorldLocation(Id, location.Value, rotationAngle.Value);
                Location.AddToWorld();
            }
            World.Instance.Add(this);
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