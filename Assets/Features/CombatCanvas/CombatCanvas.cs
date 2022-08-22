using UnityEngine;
using Zekzek.Ability;
using static InputManager;

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

    private void Start()
    {
        InputManager.Instance.AddListener<Vector2>(PlayerAction.Move, InputWatchType.OnStart, OnMove);
        InputManager.Instance.AddListener<float>(PlayerAction.Action, InputWatchType.OnStart, OnAction);
    }

    private void OnMove(Vector2 input)
    {
        // Deadzone, should this be handled in InputManager?
        if (input.sqrMagnitude<0.1f) { return; }

        if (input.x * input.x > input.y * input.y) {
            if (input.x > 0) { 
                _textColumn.HandleExpand(); 
            } else {
                _textColumn.HandleCollapse();
            }
        } else {
            if (input.y > 0) {
                _textColumn.HandleUp();
            } else {
                _textColumn.HandleDown();
            }
        }
    }

    private void OnAction(float value)
    {
        if (value > 0.5f) {
            //_textColumn.Activate()
        }
    }
}
