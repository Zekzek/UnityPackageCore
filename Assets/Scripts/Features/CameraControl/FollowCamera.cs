using System.Collections.Generic;
using UnityEngine;

namespace Zekzek.CameraControl
{
    public class FollowCamera<T> : MonoBehaviour
    {
        public bool TrackCenter;
        public Camera Camera { get; private set; }
        public string Key => key;

        public int Priority { get; set; } = 1;

        [SerializeField] private List<T> targets = new List<T>();
        [SerializeField] private Vector3 targetOffset;
        [SerializeField] private string key = "default";

        private new Rigidbody rigidbody;

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

        private void Start()
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
            Vector3 targetPosition = CalcTargetPosition();
            Vector3 deltaPosition = (targetPosition - transform.position) - 0.5f * rigidbody.velocity;
            rigidbody.AddForce(Time.deltaTime * 5000f * deltaPosition);
        }

        private Vector3 CalcTargetPosition()
        {
            int count = 0;
            Vector3 targetPositionSum = Vector3.zero;
            foreach (T target in targets) {
                if (target != null) {
                    count++;
                    targetPositionSum += GetPosition(target);
                }
            }

            return count == 0 ? Vector3.zero : targetPositionSum / count + targetOffset;
        }

        private Vector3 GetPosition(T target)
        {
            if (target is Component component) {
                return component.transform.position;
            } else {
                return Vector3.zero;
            }
        }
    }
}