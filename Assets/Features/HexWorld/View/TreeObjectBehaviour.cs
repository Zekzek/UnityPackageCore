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

        private void Start()
        {
            Vector2[] spokes = new Vector2[6];
            for(int i = 0; i < spokes.Length; i++) {
                if (i % 2 == 0) {
                    spokes[i] = new Vector2(0.6f + (UnityEngine.Random.value * 100) % 4 * 0.1f, 0.1f + (UnityEngine.Random.value * 100) % 4 * 0.1f);
                } else {
                    spokes[i] = new Vector2(0.1f + (UnityEngine.Random.value * 100) % 4 * 0.1f, 0.1f + (UnityEngine.Random.value * 100) % 4 * 0.1f);
                }
            }

            SetDiskMesh(Vector3.zero, Vector3.zero, spokes);
            SetOutlineMesh(Vector3.zero, Vector3.up, 0.05f, spokes);
        }

        private void SetDiskMesh(Vector3 color, Vector3 center, params Vector2[] spokes)
        {
            MeshRenderer meshRenderer = disk.GetComponent<MeshRenderer>();
            MeshFilter meshFilter = disk.GetComponent<MeshFilter>();

            meshRenderer.material = MaterialMaker.Instance.Get(color);
            meshFilter.mesh = MeshMaker.Instance.ConvertToHardEdged(MeshMaker.Instance.GetDisk(center, MeshMaker.Instance.GetPointsFromSpokes(center, spokes)));
        }

        private void SetOutlineMesh(Vector3 center, Vector3 color, float width, params Vector2[] spokes)
        {
            MeshRenderer meshRenderer = outline.GetComponent<MeshRenderer>();
            MeshFilter meshFilter = outline.GetComponent<MeshFilter>();

            meshRenderer.material = MaterialMaker.Instance.Get(color);
            meshFilter.mesh = MeshMaker.Instance.ConvertToHardEdged(MeshMaker.Instance.GetOutline(width, MeshMaker.Instance.GetPointsFromSpokes(center, spokes)));
        }
    }
}