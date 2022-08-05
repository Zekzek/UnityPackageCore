namespace Zekzek.HexWorld
{
    public abstract class WorldObjectComponent
    {
        public abstract WorldComponentType ComponentType { get; }
        public uint WorldObjectId { get; private set; }
        private WorldObject _worldObject;

        protected WorldObjectComponent(uint worldObjectId) { Attach(worldObjectId); }

        protected WorldObjectComponent GetSibling(WorldComponentType siblingType)
        {
            return _worldObject?.GetComponent(siblingType);
        }

        public bool Attach(uint worldObjectId)
        {
            if (WorldObjectId == 0) { 
                WorldObjectId = worldObjectId; 
                _worldObject = HexWorld.Instance.Get(worldObjectId);
                return true;
            }
            return false;
        }
    }
}