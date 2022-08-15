using UnityEngine;
using Zekzek.Ability;

public class CombatCanvas : MonoBehaviour
{
    [SerializeField] private MenuTextColumn _textColumnPrefab;

    private MenuTextColumn _textColumn;
    private static CombatCanvas _instance;

    public static void Set(AbilityComponent component)
    {
        _instance._textColumn.Set(component, null);
    }

    private void Awake()
    {
        MenuTextColumn.InitPrefab(_textColumnPrefab);
        _textColumn = Instantiate(_textColumnPrefab, transform);
        _instance = this;
    }

    private float timer = 0;
    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > 0.5f) {
            timer -= 0.5f;

            int choice = Random.Range(0, 4);
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
