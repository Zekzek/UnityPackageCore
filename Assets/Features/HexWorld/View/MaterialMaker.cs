using System.Collections.Generic;
using UnityEngine;

namespace Zekzek.HexWorld
{
    public class MaterialMaker
    {
        private const float COLOR_STEP_SIZE = 0.1f;

        private readonly IDictionary<Vector3Int, Material> _materials = new Dictionary<Vector3Int, Material>();

        // Singleton
        private static MaterialMaker _instance;
        public static MaterialMaker Instance { get { if (_instance == null) { _instance = new MaterialMaker(); } return _instance; } }
        private MaterialMaker() { }

        public Material Get(Vector3 color)
        {
            Vector3Int key = ConvertToColorSteps(color);

            if (!_materials.ContainsKey(key)) {
                Material colorMaterial = new Material(Shader.Find("Standard"));
                colorMaterial.color = new Color(key.x * COLOR_STEP_SIZE, key.y * COLOR_STEP_SIZE, key.z * COLOR_STEP_SIZE);

                _materials.Add(key, colorMaterial);
            }

            return _materials[key];
        }

        private Vector3Int ConvertToColorSteps(Vector3 color)
        {
            return new Vector3Int(Mathf.CeilToInt(color.x / COLOR_STEP_SIZE), Mathf.CeilToInt(color.y / COLOR_STEP_SIZE), Mathf.CeilToInt(color.z / COLOR_STEP_SIZE));
        }
    }
}