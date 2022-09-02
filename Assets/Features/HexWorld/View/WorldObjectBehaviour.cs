using System;
using UnityEngine;
using Zekzek.UnityModelMaker;

namespace Zekzek.HexWorld
{
    public abstract class WorldObjectBehaviour : MonoBehaviour
    {
        private WorldObject _model;
        public virtual WorldObject Model { get => _model; set { _model = value; } }
        protected Type ModelType { get; }

        protected virtual void Update()
        {
            if (Model == null) { return; }
            transform.position = Model.Location.Position;
            transform.rotation = FacingUtil.GetRotation(Model.Location.Facing);
        }

        protected Vector3[] GetPointsFromSpokes(int points, Vector2 peak, Vector2 valley)
        {
            Vector2[] spokes = new Vector2[2 * points];
            for(int i = 0; i < points; i++) {
                spokes[2 * i] = peak;
                spokes[2 * i + 1] = valley;
            }

            return GetPointsFromSpokes(spokes);
        }

        protected Vector3[] GetPointsFromSpokes(params Vector2[] spokes)
        {
            return MeshMaker.Instance.GetPointsFromSpokes(Vector3.zero, spokes);
        }

        protected Mesh GetDiskMesh(params Vector3[] points)
        {
            return MeshMaker.Instance.ConvertToHardEdged(MeshMaker.Instance.GetDisk(Vector3.zero, points));
        }

        protected Mesh GetOutlineMesh(float width, params Vector3[] points)
        {
            return MeshMaker.Instance.ConvertToHardEdged(MeshMaker.Instance.GetOutline(width, points));
        }

        protected void ApplyMesh(GameObject gameObject, Vector3 color, Mesh mesh)
        {
            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();

            if (!meshRenderer) { meshRenderer = gameObject.AddComponent<MeshRenderer>(); }
            if (!meshFilter) { meshFilter = gameObject.AddComponent<MeshFilter>(); }

            meshRenderer.material = MaterialMaker.Instance.Get(color);
            meshFilter.mesh = mesh;
        }
    }
}