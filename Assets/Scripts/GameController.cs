using UnityEngine;
using Zekzek.CameraControl;
using Zekzek.HexWorld;

public class GameController : MonoBehaviour
{
    private void Start()
    {
        GenerationUtil.Init(42, 5, TerrainType.Desert, TerrainType.Forest, TerrainType.Hills);
        
        WorldObject player = GenerationUtil.InstantiateEntity(new MovementSpeed(5, 2, 2, 2, 5, 2, 5, 2, 1), Vector2Int.zero);
        StatComponent playerStats = new StatComponent(player.Id);
        playerStats.StatBlock.AddAmount(Zekzek.Stats.StatType.Health, 100);
        player.AddComponent(playerStats);

        WorldObject enemy = GenerationUtil.InstantiateEntity(new MovementSpeed(5, 2, 2, 2, 5, 2, 5, 2, 1), Vector2Int.one);
        StatComponent enemyStats = new StatComponent(enemy.Id);
        enemyStats.StatBlock.AddAmount(Zekzek.Stats.StatType.Health, 100);
        enemy.AddComponent(enemyStats);

        CameraController<LocationComponent>.Main.AddTarget(player.Location);
    }
}
