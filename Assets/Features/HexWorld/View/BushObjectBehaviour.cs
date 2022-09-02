using System.Collections.Generic;
using UnityEngine;

namespace Zekzek.HexWorld
{
    public class BushObjectBehaviour : WorldObjectBehaviour
    {
        public override WorldObject Model { get => base.Model; set { base.Model = value; SetMesh((int)value.Id); } }

        private List<GameObject> _children = new List<GameObject>();

        private void SetMesh(int seed)
        {
            SetMesh(
                GenerationUtil.GetColor(Model.Location.GridIndex),
                2 + Noise.Noise.GetPositiveInt(seed) % 2, 
                2 + Noise.Noise.GetPositiveInt(seed, 1) % 4,
                90 + Noise.Noise.GetPositiveInt(seed, 1) % 180,
                new Vector2(0.5f + Noise.Noise.GetPercent(seed, 2) / 2f, 0.1f + Noise.Noise.GetPercent(seed, 3) / 4f),
                new Vector2(0.1f + Noise.Noise.GetPercent(seed, 4) / 3f, Noise.Noise.GetPercent(seed, 3) / 4f));
        }

        private void SetMesh(Vector3 color, int layerCount, int pointCount, int spinStep, Vector2 peak, Vector2 valley)
        {
            foreach(GameObject child in _children) {
                child.SetActive(false);
            }

            Mesh disk = null;
            Mesh outline = null;
            Vector3 scale = Vector3.one;
            float spin = 0;
            for(int i = 0; i < layerCount; i++) {
                Vector3[] points = GetPointsFromSpokes(pointCount, peak, valley);

                while (2 * i >= _children.Count) {
                    GameObject diskObect = new GameObject("Disk" + i);
                    diskObect.transform.parent = gameObject.transform;
                    diskObect.transform.localPosition = Vector3.zero;
                    _children.Add(diskObect);
                    GameObject outlineObect = new GameObject("Outline" + i);
                    outlineObect.transform.parent = gameObject.transform;
                    outlineObect.transform.localPosition = Vector3.zero;
                    _children.Add(outlineObect);
                }

                disk ??= GetDiskMesh(points);
                ApplyMesh(_children[2 * i], Vector3.zero, disk);
                _children[2 * i].transform.localScale = scale;
                _children[2 * i].transform.localRotation = Quaternion.Euler(0, spin, 0);
                _children[2 * i].SetActive(true);

                outline ??= GetOutlineMesh(0.05f, points);
                ApplyMesh(_children[2 * i + 1], color, outline);
                _children[2 * i + 1].transform.localScale = scale;
                _children[2 * i + 1].transform.localRotation = Quaternion.Euler(0, spin, 0);
                _children[2 * i + 1].SetActive(true);

                scale = new Vector3(0.7f * scale.x, 1.3f * scale.y, 0.7f * scale.z);
                spin += spinStep;
            }
        }
    }
}