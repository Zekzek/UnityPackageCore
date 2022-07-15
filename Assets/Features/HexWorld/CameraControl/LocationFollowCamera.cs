using UnityEngine;
using Zekzek.HexWorld;

namespace Zekzek.CameraControl
{
    public class LocationFollowCamera : FollowCamera<LocationComponent> {
        protected override Vector3 GetPosition(LocationComponent target)
        {
            return target.Position;
        }
    }
}