using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    private void FixedUpdate()
    {
        Vector2 rotateAmount = InputManager.Instance.Get<Vector2>(InputManager.PlayerAction.Move);

        transform.localPosition += (Vector3) rotateAmount * 5 * Time.deltaTime;
    }
}