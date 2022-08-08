using UnityEngine;

namespace Zekzek.UnityModelMaker
{
    public class GeneratedMesh : MonoBehaviour
    {
        private void Start()
        {
            gameObject.AddComponent<MeshRenderer>().material = MaterialMaker.Instance.Get(Vector3.right);
            gameObject.AddComponent<MeshFilter>().mesh = MeshMaker.Instance.Get(Vector3.zero);
        }
    }
}