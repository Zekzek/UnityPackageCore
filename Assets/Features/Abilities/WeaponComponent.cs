
using Zekzek.HexWorld;

namespace Zekzek.Ability
{
    public class WeaponComponent : WorldObjectComponent
    {
        public override WorldComponentType ComponentType => WorldComponentType.Weapon;
        public WeaponData WeaponData { get; private set; }

        public WeaponComponent(uint worldObjectId) : base(worldObjectId) { }

        public void Add(string weaponId)
        {
            WeaponData = JsonContent.ContentUtil.LoadData<WeaponData>(weaponId);
        }
    }
}