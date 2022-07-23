using UnityEngine;

public class FillBar : MonoBehaviour
{
    [SerializeField] private Transform fillTransform;

    private Vector3 _startScale;

    private void Start()
    {
        _startScale = fillTransform.localScale;
    }

    public void SetFill(float percent)
    {
        fillTransform.localScale = new Vector3(percent * _startScale.x, _startScale.y, _startScale.z);
    }
}
