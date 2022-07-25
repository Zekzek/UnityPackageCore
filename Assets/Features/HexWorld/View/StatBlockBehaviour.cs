using UnityEngine;

namespace Zekzek.HexWorld
{
    public class StatBlockBehaviour : MonoBehaviour
    {
        [SerializeField] FillBar healthBar;

        public virtual StatComponent Model { get; set; }


        private void Update()
        {
            if (Model == null) { return; }

            healthBar.SetFill(Model.StatBlock.GetPercent(Stats.StatType.Health));
            LookAtCamera();
        }

        private void LookAtCamera()
        {
            if (Camera.main == null) { return; }
            transform.LookAt(new Vector3(Camera.main.transform.position.x, 0, Camera.main.transform.position.z));
        }
    }
}