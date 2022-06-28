using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMover : MonoBehaviour
{
    private void FixedUpdate()
    {
        Vector2 rotateAmount = InputManager.Instance.Get("Move").ReadValue<Vector2>();

        transform.localPosition += (Vector3) rotateAmount * 5 * Time.deltaTime;


        //Vector3 input = Vector3.zero;
        //
        //if (Input.GetKey(KeyCode.W)) { input += transform.forward * 5; }
        //if (Input.GetKey(KeyCode.A)) { input += transform.right * -5; }
        //if (Input.GetKey(KeyCode.S)) { input += transform.forward * -5; }
        //if (Input.GetKey(KeyCode.D)) { input += transform.right * 5; }
        //
        //transform.localPosition += input * Time.deltaTime;
    }
}
