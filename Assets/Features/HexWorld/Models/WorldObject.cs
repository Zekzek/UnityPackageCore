using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Zekzek.HexWorld
{
    public class WorldObject
    {
        public readonly uint Id;
        public LocationComponent Location => (LocationComponent)GetComponent(WorldComponentType.Location);
        public MoveComponent Moveable => (MoveComponent)GetComponent(WorldComponentType.Moveable);
        public WorldObjectType Type { get; private set; }

        private Dictionary<WorldComponentType, WorldObjectComponent> _components = new Dictionary<WorldComponentType, WorldObjectComponent>();

        private WorldObject(WorldObjectType type) {
            Id = HexWorld.Instance.NextId;
            Type = type;
        }

        public void AddComponent(WorldObjectComponent component) { _components.Add(component.ComponentType, component); }
        public void RemoveComponent(WorldObjectComponent component) { _components.Remove(component.ComponentType); }
        public bool HasComponent(WorldComponentType type) { return _components.ContainsKey(type); }
        public WorldObjectComponent GetComponent(WorldComponentType type) { return HasComponent(type) ? _components[type] : null; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"Id: {Id}");
            foreach(WorldComponentType key in _components.Keys) { builder.Append($"{key}: {_components[key]}"); }
            return builder.ToString();
        }

        public static WorldObject CreateTile(Vector3Int gridPosition, float rotationAngle = 0)
        {
            WorldObject instance = new WorldObject(WorldObjectType.Tile);
            instance.AddComponent(new LocationComponent(instance.Id, gridPosition, rotationAngle));
            instance.AddComponent(new PlatformComponent(instance.Id));
            instance.AddComponent(new TargetableComponent(instance.Id));
            HexWorld.Instance.Add(instance);
            return instance;
        }

        public static WorldObject CreateEntity(MovementSpeed speed, Vector3Int gridPosition, float rotationAngle = 0)
        {
            WorldObject instance = new WorldObject(WorldObjectType.Entity);
            instance.AddComponent(new LocationComponent(instance.Id, gridPosition, rotationAngle));
            instance.AddComponent(new MoveComponent(instance.Id, speed));
            HexWorld.Instance.Add(instance);
            return instance;
        }
    }
}