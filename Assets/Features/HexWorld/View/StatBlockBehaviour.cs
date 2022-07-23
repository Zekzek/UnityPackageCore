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

            healthBar.SetFill(Model.StatBlock.GetTotalValue(Stats.StatType.Health) / Model.StatBlock.GetTotalValue(Stats.StatType.Health));
            LookAtCamera();
        }

        private void LookAtCamera()
        {
            if (Camera.main == null) { return; }
            transform.LookAt(Camera.main.transform.position);
        }
    }
}