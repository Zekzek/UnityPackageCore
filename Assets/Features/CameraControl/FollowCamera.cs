using System.Collections.Generic;
using UnityEngine;

namespace Zekzek.CameraControl
{
    public class FollowCamera<T> : MonoBehaviour
    {
        public Camera Camera { get; private set; }
        public string Key => key;

        public int Priority { get; set; } = 1;
        public Vector3 TargetPosition { get; private set; }

        [SerializeField] private List<T> targets = new List<T>();
        [SerializeField] private Vector3 targetOffset;
        [SerializeField] private string key = "default";
        [SerializeField] private float followSpeed = 1;

        private new Rigidbody rigidbody;
        private Vector3 idealPosition;

        public void AddTarget(T target)
        {
            if (!targets.Contains(target)) {
                targets.Add(target);
            }
        }

        public bool RemoveTarget(T target)
        {
            return targets.Remove(target);
        }

        public bool TryChangeKey(string value)
        {
            if (!CameraController<T>.ContainsKey(value)) {
                CameraController<T>.Unregister(key);
                key = value;
                CameraController<T>.Register(key, this);
                return true;
            }
            return false;
        }

        public void RotateHorizontal(float degrees)
        {
            Quaternion rotation = Quaternion.AngleAxis(degrees, Vector3.up);
            targetOffset = rotation * targetOffset;
        }

        public void RotateVertical(float degrees) 
        {
            if (degrees > 0) {
                targetOffset = Vector3.RotateTowards(targetOffset, Vector3.up, degrees * Mathf.Deg2Rad, 0);
            } else if (degrees < 0) {
                targetOffset = Vector3.RotateTowards(targetOffset, Vector3.down, -degrees * Mathf.Deg2Rad, 0);
            }
        } 

        private void Awake()
        {
            CameraController<T>.Register(Key, this);
            rigidbody = GetComponent<Rigidbody>();
            Camera = GetComponent<Camera>();
        }

        private void OnDestroy()
        {
            CameraController<T>.Unregister(Key);
        }

        private void FixedUpdate()
        {
            UpdateTargetPosition();
            UpdateGoalPosition();

            Vector3 deltaPosition = 0.2f * (idealPosition - transform.position);
            Vector3 deltaSquared = new Vector3(
                deltaPosition.x * deltaPosition.x * deltaPosition.x,
                deltaPosition.y * deltaPosition.y * deltaPosition.y, 
                deltaPosition.z * deltaPosition.z * deltaPosition.z);
            rigidbody.AddForce(Time.deltaTime * 1000f * deltaSquared * followSpeed);

            var targetRotation = Quaternion.LookRotation(-targetOffset); 
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * followSpeed);

            //Vector2 rotateAmount = InputManager.Instance.Get<Vector2>(InputManager.PlayerAction.Rotate);
            //RotateVertical(rotateAmount.y);
            //RotateHorizontal(rotateAmount.x);
        }

        private void UpdateTargetPosition()
        {
            int count = 0;
            Vector3 targetPositionSum = Vector3.zero;
            foreach (T target in targets) {
                if (target != null) {
                    count++;
                    targetPositionSum += GetPosition(target);
                }
            }

            TargetPosition = count == 0 ? Vector3.zero : targetPositionSum / count;
        }

        private void UpdateGoalPosition()
        {
            idealPosition = TargetPosition + targetOffset;
        }

        protected virtual Vector3 GetPosition(T target)
        {
            if (target is Component component) {
                return component.transform.position;
            } else {
                return Vector3.zero;
            }
        }
    }
}