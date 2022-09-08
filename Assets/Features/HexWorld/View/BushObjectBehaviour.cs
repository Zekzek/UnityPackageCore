using System.Collections.Generic;
using UnityEngine;
using Zekzek.UnityModelMaker;

namespace Zekzek.HexWorld
{
    public class BushObjectBehaviour : WorldObjectBehaviour
    {
        public override WorldObject Model { get => base.Model; set { base.Model = value; SetMesh(); } }

        private GameObject[] _children = null;

        private void SetMesh()
        {
            Vector2Int index = Model.Location.GridIndex;
            int pointCount = (int)(2 + 4 * Noise.Noise.SteppedPercent(19, index.x, index.y, 1));
            SetMesh(GenerationUtil.GetColor(Model.Location.GridIndex),
                layerCount: 3, pointCount: pointCount,
                spin: (int)(180f / pointCount * (0.5f + Noise.Noise.SteppedPercent(9, index.x, index.y, 2))),
                peak: new Vector2(0.2f + 0.2f * Noise.Noise.SteppedPercent(13, index.x, index.y, 3), 0.05f + 0.05f * Noise.Noise.SteppedPercent(13, index.x, index.y, 4)),
                valley: new Vector2(0.05f + 0.2f * Noise.Noise.SteppedPercent(23, index.x, index.y, 5), 0.05f * Noise.Noise.SteppedPercent(23, index.x, index.y, 6)));
        }

        private void SetMesh(Vector3 color, int layerCount, int pointCount, int spin, Vector2 peak, Vector2 valley)
        {
            if (_children == null) {
                _children = new GameObject[2];
                GameObject diskObect = new GameObject("Disk");
                diskObect.transform.parent = gameObject.transform;
                diskObect.transform.localPosition = Vector3.zero;
                _children[0] = diskObect;
                GameObject outlineObect = new GameObject("Outline");
                outlineObect.transform.parent = gameObject.transform;
                outlineObect.transform.localPosition = Vector3.zero;
                _children[1] = outlineObect;
            }

            foreach (GameObject child in _children) {
                child.SetActive(false);
            } 

            Vector3[] points = GetPointsFromSpokes(pointCount, peak, valley);

            Mesh disk = MeshMaker.Instance.GetDisk(Vector3.zero, points);
            disk = MeshMaker.GetLayered(disk, layerCount, new Vector3(0.7f, 1.3f, 0.7f), new Vector3(0, spin, 0), Vector3.zero);
            disk = MeshMaker.AddBarnacles(disk, 0.5f * Vector3.one, 2);
            ApplyMesh(_children[0], Vector3.zero, disk);
            _children[0].SetActive(true);

            Mesh outline = MeshMaker.Instance.GetOutline(0.02f, points);
            outline = MeshMaker.GetLayered(outline, layerCount, new Vector3(0.7f, 1.3f, 0.7f), new Vector3(0, spin, 0), Vector3.zero);
            outline = MeshMaker.AddBarnacles(outline, 0.5f * Vector3.one, 2);
            ApplyMesh(_children[1], color, outline);
            _children[1].SetActive(true);
        }
    }
}