using UnityEngine;
using Zekzek.HexWorld;

public class GameController : MonoBehaviour
{
    private void Start()
    {
        GenerationUtil.Init(42, 5, TerrainType.Desert, TerrainType.Forest, TerrainType.Hills);
        GenerationUtil.InstantiateEntity(new MovementSpeed(1, 1, 1, 1, 1, 1, 1, 1, 1), Vector2Int.zero);
    }
}
