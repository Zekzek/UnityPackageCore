using UnityEngine;
using Zekzek.CameraControl;
using Zekzek.HexWorld;

public class GameController : MonoBehaviour
{
    private bool following = false;

    private void Start()
    {
        foreach(Vector2Int index in WorldUtil.GetBurstIndicesAround(new Vector2Int(0, 0), 3, true)) {
            WorldObject.CreateTile(new Vector3Int(index.x, 0, index.y));
        }
        WorldObject.CreateEntity(new MovementSpeed(1, 1, 1, 1, 1, 1, 1, 1, 1), Vector3Int.zero);
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
