using System;
using UnityEngine;

namespace Zekzek.HexWorld
{
    public class HexTileBehaviour : WorldObjectBehaviour
    {
        protected override Type ModelType => default;
        private TargetableComponent _targetableComponent;

        [SerializeField] private GameObject tileObject;
        [SerializeField] private GameObject highlightObject;

        public override WorldObject Model { 
            get => base.Model; 
            set {
                if (Model != null) {
                    if (_targetableComponent == null) {
                        _targetableComponent = (TargetableComponent)Model.GetComponent(WorldComponentType.Targetable);
                    }
                    _targetableComponent.OnHighlightChanged -= HandleHighlightChanged;
                }

                base.Model = value;
                InitTargetComponent();
                _targetableComponent.OnHighlightChanged += HandleHighlightChanged;
                HandleHighlightChanged();
            }
        }

        private void InitTargetComponent()
        {
            if (Model == null) {
                _targetableComponent = null;
            } else if (_targetableComponent == null) {
                _targetableComponent = (TargetableComponent)Model.GetComponent(WorldComponentType.Targetable);
            }
        }

        private void HandleHighlightChanged()
        {
            highlightObject.SetActive(_targetableComponent.Highlight);
        }
    }
}