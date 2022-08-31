using System;
using UnityEngine;
using Zekzek.UnityModelMaker;

namespace Zekzek.HexWorld
{
    public class TreeObjectBehaviour : WorldObjectBehaviour
    {
        [SerializeField] private uint key;

        protected override Type ModelType => default;

        private void Start()
        {
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            MeshFilter meshFilter = GetComponent<MeshFilter>();

            meshRenderer.material = MaterialMaker.Instance.Get(0.2f * Vector3.right);
            meshFilter.mesh = MeshMaker.Instance.ConvertToHardEdged(MeshMaker.Instance.GetOutline(Vector3.left, Vector3.up, Vector3.right));
        }
    }
}