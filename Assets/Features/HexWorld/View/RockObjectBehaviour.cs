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
                if (_model != null) { UpdateMesh(); }
            }
        }

        private void UpdateMesh()
        {
            if (_meshRenderer == null) { _meshRenderer = GetComponent<MeshRenderer>(); }
            if (_meshFilter == null) { _meshFilter = GetComponent<MeshFilter>(); }

            DisplayComponent display = _model.Display;
            _meshRenderer.material = MaterialMaker.Instance.Get((0.1f + 0.4f * display.Color) * Vector3.one);
            _meshFilter.mesh = MeshMaker.Instance.GetRocks(display.Color, display.Quantity);
        }
    }
}