using System.Collections.Generic;
using System.Text;
using Zekzek.Combat;

namespace Zekzek.HexWorld
{
    public class WorldObject
    {
        public readonly uint Id;
        public AbilityComponent Ability => GetComponent(WorldComponentType.Ability) as AbilityComponent;
        public DisplayComponent Display => GetComponent(WorldComponentType.Display) as DisplayComponent;
        public LocationComponent Location => GetComponent(WorldComponentType.Location) as LocationComponent;
        public PlatformComponent Platform => GetComponent(WorldComponentType.Platform) as PlatformComponent;
        public StatComponent Stats => GetComponent(WorldComponentType.Stats) as StatComponent;
        
        public WorldObjectType Type { get; private set; }

        private Dictionary<WorldComponentType, WorldObjectComponent> _components = new Dictionary<WorldComponentType, WorldObjectComponent>();

        public WorldObject(WorldObjectType type) {
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
    }
}