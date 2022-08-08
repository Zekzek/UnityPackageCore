using System.Collections.Generic;
using UnityEngine;

namespace Zekzek.UnityModelMaker
{
    public class MeshMaker
    {
        private const float STEP_SIZE = 0.1f;

        private readonly IDictionary<Vector3Int, Mesh> _meshes = new Dictionary<Vector3Int, Mesh>();

        // Singleton
        private static MeshMaker _instance;
        public static MeshMaker Instance { get { if (_instance == null) { _instance = new MeshMaker(); } return _instance; } }
        private MeshMaker() { }

        public Mesh Get(Vector3 color)
        {
            Mesh mesh = new Mesh {
                vertices = new Vector3[] {
                    new Vector3(0, 1, 0),     //center
                    new Vector3(0, 1, 0),     //center
                    new Vector3(0, 1, 0),     //center
                    new Vector3(0, 1, 0),     //center
                    new Vector3(-1, 0, 0),    //west
                    new Vector3(0, 0, 1),     //north
                    new Vector3(1, 0, 0),     //east
                    new Vector3(0, 0, -1),    //south
                },
                normals = new Vector3[] {
                    new Vector3(-1, 1, 1),    //NW-center
                    new Vector3(1, 1, 1),     //NE-center
                    new Vector3(1, 1, -1),    //SE-center
                    new Vector3(-1, 1, -1),   //SW-center
                    new Vector3(-1, 0, 0),    //west
                    new Vector3(0, 0, 1),     //north
                    new Vector3(1, 0, 0),     //east
                    new Vector3(0, 0, -1),    //south
                },
                triangles = new int[] {
                    0, 4, 5,  //NW
                    1, 5, 6,  //NE
                    2, 6, 7,  //SE
                    3, 7, 4   //SW
                }
            };

            return mesh;
        }
    }
}