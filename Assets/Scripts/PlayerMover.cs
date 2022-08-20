using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMover : MonoBehaviour
{
    private void Start()
    {
        //InputManager.Instance.AddListener(InputManager.PlayerAction.Move, OnMove);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        transform.localPosition += (Vector3)input * 5 * Time.deltaTime;
    }
}