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

        public Mesh GetRocks(float size, float quantity)
        {
            int count = (int)(1 + 3 * quantity);
            float biggest = 0.07f + 0.02f * (int)(5 * size);

            Mesh rock = GetRock(biggest * Vector3.one, Vector3.zero);
            return AddBarnacles(rock, (0.5f + biggest % 0.4f) * Vector3.one, count);
        }

        private Mesh GetRock(Vector3 scale, Vector3 offset)
        {
            Vector3[] vertices = new Vector3[] {
                CalcHexPoint(Vector2.zero, Vector2.zero, 1, scale),

                CalcHexPoint(Vector2.zero, nPoint, 0.8f, scale),
                CalcHexPoint(Vector2.zero, nePoint, 0.8f, scale),
                CalcHexPoint(Vector2.zero, sePoint, 0.8f, scale),
                CalcHexPoint(Vector2.zero, sPoint, 0.8f, scale),
                CalcHexPoint(Vector2.zero, swPoint, 0.8f, scale),
                CalcHexPoint(Vector2.zero, nwPoint, 0.8f, scale),

                CalcHexPoint(neHex, nwPoint, 0.4f, scale),
                CalcHexPoint(neHex, Vector2.zero, 0.5f, scale),
                CalcHexPoint(eHex, nPoint, 0.4f, scale),
                CalcHexPoint(eHex, Vector2.zero, 0.5f, scale),
                CalcHexPoint(seHex, nePoint, 0.4f, scale),
                CalcHexPoint(seHex, Vector2.zero, 0.5f, scale),
                CalcHexPoint(swHex, sePoint, 0.4f, scale),
                CalcHexPoint(swHex, Vector2.zero, 0.5f, scale),
                CalcHexPoint(wHex, sPoint, 0.4f, scale),
                CalcHexPoint(wHex, Vector2.zero, 0.5f, scale),
                CalcHexPoint(nwHex, swPoint, 0.4f, scale),
                CalcHexPoint(nwHex, Vector2.zero, 0.5f, scale),

                CalcHexPoint(Vector2.zero, nPoint, 0.1f, scale),
                CalcHexPoint(Vector2.zero, nePoint, 0.1f, scale),
                CalcHexPoint(Vector2.zero, sePoint, 0.1f, scale),
                CalcHexPoint(Vector2.zero, sPoint, 0.1f, scale),
                CalcHexPoint(Vector2.zero, swPoint, 0.1f, scale),
                CalcHexPoint(Vector2.zero, nwPoint, 0.1f, scale),

                CalcHexPoint(Vector2.zero, Vector2.zero, 0, scale),
            };

            int[] triangles = new int[] {
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
                1, 18, 7,

                8,7,19,
                20,8,19,
                9,8,20,
                10,9,20,
                21,10,20,
                11,10,21,
                12,11,21,
                22,12,21,
                13,12,22,
                14,13,22,
                23,14,22,
                15,14,23,
                16,15,23,
                24,16,23,
                17,16,24,
                18,17,24,
                19,18,24,
                7,18,19,

                20,19,25,
                21,20,25,
                22,21,25,
                23,22,25,
                24,23,25,
                19,24,25
            };

            Vector3[] normals = new Vector3[vertices.Length];
            for (int i = 0; i < vertices.Length; i++) {
                normals[i] = vertices[i] - scale.y * Vector3.up / 2;
            }

            for (int i = 0; i < vertices.Length; i++) {
                vertices[i] += offset;
            }

            Mesh mesh = new Mesh {
                vertices = vertices,
                normals = normals,
                triangles = triangles
            };

            return mesh;
        }

        private static Mesh Combine(params Mesh[] meshes)
        {
            CombineInstance[] combiners = new CombineInstance[meshes.Length];
            for(int i = 0; i < combiners.Length; i++) {
                combiners[i] = new CombineInstance() { mesh = meshes[i], transform = Matrix4x4.identity };
            };

            Mesh combined = new Mesh();
            combined.CombineMeshes(combiners);
            return combined;
        }

        private static Mesh ScaleAndTranslate(Mesh source, Vector3 scale, Vector3 offset)
        {
            Vector3[] vertices = new Vector3[source.vertexCount];
            for (int i = 0; i < source.vertexCount; i++) {
                vertices[i] = Vector3.Scale(source.vertices[i], scale) + offset;
            }

            return new Mesh() { vertices = vertices, normals = source.normals, triangles = source.triangles };
        }

        private static Mesh AddBarnacles(Mesh source, Vector3 scale, int count)
        {
            Mesh[] meshes = new Mesh[count + 1];
            meshes[0] = source;

            Vector3 extent = source.bounds.extents;
            Vector3 barnacleExtent = Vector3.Scale(extent, scale);

            if (count > 0) { meshes[1] = ScaleAndTranslate(source, scale, new Vector3(extent.x + barnacleExtent.x, 0, extent.z + barnacleExtent.z)); }
            if (count > 1) { meshes[2] = ScaleAndTranslate(source, scale, new Vector3(extent.x + barnacleExtent.x, 0, -extent.z - barnacleExtent.z)); }
            if (count > 2) { meshes[3] = ScaleAndTranslate(source, scale, new Vector3(-extent.x - barnacleExtent.x, 0, -extent.z - barnacleExtent.z)); }
            if (count > 3) { meshes[4] = ScaleAndTranslate(source, scale, new Vector3(-extent.x - barnacleExtent.x, 0, extent.z + barnacleExtent.z)); }

            return Combine(meshes);
        }

        private Vector3 CalcHexPoint(Vector2 hex, Vector2 corner, float height, Vector3 scale)
        {
            return new Vector3(scale.x * (hex.x + corner.x), scale.y * height, scale.z * (hex.y + corner.y));
        }

        private Mesh ConvertToHardEdged(Mesh mesh)
        {
            Vector3[] vertices = new Vector3[mesh.triangles.Length];
            for(int i = 0; i < mesh.triangles.Length; i++) {
                vertices[i] = mesh.vertices[mesh.triangles[i]];
            }

            Vector3[] normals = new Vector3[vertices.Length];
            for (int i = 0; i < vertices.Length; i += 3) {
                Vector3 normal = (vertices[i] + vertices[i + 1] + vertices[i + 2]) / 3f;
                normals[i] = normals[i + 1] = normals[i + 2] = normal;
            }

            int[] triangles = new int[3 * vertices.Length];
            for (int i = 0; i < vertices.Length; i++) {
                triangles[i] = i;
            }

            return new Mesh {
                vertices = vertices,
                normals = normals,
                triangles = triangles
            };
        }
    }
}