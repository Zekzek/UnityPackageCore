using System.Collections.Generic;
using UnityEngine;

namespace Zekzek.HexWorld
{
    public class BushObjectBehaviour : WorldObjectBehaviour
    {
        public override WorldObject Model { get => base.Model; set { base.Model = value; SetMesh(); } }

        private List<GameObject> _children = new List<GameObject>();

        private void Start()
        {
            SetMesh();
        }

        private void SetMesh()
        {
            SetMesh(3, 4, new Vector2(1, 0.2f), new Vector2(0.4f, 0.1f));
        }

        private void SetMesh(int layerCount, int pointCount, Vector2 peak, Vector2 valley)
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
                    _children.Add(diskObect);
                    GameObject outlineObect = new GameObject("Outline" + i);
                    outlineObect.transform.parent = gameObject.transform;
                    _children.Add(outlineObect);
                }

                disk ??= GetDiskMesh(points);
                ApplyMesh(_children[2 * i], Vector3.zero, disk);
                _children[2 * i].transform.localScale = scale;
                _children[2 * i].transform.localRotation = Quaternion.Euler(0, spin, 0);


                outline ??= GetOutlineMesh(0.01f, points);
                ApplyMesh(_children[2 * i + 1], Vector3.up, outline);
                _children[2 * i + 1].transform.localScale = scale;
                _children[2 * i + 1].transform.localRotation = Quaternion.Euler(0, spin, 0);

                scale = new Vector3(0.7f * scale.x, 1.8f * scale.y, 0.7f * scale.z);
                spin += 110;
            }
        }
    }
}