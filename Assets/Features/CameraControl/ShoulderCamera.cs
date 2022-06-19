using System.Collections.Generic;
using UnityEngine;

namespace Zekzek.CameraControl
{
    public class ShoulderCamera : MonoBehaviour
    {
        public static ShoulderCamera Instance { get; private set; }

        public Camera Camera { get; private set; }

        [SerializeField] private Transform target;
        [SerializeField] private Vector3 targetOffset;
        [SerializeField] private Vector3 shoulderOffset;

        private new Rigidbody rigidbody;
        private Vector3 idealPosition;
        private Vector3 targetPosition;

        public void RotateHorizontal(float degrees)
        {
            Quaternion rotation = Quaternion.AngleAxis(degrees, Vector3.up);
            targetOffset = rotation * targetOffset;
            target.rotation = target.rotation * rotation;
        }

        public void RotateVertical(float degrees)
        {
            if (degrees > 0) {
                targetOffset = Vector3.RotateTowards(targetOffset, Vector3.up, degrees * Mathf.Deg2Rad, 0);
            } else if (degrees < 0) {
                targetOffset = Vector3.RotateTowards(targetOffset, Vector3.down, -degrees * Mathf.Deg2Rad, 0);
            }
        }

        public void Recenter()
        {
            targetOffset = new Vector3(-2, 2, -10);
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            rigidbody = GetComponent<Rigidbody>();
            Camera = GetComponent<Camera>();
        }

        private void FixedUpdate()
        {
            UpdateTargetPosition();
            UpdateGoalPosition();

            Vector3 deltaPosition = idealPosition - (transform.position + 0.5f * rigidbody.velocity);
            if (deltaPosition.sqrMagnitude > 1) { deltaPosition.Normalize(); }
            rigidbody.AddForce(Time.deltaTime * 5000f * deltaPosition);

            transform.LookAt(targetPosition + shoulderOffset);

            if (Input.GetKey(KeyCode.UpArrow)) { RotateVertical(1f); }
            if (Input.GetKey(KeyCode.LeftArrow)) { RotateHorizontal(-1f); }
            if (Input.GetKey(KeyCode.DownArrow)) { RotateVertical(-1f); }
            if (Input.GetKey(KeyCode.RightArrow)) { RotateHorizontal(1f); }
            if (Input.GetKey(KeyCode.Space)) { Recenter(); }
        }

        private void UpdateTargetPosition()
        {
            targetPosition = target.position;
        }

        private void UpdateGoalPosition()
        {
            idealPosition = targetPosition + targetOffset + shoulderOffset;
        }
    }
}