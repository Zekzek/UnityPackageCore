namespace Zekzek.HexWorld
{
    public class DisplayComponent : WorldObjectComponent
    {
        public override WorldComponentType ComponentType => WorldComponentType.Display;

        public float Quantity { get; private set; }
        public float Color { get; private set; }

        public DisplayComponent(uint worldObjectId, float quantity, float color) : base(worldObjectId) 
        {
            Quantity = quantity;
            Color = color;
        }
    }
}