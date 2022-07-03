using System;
using UnityEngine;

namespace Zekzek.HexWorld
{
    public class LocationComponent : WorldObjectComponent
    {
        public Action OnGridIndexChange; 

        public override WorldComponentType ComponentType => WorldComponentType.Location;
        public WorldLocation Current => _location;

        public virtual Vector3 Position {
            get => _location.Position;
            set { UpdateLocation(new WorldLocation(value, _location?.RotationAngle ?? 0f)); }
        }

        public Vector3Int GridPosition {
            get => _location.GridPosition;
            set { UpdateLocation(new WorldLocation(value, _location?.Facing ?? Vector2Int.right)); }
        }

        public Vector2Int GridIndex {
            get => _location.GridIndex;
            set { UpdateLocation(new WorldLocation(WorldUtil.GridIndexToPosition(value, _location?.GridPosition.y ?? 0), _location?.RotationAngle ?? 0f)); }
        }

        public float RotationAngle {
            get => _location.RotationAngle;
            set { UpdateLocation(new WorldLocation(_location?.Position ?? Vector3.zero, value)); }
        }

        public Vector2Int Facing {
            get => _location.Facing;
            set { UpdateLocation(new WorldLocation(_location?.Position ?? Vector3.zero, value)); }
        }

        private WorldLocation _location;

        public LocationComponent(uint worldObjectId, Vector3Int gridPosition, float rotationAngle = 0) : base(worldObjectId)
        {
            _location = new WorldLocation(gridPosition, rotationAngle);
        }

        public LocationComponent(uint worldObjectId, Vector3Int gridPosition, Vector2Int facing) : base(worldObjectId)
        {
            _location = new WorldLocation(gridPosition, facing);
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

        public void AddToWorld()
        {
            WorldScheduler.Instance.RegisterIn(0, WorldObjectId, Current);
            //HexWorld.Instance.Add(this);
        }

        public void RemoveFromWorld()
        {
            WorldScheduler.Instance.Unregister(WorldObjectId);
            //HexWorld.Instance.Remove(this);
        }

        public override string ToString()
        {
            return $"Position:{GridPosition}, Facing{Facing}";
        }
    }
}