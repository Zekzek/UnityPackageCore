using System.Collections.Generic;

namespace Zekzek.CameraControl
{
    public static class CameraController<T>
    {
        private static readonly Dictionary<string, FollowCamera<T>> cameras = new Dictionary<string, FollowCamera<T>>();

        public static void Register(string key, FollowCamera<T> camera)
        {
            cameras[key] = camera;
        }

        public static void Unregister(string key)
        {
            if (cameras.ContainsKey(key)) {
                cameras.Remove(key);
            }
        }

        public static bool ContainsKey(string key)
        {
            return cameras.ContainsKey(key);
        }

        public static void Clear()
        {
            cameras.Clear();
        }

        public static FollowCamera<T> Main {
            get {
                int highestPriority = int.MaxValue;
                FollowCamera<T> priorityCamera = null;
                foreach (FollowCamera<T> camera in cameras.Values) {
                    if (camera.Priority < highestPriority) {
                        highestPriority = camera.Priority;
                        priorityCamera = camera;
                    }
                }
                return priorityCamera;
            }
        }
    }
}