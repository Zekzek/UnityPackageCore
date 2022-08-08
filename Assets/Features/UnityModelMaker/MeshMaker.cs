using System.Collections.Generic;
using UnityEngine;

namespace Zekzek.UnityModelMaker
{
    public class MeshMaker
    {
        public enum MeshType
        {
            Rock,
            Tree
        }

        private const float STEP_SIZE = 0.1f;

        private readonly IDictionary<Vector3Int, Mesh> _meshes = new Dictionary<Vector3Int, Mesh>();

        private static readonly Vector2 nPoint =  new Vector2(    0,  0.57f);
        private static readonly Vector2 nePoint = new Vector2( 0.5f,  0.28f);
        private static readonly Vector2 sePoint = new Vector2( 0.5f, -0.28f);
        private static readonly Vector2 sPoint =  new Vector2(    0, -0.57f);
        private static readonly Vector2 swPoint = new Vector2(-0.5f, -0.28f);
        private static readonly Vector2 nwPoint = new Vector2(-0.5f,  0.28f);

        private static readonly Vector2 neHex = new Vector2( 0.5f,  0.85f);
        private static readonly Vector2 eHex =  new Vector2(   1f,  0);
        private static readonly Vector2 seHex = new Vector2( 0.5f, -0.85f);
        private static readonly Vector2 swHex = new Vector2(-0.5f, -0.85f);
        private static readonly Vector2 wHex =  new Vector2(  -1f,  0);
        private static readonly Vector2 nwHex = new Vector2(-0.5f,  0.85f);

        // Singleton
        private static MeshMaker _instance;
        public static MeshMaker Instance { get { if (_instance == null) { _instance = new MeshMaker(); } return _instance; } }
        private MeshMaker() { }

        public Mesh Get(Vector3 color)
        {
            return GetRock(new Vector3(0.2f, 0.5f, 0.9f));
        }

        private Vector3 CalcHexPoint(Vector2 hex, Vector2 corner, float height, Vector3 scale)
        {
            return new Vector3(scale.x * (hex.x + corner.x), scale.y * height, scale.z * (hex.y + corner.y));
        }

        private Mesh GetRock(Vector3 scale)
        {
            Mesh mesh = new Mesh {
                vertices = new Vector3[] {
                    CalcHexPoint(Vector2.zero, Vector2.zero, 1, scale),

                    CalcHexPoint(Vector2.zero, nPoint, 0.7f, scale),
                    CalcHexPoint(Vector2.zero, nePoint, 0.7f, scale),
                    CalcHexPoint(Vector2.zero, sePoint, 0.7f, scale),
                    CalcHexPoint(Vector2.zero, sPoint, 0.7f, scale),
                    CalcHexPoint(Vector2.zero, swPoint, 0.7f, scale),
                    CalcHexPoint(Vector2.zero, nwPoint, 0.7f, scale),
                    
                    CalcHexPoint(neHex, nwPoint, 0f, scale),
                    CalcHexPoint(neHex, Vector2.zero, 0.1f, scale),
                    CalcHexPoint(eHex, nPoint, 0f, scale),
                    CalcHexPoint(eHex, Vector2.zero, 0.1f, scale),
                    CalcHexPoint(seHex, nePoint, 0f, scale),
                    CalcHexPoint(seHex, Vector2.zero, 0.1f, scale),
                    CalcHexPoint(swHex, sePoint, 0f, scale),
                    CalcHexPoint(swHex, Vector2.zero, 0.1f, scale),
                    CalcHexPoint(wHex, sPoint, 0f, scale),
                    CalcHexPoint(wHex, Vector2.zero, 0.1f, scale),
                    CalcHexPoint(nwHex, swPoint, 0f, scale),
                    CalcHexPoint(nwHex, Vector2.zero, 0.1f, scale),
                },
                triangles = new int[] {
                    0, 1, 2,
                    0, 2, 3,
                    0, 3, 4,
                    0, 4, 5,
                    0, 5, 6,
                    0, 6, 1,

                    1, 7, 8,
                    1, 8, 2,
                    2, 8, 9,
                    
                    2, 9, 10,
                    2, 10, 3,
                    3, 10, 11,

                    3, 11, 12,
                    3, 12, 4,
                    4, 12, 13,

                    4, 13, 14,
                    4, 14, 5,
                    5, 14, 15,

                    5, 15, 16,
                    5, 16, 6,
                    6, 16, 17,

                    6, 17, 18,
                    6, 18, 1,
                    1, 18, 7
                }
            };
            return mesh;
        }
    }
}