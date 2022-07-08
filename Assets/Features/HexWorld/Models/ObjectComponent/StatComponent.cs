namespace Zekzek.HexWorld
{
    public class StatComponent : WorldObjectComponent
    {
        public override WorldComponentType ComponentType => WorldComponentType.Stats;

        public StatComponent(uint worldObjectId) : base(worldObjectId)
        {
        }
    }
}