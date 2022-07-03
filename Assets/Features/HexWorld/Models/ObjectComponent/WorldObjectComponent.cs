namespace Zekzek.HexWorld
{
    public abstract class WorldObjectComponent
    {
        public abstract WorldComponentType ComponentType { get; }
        public uint WorldObjectId { get; }

        protected readonly WorldObject _worldObject;

        protected WorldObjectComponent(uint worldObjectId) {
            WorldObjectId = worldObjectId;
            _worldObject = HexWorld.Instance.Get(worldObjectId);
        }

        protected WorldObjectComponent GetSibling(WorldComponentType siblingType)
        {
            return HexWorld.Instance.Get(WorldObjectId)?.GetComponent(siblingType);
        }
    }
}