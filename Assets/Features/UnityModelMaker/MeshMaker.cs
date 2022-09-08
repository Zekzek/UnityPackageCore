using System.Collections.Generic;
using UnityEngine;
using Zekzek.HexWorld;

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

        private static readonly Vector2Int[] neighbors = new Vector2Int[] {
            FacingUtil.E,
            FacingUtil.SE,
            FacingUtil.SW,
            FacingUtil.W,
            FacingUtil.NW,
            FacingUtil.NE
        };

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

        public Mesh UpdateTerrain(Mesh source, Vector2Int centerTile, int screenWidth, int screenHeight)
        {
            Dictionary<Vector2Int, int> indexMap = new Dictionary<Vector2Int, int>();
            Dictionary<Vector2Int, int> heights = new Dictionary<Vector2Int, int>();
            Vector3[] vertices = new Vector3[source.vertexCount];

            List<Vector2Int> screenIndices = new List<Vector2Int>(WorldUtil.GetRectangleIndicesAround(centerTile, screenWidth, screenHeight));
            
            int vertexCount = 0;
            foreach (Vector2Int screenIndex in screenIndices) {
                // Ensure the center point has a recorded height
                if (!heights.ContainsKey(screenIndex)) {
                    heights.Add(screenIndex, HexWorld.HexWorld.Instance.GetFirstAt(screenIndex, WorldObjectType.Tile)?.Location.GridHeight ?? 0);
                }

                // Ensure every neighbor has a recorded height
                foreach (Vector2Int neighbor in neighbors) {
                    if (!heights.ContainsKey(screenIndex + neighbor)) {
                        heights.Add(screenIndex + neighbor, HexWorld.HexWorld.Instance.GetFirstAt(screenIndex + neighbor, WorldObjectType.Tile)?.Location.GridHeight ?? 0);
                    }
                }

                // Encode by multiplying by 2, so we can easily reference midpoints as ints
                Vector2Int encodedCenterPoint = 2 * screenIndex;

                // Ensure the center point has a vertex/normal
                Vector3 centerLocation = WorldUtil.GridIndexToPosition(screenIndex, heights[screenIndex]);
                if (!indexMap.ContainsKey(encodedCenterPoint)) {
                    indexMap.Add(encodedCenterPoint, vertexCount);
                    vertices[vertexCount] = centerLocation;
                    vertexCount++;
                }

                // Ensure every midpoint has a vertex/normal
                foreach (Vector2Int neighbor in neighbors) {
                    if (!indexMap.ContainsKey(encodedCenterPoint + neighbor)) {
                        Vector2Int neighborIndex = screenIndex + neighbor;
                        Vector3 neighborLocation = WorldUtil.GridIndexToPosition(neighborIndex, heights[neighborIndex]);
                        indexMap.Add(encodedCenterPoint + neighbor, vertexCount);
                        vertices[vertexCount] = (centerLocation + neighborLocation) / 2f;
                        vertexCount++;
                    }
                }
            }

            Mesh terrain = new Mesh {
                vertices = vertices,
                normals = source.normals,
                triangles = source.triangles
            };
            terrain.RecalculateNormals();
            return terrain;

        }

        public Mesh GetTerrain(Vector2Int centerTile, int screenWidth, int screenHeight)
        {
            Dictionary<Vector2Int, int> indexMap = new Dictionary<Vector2Int, int>();
            Dictionary<Vector2Int, int> heights = new Dictionary<Vector2Int, int>();
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> triangles = new List<int>();

            List<Vector2Int> screenIndices = new List<Vector2Int>(WorldUtil.GetRectangleIndicesAround(centerTile, screenWidth, screenHeight));
            
            foreach (Vector2Int screenIndex in screenIndices) {
                // Ensure the center point has a recorded height
                if (!heights.ContainsKey(screenIndex)) {
                    heights.Add(screenIndex, HexWorld.HexWorld.Instance.GetFirstAt(screenIndex, WorldObjectType.Tile)?.Location.GridHeight ?? 0);
                }

                // Ensure every neighbor has a recorded height
                foreach (Vector2Int neighbor in neighbors) {
                    if (!heights.ContainsKey(screenIndex + neighbor)) {
                        heights.Add(screenIndex + neighbor, HexWorld.HexWorld.Instance.GetFirstAt(screenIndex + neighbor, WorldObjectType.Tile)?.Location.GridHeight ?? 0);
                    }
                }

                // Encode by multiplying by 2, so we can easily reference midpoints as ints
                Vector2Int encodedCenterPoint = 2 * screenIndex;

                // Ensure the center point has a vertex/normal
                Vector3 centerLocation = WorldUtil.GridIndexToPosition(screenIndex, heights[screenIndex]);
                if (!indexMap.ContainsKey(encodedCenterPoint)) {
                    indexMap.Add(encodedCenterPoint, vertices.Count);
                    vertices.Add(centerLocation);
                    normals.Add(Vector3.zero);
                }

                // Ensure every midpoint has a vertex/normal
                foreach (Vector2Int neighbor in neighbors) {
                    if (!indexMap.ContainsKey(encodedCenterPoint + neighbor)) {
                        Vector2Int neighborIndex = screenIndex + neighbor;
                        Vector3 neighborLocation = WorldUtil.GridIndexToPosition(neighborIndex, heights[neighborIndex]);
                        indexMap.Add(encodedCenterPoint + neighbor, vertices.Count);
                        vertices.Add((centerLocation + neighborLocation) / 2f);
                        normals.Add(Vector3.zero);
                    }
                }

                // Draw triangles for the hex
                for (int i = 0; i < neighbors.Length; i++) {
                    triangles.Add(indexMap[encodedCenterPoint]);
                    triangles.Add(indexMap[encodedCenterPoint + neighbors[i]]);
                    triangles.Add(indexMap[encodedCenterPoint + neighbors[(i + 1) % neighbors.Length]]);
                }

                Vector2Int[] encodedBetweenPoints = new Vector2Int[] {
                    encodedCenterPoint + FacingUtil.E + FacingUtil.SE,
                    encodedCenterPoint + FacingUtil.SE + FacingUtil.SW,
                    encodedCenterPoint + FacingUtil.SW + FacingUtil.W,
                    encodedCenterPoint + FacingUtil.W + FacingUtil.NW,
                    encodedCenterPoint + FacingUtil.NW + FacingUtil.NE,
                    encodedCenterPoint + FacingUtil.NE + FacingUtil.E
                };

                // Draw any triangles between existing hexes
                for (int i = 0; i < neighbors.Length; i++) {
                    if (indexMap.ContainsKey(encodedBetweenPoints[i])) {
                        triangles.Add(indexMap[encodedCenterPoint + neighbors[i]]);
                        triangles.Add(indexMap[encodedBetweenPoints[i]]);
                        triangles.Add(indexMap[encodedCenterPoint + neighbors[(i + 1) % neighbors.Length]]);
                        //TODO: don't duplicate triangles
                    }
                }
            }

            // Build and return the final mesh
            Mesh terrain = new Mesh {
                vertices = vertices.ToArray(),
                normals = normals.ToArray(),
                triangles = triangles.ToArray()
            };
            terrain.RecalculateNormals();
            return terrain;
        }

        public Mesh GetOutline(float width, params Vector3[] points)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> triangles = new List<int>();

            // Calculate slope between lines
            Vector3[] slopes = new Vector3[points.Length - 1];
            for (int i = 0; i < slopes.Length; i++) {
                slopes[i] = (points[i+1] - points[i]).normalized;
            }

            // Find directions relative to each points position in the outline (excluding ends)
            Vector3[] outs = new Vector3[points.Length];
            Vector3[] perpendiculars = new Vector3[points.Length];
            for (int i = 1; i < points.Length - 1; i++) {
                perpendiculars[i] = Vector3.Cross(slopes[i], slopes[i-1]).normalized;
                outs[i] = -Vector3.Cross((slopes[i] + slopes[i - 1]).normalized, perpendiculars[i]);
            }

            // Find directions relative to end points, completing the shape when they match
            if (points[0].Equals(points[points.Length - 1])) {
                perpendiculars[points.Length - 1] = perpendiculars[0] = Vector3.Cross(slopes[0], slopes[slopes.Length - 1]).normalized;
                outs[points.Length -1] = outs[0] = -Vector3.Cross((slopes[0] + slopes[slopes.Length - 1]).normalized, perpendiculars[0]);
            } else {
                outs[0] = outs[1];
                outs[outs.Length - 1] = outs[outs.Length - 2];
                perpendiculars[0] = perpendiculars[1];
                perpendiculars[perpendiculars.Length - 1] = perpendiculars[perpendiculars.Length - 2];
            }

            // Generate outline vertices, normals, and triangles for each point
            Vector3[] lastOutlineRing = null;
            for (int i = 0; i < points.Length; i++) {
                Vector3[] outlineRingNormals = new Vector3[] {
                    outs[i], //should be negative if an inward corner
                    perpendiculars[i],
                    -perpendiculars[i]
                };
                normals.AddRange(outlineRingNormals);
                Vector3[] outlineRing = new Vector3[] {
                    points[i] + outlineRingNormals[0] * width,
                    points[i] + outlineRingNormals[1] * width/2f,
                    points[i] + outlineRingNormals[2] * width/2f
                };
                vertices.AddRange(outlineRing);
                if (lastOutlineRing != null) {
                    int vertexCount = vertices.Count;
                    triangles.AddRange(new int[] { vertexCount - 5, vertexCount - 3, vertexCount - 6 });
                    triangles.AddRange(new int[] { vertexCount - 2, vertexCount - 3, vertexCount - 5 });
                    triangles.AddRange(new int[] { vertexCount - 4, vertexCount - 2, vertexCount - 5 });
                    triangles.AddRange(new int[] { vertexCount - 1, vertexCount - 2, vertexCount - 4 });
                    triangles.AddRange(new int[] { vertexCount - 6, vertexCount - 1, vertexCount - 4 });
                    triangles.AddRange(new int[] { vertexCount - 3, vertexCount - 1, vertexCount - 6 });
                }
                lastOutlineRing = outlineRing;
            }

            // Combine the final mesh
            Mesh mesh = new Mesh {
                vertices = vertices.ToArray(),
                normals = normals.ToArray(),
                triangles = triangles.ToArray()
            };
            return mesh;
        }

        public Vector3[] GetPointsFromSpokes(Vector3 center, params Vector2[] lengths)
        {
            Vector3[] points = new Vector3[lengths.Length + 1];
            for(int i = 0; i < lengths.Length; i++) {
                points[i] = center + new Vector3(
                    lengths[i].x * Mathf.Sin(2 * Mathf.PI * i / lengths.Length), 
                    lengths[i].y,
                    lengths[i].x * Mathf.Cos(2 * Mathf.PI * i / lengths.Length));
            }
            points[lengths.Length] = points[0];
            return points;
        }

        public Mesh GetDisk(Vector3 center, params Vector3[] points)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> triangles = new List<int>();

            //TODO: fiond a better way to get 'up'
            Vector3 up = Vector3.Cross(points[0], points[1]).normalized;

            // Generate outline vertices, normals, and triangles for each point to the center
            vertices.Add(center);
            normals.Add(up);
            for (int i = 0; i < points.Length; i++) {
                vertices.Add(points[i]);
                normals.Add(points[i]);
                if (vertices.Count >= 3) {
                    triangles.Add(0);
                    triangles.Add(i);
                    triangles.Add(i - 1);
                    triangles.Add(0);
                    triangles.Add(i - 1);
                    triangles.Add(i);
                }
            }

            // Finish the loop
            triangles.Add(0);
            triangles.Add(vertices.Count - 1);
            triangles.Add(vertices.Count - 2);
            triangles.Add(0);
            triangles.Add(vertices.Count - 2);
            triangles.Add(vertices.Count - 1);

            // Combine the final mesh
            Mesh mesh = new Mesh {
                vertices = vertices.ToArray(),
                normals = normals.ToArray(),
                triangles = triangles.ToArray()
            };
            return mesh;
        }

        public Mesh GetTree()
        {
            return null;
        }

        public Mesh GetTreeHighlight()
        {
            return null;
        }

        public static Mesh Combine(params Mesh[] meshes)
        {
            CombineInstance[] combiners = new CombineInstance[meshes.Length];
            for(int i = 0; i < combiners.Length; i++) {
                combiners[i] = new CombineInstance() { mesh = meshes[i], transform = Matrix4x4.identity };
            };

            Mesh combined = new Mesh();
            combined.CombineMeshes(combiners);
            return combined;
        }

        public static Mesh ScaleRotateTranslate(Mesh source, Vector3 scale, Vector3 rotate, Vector3 translate)
        {
            Vector3[] vertices = new Vector3[source.vertexCount];
            for (int i = 0; i < source.vertexCount; i++) {
                vertices[i] = Quaternion.Euler(rotate.x, rotate.y, rotate.z) * (Vector3.Scale(source.vertices[i], scale) + translate);
            }

            return new Mesh() { vertices = vertices, normals = source.normals, triangles = source.triangles };
        }

        public static Mesh AddBarnacles(Mesh source, Vector3 scale, int count)
        {
            Mesh[] meshes = new Mesh[count + 1];
            meshes[0] = source;
            Vector3 extent = source.bounds.extents;

            if (count > 0) { meshes[1] = ScaleRotateTranslate(source, scale, Vector3.zero, new Vector3(extent.x, 0, extent.z)); }
            if (count > 1) { meshes[2] = ScaleRotateTranslate(source, scale, Vector3.zero, new Vector3(extent.x, 0, -extent.z)); }
            if (count > 2) { meshes[3] = ScaleRotateTranslate(source, scale, Vector3.zero, new Vector3(-extent.x, 0, -extent.z)); }
            if (count > 3) { meshes[4] = ScaleRotateTranslate(source, scale, Vector3.zero, new Vector3(-extent.x, 0, extent.z)); }

            return Combine(meshes);
        }

        private Vector3 CalcHexPoint(Vector2 hex, Vector2 corner, float height, Vector3 scale)
        {
            return new Vector3(scale.x * (hex.x + corner.x), scale.y * height, scale.z * (hex.y + corner.y));
        }

        public static Mesh GetLayered(Mesh mesh, int layerCount, Vector3 scale, Vector3 rotate, Vector3 translate)
        {
            List<Mesh> meshes = new List<Mesh>(layerCount);
            Mesh lastMesh = mesh;
            meshes.Add(lastMesh);

            for (int i = 1; i < layerCount; i++) {
                lastMesh = MeshMaker.ScaleRotateTranslate(lastMesh, scale, rotate, translate);
                meshes.Add(lastMesh);
            }

            Mesh combinedMesh = MeshMaker.Combine(meshes.ToArray());
            return MeshMaker.Instance.ConvertToHardEdged(combinedMesh);

        }

        public Mesh ConvertToHardEdged(Mesh mesh)
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

            Mesh hardEdged = new Mesh {
                vertices = vertices,
                normals = normals,
                triangles = triangles
            };
            hardEdged.RecalculateNormals();
            return hardEdged;
        }
    }
}