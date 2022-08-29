using UnityEngine;
using Zekzek.Ability;
using Zekzek.CameraControl;
using Zekzek.HexWorld;

public class GameController : MonoBehaviour
{
    private void Start()
    {
        GenerationUtil.Init(42, 5, TerrainType.Desert, TerrainType.Forest, TerrainType.Hills);
        WorldObject player = CreatePlayer();
        CameraController<LocationComponent>.Main.AddTarget(player.Location);
        InputManager.Instance.PushMode(InputManager.InputMode.WorldNavigation);
    }

    private WorldObject CreatePlayer()
    {
        WorldObject player = GenerationUtil.InstantiateEntity(new MovementSpeed(2, 1, 1, 0.5f, 2, 2, 2, 2, 1), Vector2Int.zero);

        AbilityComponent playerAbilities = new AbilityComponent(player.Id);
        playerAbilities.Add("jab");
        playerAbilities.Add("lunge");
        playerAbilities.Add("fire");
        playerAbilities.Add("ice");
        playerAbilities.Add("water");
        playerAbilities.Add("lightning");
        playerAbilities.Add("earth");
        playerAbilities.Add("wind");
        player.AddComponent(playerAbilities);

        WeaponComponent playerWeapon = new WeaponComponent(player.Id);
        playerWeapon.Add("dagger");
        player.AddComponent(playerWeapon);

        return player;
    }
}
