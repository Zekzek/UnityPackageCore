using System;

namespace Zekzek.HexWorld
{
    public class TargetableComponent : WorldObjectComponent
    {
        public Action OnHighlightChanged;

        public override WorldComponentType ComponentType => WorldComponentType.Targetable;

        private bool _highlight;
        public bool Highlight { 
            get => _highlight;
            set {
                if (_highlight != value) {
                    _highlight = value;
                    OnHighlightChanged();
                }
            }
        }

        public TargetableComponent(uint worldObjectId) : base(worldObjectId) { }
    }
}