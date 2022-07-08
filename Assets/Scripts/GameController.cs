using UnityEngine;
using Zekzek.CameraControl;
using Zekzek.HexWorld;

public class GameController : MonoBehaviour
{
    private bool following = false;

    private void Start()
    {
        GenerationUtil.CreateTile(Vector3Int.zero);
        GenerationUtil.CreateRandomTilesAround(Vector2Int.zero);
        foreach (Vector2Int index in WorldUtil.GetBurstIndicesAround(new Vector2Int(0, 0), 1, false)) {
            GenerationUtil.CreateRandomTilesAround(index);
        }
        GenerationUtil.CreateEntity(new MovementSpeed(1, 1, 1, 1, 1, 1, 1, 1, 1), Vector3Int.zero);
    }

    private void Update()
    {
        if (!following) {
            GameObject target = GameObject.Find("HexTile: (0, 0)");
            if (target != null) {
                //CameraController<Transform>.Priority.AddTarget(target.transform);
                following = true;
            }
        }
    }
}
