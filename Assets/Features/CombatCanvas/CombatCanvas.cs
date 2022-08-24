using UnityEngine;
using Zekzek.Ability;
using Zekzek.HexWorld;
using static InputManager;

public class CombatCanvas : MonoBehaviour
{
    [SerializeField] private MenuTextColumn _textColumnPrefab;

    private MenuTextColumn _textColumn;
    private static CombatCanvas _instance;
    private WorldObject _user;

    public static void Set(WorldObject user)
    {
        _instance._user = user;
        _instance._textColumn.Set(user.Ability, null);
    }

    private void Awake()
    {
        MenuTextColumn.InitPrefab(_textColumnPrefab);
        _textColumn = Instantiate(_textColumnPrefab, transform);
        _instance = this;
    }

    private void Start()
    {
        InputManager.Instance.AddListener<Vector2>(PlayerAction.Rotate, InputWatchType.OnStart, OnMove);
        InputManager.Instance.AddListener<float>(PlayerAction.Action, InputWatchType.OnStart, OnAction);
    }

    private void OnMove(Vector2 input)
    {
        // Deadzone, should this be handled in InputManager?
        if (input.sqrMagnitude<0.1f) { return; }

        if (input.x * input.x > input.y * input.y) {
            if (input.x > 0) {
                //_textColumn.HandleExpand();
                OnAction(1);
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
            if (_textColumn.CanExpand()) {
                _textColumn.HandleExpand();
            } else {
                AbilityData abilityData = _user.Ability.GetAt(_textColumn.GetSelectedLocation());
                HexWorldBehaviour.Instance.UpdateHighlight(_user.Location.Current.GridIndex, _user.Location.Current.RotationAngle, abilityData.Spread, abilityData.Reach);
                // TODO: on subsequent use, activate ability
            }
        }
    }
}
