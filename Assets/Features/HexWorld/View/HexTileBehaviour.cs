﻿using System;
using UnityEngine;

namespace Zekzek.HexWorld
{
    public class HexTileBehaviour : WorldObjectBehaviour
    {
        protected override Type ModelType => default;
        private TargetableComponent _targetableComponent;

        [SerializeField] private GameObject tileObject;
        [SerializeField] private GameObject highlightObject;
        [SerializeField] private MeshRenderer[] colorRenderers;

        public override WorldObject Model { 
            get => base.Model;
            set {
                if (_targetableComponent != null) {
                    _targetableComponent.Highlight = false;
                    _targetableComponent.OnHighlightChanged -= HandleHighlightChanged;
                }

                base.Model = value;
                InitTargetComponent();

                if (value != null) {
                    _targetableComponent.OnHighlightChanged += HandleHighlightChanged;
                    HandleHighlightChanged();
                }

                Material colorMaterial = MaterialMaker.Instance.Get(Vector3.Normalize(Model.Location.Position/20f));
                foreach (Renderer renderer in colorRenderers) {
                    renderer.material = colorMaterial;
                }
            }
        }

        protected override void Update()
        {
            // Tiles don't need to rotate, so override to only update position
            if (Model == null) { return; }
            transform.position = Model.Location.Position;
        }

        private void InitTargetComponent()
        {
            if (Model == null) {
                _targetableComponent = null;
            } else {
                _targetableComponent = (TargetableComponent)Model.GetComponent(WorldComponentType.Targetable);
            }
        }

        private void HandleHighlightChanged()
        {
            highlightObject.SetActive(_targetableComponent.Highlight);
        }
    }
}