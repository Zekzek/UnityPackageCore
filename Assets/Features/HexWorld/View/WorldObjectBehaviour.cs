using System;
using UnityEngine;

namespace Zekzek.HexWorld
{
    public abstract class WorldObjectBehaviour : MonoBehaviour
    {
        public virtual WorldObject Model { get; set; }
        protected abstract Type ModelType { get; }

        protected virtual void Update()
        {
            if (Model == null) { return; }
            transform.position = Model.Location.Position;
        }
    }
}