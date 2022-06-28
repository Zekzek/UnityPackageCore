using UnityEngine;

namespace Zekzek.HexWorld
{
    public class HexTileBehaviour : MonoBehaviour
    {
        [SerializeField]
        private GameObject tileObject;

        [SerializeField]
        private GameObject highlightObject;

        public HexTile Model { get; private set; }

        public void Apply(HexTile tile)
        {
            // Remove listeners to previous model
            if (Model != null) {
                Model.OnHeightChanged -= HandleHeightChanged;
                Model.OnHighlightChanged -= HandleHighlightChanged;
            }

            // Replace model reference
            Model = tile;

            // Activate and name unity object
            gameObject.SetActive(tile != null);
            gameObject.name = "HexTile: " + tile?.Location.GridIndex;

            // Apply listeners for new model
            if (tile != null) {
                Model.OnHeightChanged += HandleHeightChanged;
                Model.OnHighlightChanged += HandleHighlightChanged;
                HandleHeightChanged();
                HandleHighlightChanged();
            }
        }

        private void HandleHeightChanged()
        {
            tileObject.SetActive(Model.Location.GridPosition.y >= 0);
            transform.localPosition = Model.Location.Position;
        }

        private void HandleHighlightChanged()
        {
            highlightObject.SetActive(Model.Highlight);
        }

        public void HandleInput()
        {
            //if (Input.GetKeyDown(KeyCode.Equals)) { Model.Raise(); }
            //if (Input.GetKeyDown(KeyCode.Minus)) { Model.Lower(); }
        }
    }
}