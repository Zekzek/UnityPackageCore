using System;
using UnityEngine;
using Zekzek.UnityModelMaker;

namespace Zekzek.HexWorld
{
    public class RockObjectBehaviour : WorldObjectBehaviour
    {
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private WorldObject _model;
        public override WorldObject Model { 
            get => _model;
            set {
                _model = value;
                UpdateMesh();
            }
        }

        protected override Type ModelType => default;

        private void UpdateMesh()
        {
            if (_meshRenderer == null) { _meshRenderer = GetComponent<MeshRenderer>(); }
            if (_meshFilter == null) { _meshFilter = GetComponent<MeshFilter>(); }
            
            _meshRenderer.material = MaterialMaker.Instance.Get(0.3f * Vector3.one);
            _meshFilter.mesh = MeshMaker.Instance.Get(Vector3.zero);
        }
    }
}