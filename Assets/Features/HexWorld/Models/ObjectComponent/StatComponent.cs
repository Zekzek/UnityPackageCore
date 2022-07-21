using Zekzek.Stats;

namespace Zekzek.HexWorld
{
    public class StatComponent : WorldObjectComponent
    {
        public override WorldComponentType ComponentType => WorldComponentType.Stats;

        public StatBlock StatBlock { get; private set; } = new StatBlock();
        public StatComponent(uint worldObjectId) : base(worldObjectId) { }
    }
}