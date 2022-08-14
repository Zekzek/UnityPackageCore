using UnityEngine;

public class CombatCanvas : MonoBehaviour
{
    [SerializeField] private MenuTextColumn _textColumnPrefab;

    private MenuTextColumn _textColumn;

    private void Start()
    {
        MenuTextColumn.InitPrefab(_textColumnPrefab);
        _textColumn = Instantiate(_textColumnPrefab, transform);
        _textColumn.Set(0);
    }

    private float timer = 0;
    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > 0.5f) {
            timer -= 0.5f;

            int choice = Random.Range(0, 5);
            if (choice == 0) {
                _textColumn.HandleDown();
            } else if (choice == 1) {
                _textColumn.HandleUp();
            } else if (choice == 2) {
                _textColumn.HandleExpand();
            } else if (choice == 3 || choice == 4) {
                _textColumn.HandleCollapse();
            }
        }
    }
}
