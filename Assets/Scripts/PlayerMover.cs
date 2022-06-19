using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    private void FixedUpdate()
    {
        Vector3 input = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) { input += transform.forward * 5; }
        if (Input.GetKey(KeyCode.A)) { input += transform.right * -5; }
        if (Input.GetKey(KeyCode.S)) { input += transform.forward * -5; }
        if (Input.GetKey(KeyCode.D)) { input += transform.right * 5; }

        transform.localPosition += input * Time.deltaTime;
    }
}
