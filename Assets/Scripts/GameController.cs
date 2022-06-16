using UnityEngine;
using Zekzek.CameraControl;
using Zekzek.HexWorld;

public class GameController : MonoBehaviour
{
    private bool following = false;

    private void Start()
    {
        var tile = new HexTile(0,0,0);
        World.Instance.Add(tile);
    }

    private void Update()
    {
        if (!following) {
            GameObject target = GameObject.Find("HexTile: (0, 0)");
            if (target != null) {
                CameraController<Transform>.Priority.AddTarget(target.transform);
                following = true;
            }
        }



        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            CameraController<Transform>.Priority.RotateVertical(10);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            CameraController<Transform>.Priority.RotateVertical(-10);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            CameraController<Transform>.Priority.RotateHorizontal(-10);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            CameraController<Transform>.Priority.RotateHorizontal(10);
        }
    }

    private void FixedUpdate()
    {
        CameraController<Transform>.Priority.RotateHorizontal(0.5f);
    }
}
