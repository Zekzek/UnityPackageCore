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
            Vector2Int index = Model.Location.GridIndex;
            int pointCount = 4;// (int)(2 + 4 * Noise.Noise.SteppedPercent(19, index.x, index.y, 1));
            SetMesh(GenerationUtil.GetColor(Model.Location.GridIndex),
                layerCount: 3, pointCount: pointCount,
                spin: (int)(180f / pointCount * (0.5f + Noise.Noise.SteppedPercent(9, index.x, index.y, 2))),
                peak: new Vector2(0.5f + 0.5f * Noise.Noise.SteppedPercent(13, index.x, index.y, 3), 0.1f + 0.2f * Noise.Noise.SteppedPercent(13, index.x, index.y, 4)),
                valley: new Vector2(0.1f + 0.5f * Noise.Noise.SteppedPercent(23, index.x, index.y, 5), 0.2f * Noise.Noise.SteppedPercent(23, index.x, index.y, 6)));
        }

        private void SetMesh(Vector3 color, int layerCount, int pointCount, int spin, Vector2 peak, Vector2 valley)
        {
            foreach(GameObject child in _children) {
                child.SetActive(false);
            } 

            Mesh disk = null;
            Mesh outline = null;
            Vector3 scale = Vector3.one;
            float angle = 0;
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
                _children[2 * i].transform.localRotation = Quaternion.Euler(0, angle, 0);
                _children[2 * i].SetActive(true);

                outline ??= GetOutlineMesh(0.05f, points);
                ApplyMesh(_children[2 * i + 1], color, outline);
                _children[2 * i + 1].transform.localScale = scale;
                _children[2 * i + 1].transform.localRotation = Quaternion.Euler(0, angle, 0);
                _children[2 * i + 1].SetActive(true);

                scale = new Vector3(0.8f * scale.x, 1.5f * scale.y, 0.8f * scale.z);
                angle += spin;
            }
        }
    }
}