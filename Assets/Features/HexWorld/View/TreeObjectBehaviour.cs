using System;
using UnityEngine;
using Zekzek.UnityModelMaker;

namespace Zekzek.HexWorld
{
    public class TreeObjectBehaviour : WorldObjectBehaviour
    {
        [SerializeField] private uint key;
        [SerializeField] private GameObject disk;
        [SerializeField] private GameObject outline;

        protected override Type ModelType => default;

        private void Start()
        {
            Vector3[] points = new Vector3[] {
                new Vector3(1, 0.3f, 0),
                new Vector3(0.3f, 0.1f, 0.3f),
                new Vector3(0, 0.3f, 1),
                new Vector3(-0.3f, 0.1f, 0.3f),
                new Vector3(-1, 0.3f, 0),
                new Vector3(-0.3f, 0.1f, -0.3f),
                new Vector3(0, 0.3f, -1),
                new Vector3(0.3f, 0.1f, -0.3f),
                new Vector3(1, 0.3f, 0)
            };
            SetDiskMesh(Vector3.zero, Vector3.zero, points);
            SetOutlineMesh(Vector3.right, 0.05f, points);
        }

        private void SetDiskMesh(Vector3 color, Vector3 center, params Vector3[] points)
        {
            MeshRenderer meshRenderer = disk.GetComponent<MeshRenderer>();
            MeshFilter meshFilter = disk.GetComponent<MeshFilter>();

            meshRenderer.material = MaterialMaker.Instance.Get(color);
            meshFilter.mesh = MeshMaker.Instance.ConvertToHardEdged(MeshMaker.Instance.GetDisk(center, points));
        }

        private void SetOutlineMesh(Vector3 color, float width, params Vector3[] points)
        {
            MeshRenderer meshRenderer = outline.GetComponent<MeshRenderer>();
            MeshFilter meshFilter = outline.GetComponent<MeshFilter>();

            meshRenderer.material = MaterialMaker.Instance.Get(color);
            meshFilter.mesh = MeshMaker.Instance.ConvertToHardEdged(MeshMaker.Instance.GetOutline(width, points));
        }
    }
}