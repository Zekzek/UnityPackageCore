using UnityEngine;
using Zekzek.CameraControl;
using Zekzek.HexWorld;
using Zekzek.Stats;

public class GameController : MonoBehaviour
{
    private void Start()
    {
        GenerationUtil.Init(42, 5, TerrainType.Desert, TerrainType.Forest, TerrainType.Hills);
        
        WorldObject player = GenerationUtil.InstantiateEntity(new MovementSpeed(2, 1, 1, 0.5f, 2, 2, 2, 2, 1), Vector2Int.zero);
        StatComponent playerStats = new StatComponent(player.Id);
        playerStats.StatBlock.AddAmount(Zekzek.Stats.StatType.Health, 100);
        player.AddComponent(playerStats);

        CameraController<LocationComponent>.Main.AddTarget(player.Location);
    }
}
