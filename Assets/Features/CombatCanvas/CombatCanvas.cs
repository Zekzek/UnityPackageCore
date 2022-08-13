using UnityEngine;

public class CombatCanvas : MonoBehaviour
{
    [SerializeField] private MenuTextColumn _textColumnPrefab;

    private MenuTextColumn _textColumn;

    private void Start()
    {
        _textColumn = Instantiate(_textColumnPrefab, transform);

        _textColumn.Set(1, _textColumnPrefab);
        _textColumn.Expand(2);
    }
}
