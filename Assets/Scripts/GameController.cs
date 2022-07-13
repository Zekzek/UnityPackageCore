using UnityEngine;
using Zekzek.CameraControl;
using Zekzek.HexWorld;

public class GameController : MonoBehaviour
{
    private bool following = false;

    private void Start()
    {
        GenerationUtil.InitRegions(42, 10, TerrainType.Hills);
        foreach (Vector2Int index in WorldUtil.GetBurstIndicesAround(new Vector2Int(0, 0), 100, true)) {
            GenerationUtil.InstantiateTile(index.x, index.y);
        }
        GenerationUtil.InstantiateEntity(new MovementSpeed(1, 1, 1, 1, 1, 1, 1, 1, 1), Vector2Int.zero);
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
