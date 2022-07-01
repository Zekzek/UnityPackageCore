using UnityEngine;

namespace Zekzek.HexWorld
{
    public class WorldObjectBehaviour : MonoBehaviour
    {
        public WorldObject Model { get; set; }

        private void Update()
        {
            if (Model == null) { return; }
            transform.position = Model.Location.Position;
        }
    }
}