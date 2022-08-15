using UnityEngine;
using Zekzek.Ability;
using Zekzek.CameraControl;
using Zekzek.HexWorld;
using Zekzek.Stats;

public class GameController : MonoBehaviour
{
    private void Start()
    {
        GenerationUtil.Init(42, 5, TerrainType.Desert, TerrainType.Forest, TerrainType.Hills);
        WorldObject player = CreatePlayer();
        CameraController<LocationComponent>.Main.AddTarget(player.Location);
    }

    private WorldObject CreatePlayer()
    {
        WorldObject player = GenerationUtil.InstantiateEntity(new MovementSpeed(2, 1, 1, 0.5f, 2, 2, 2, 2, 1), Vector2Int.zero);

        StatComponent playerStats = new StatComponent(player.Id);
        playerStats.StatBlock.AddAmount(StatType.Health, 100);
        player.AddComponent(playerStats);

        AbilityComponent playerAbilities = new AbilityComponent(player.Id);
        playerAbilities.Add("jab");
        playerAbilities.Add("lunge");
        playerAbilities.Add("gore");
        player.AddComponent(playerAbilities);
        CombatCanvas.Set(playerAbilities);

        return player;
    }
}
