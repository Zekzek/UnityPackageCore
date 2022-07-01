using UnityEngine;
using Zekzek.CameraControl;
using Zekzek.HexWorld;

public class GameController : MonoBehaviour
{
    private bool following = false;

    private void Start()
    {
        foreach(Vector2Int index in WorldUtil.GetBurstIndicesAround(new Vector2Int(0, 0), 3, true)) {
            HexTile tile = new HexTile(index.x, 0, index.y);
            HexWorld.Instance.tiles.Add(tile.Id, tile.Location.GridIndex, tile);
        }

        WorldObject player = new WorldObject(Vector3.zero, 0);
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
