using UnityEngine;

public class CombatCanvas : MonoBehaviour
{
    [SerializeField] private MenuTextColumn _textColumnPrefab;

    private MenuTextColumn _textColumn;

    private void Start()
    {
        MenuTextColumn.Init(_textColumnPrefab);
        _textColumn = Instantiate(_textColumnPrefab, transform);
    }

    private bool action = true;
    private float timer = 0;
    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > 0.5f) {
            timer -= 0.5f;

            if (action) {
                _textColumn.HandleDown();
            } else {
                _textColumn.HandleExpand();
            }
            action = !action;
        }
    }
}
