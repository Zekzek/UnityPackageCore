using System.Collections.Generic;
using UnityEngine;

namespace Zekzek.Parallax
{
    public class ParallaxController : MonoBehaviour
    {
        [SerializeField] private RectTransform[] initialLayers;
        [SerializeField] private bool invert;

        private List<RectTransform> _layers = new List<RectTransform>();
        private Dictionary<int, Vector2> _layerOffsetDeltas = new Dictionary<int, Vector2>();

        private void Start()
        {
            foreach (RectTransform layer in initialLayers) { AddLayer(layer); }
        }

        private void Update()
        {
            var mousePosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            foreach (RectTransform layer in _layers) {
                ApplyParallax(mousePosition, layer);
            }
        }

        private void AddLayer(RectTransform layer)
        {
            _layers.Add(layer);
            _layerOffsetDeltas[layer.GetInstanceID()] = layer.offsetMin - layer.offsetMax;
        }

        private void ApplyParallax(Vector2 viewPoint, RectTransform layer)
        {
            Vector2 offsetDelta = _layerOffsetDeltas[layer.GetInstanceID()];
            float viewX = Mathf.Clamp01(viewPoint.x);
            float viewY = Mathf.Clamp01(viewPoint.y);

            if (invert) {
                layer.offsetMin = new Vector2(offsetDelta.x * viewX, offsetDelta.y * viewY);
                layer.offsetMax = new Vector2(-offsetDelta.x * (1 - viewX), -offsetDelta.y * (1 - viewY));
            } else {
                layer.offsetMin = new Vector2(offsetDelta.x * (1 - viewX), offsetDelta.y * (1 - viewY));
                layer.offsetMax = new Vector2(-offsetDelta.x * viewX, -offsetDelta.y * viewY);
            }
        }
    }
}
