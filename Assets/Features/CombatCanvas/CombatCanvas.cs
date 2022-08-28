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
    private AbilityData _abilityData;
    private WorldLocation _targetLocation;

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
        InputManager.Instance.AddListener<Vector2>(InputMode.CombatMenu, PlayerAction.Move, InputWatchType.OnStart, OnMenuMove);
        InputManager.Instance.AddListener<Vector2>(InputMode.CombatMenu, PlayerAction.Rotate, InputWatchType.OnStart, OnMenuMove);
        InputManager.Instance.AddListener<float>(InputMode.CombatMenu, PlayerAction.Action, InputWatchType.OnStart, OnMenuAction);
        InputManager.Instance.AddListener<float>(InputMode.CombatMenu, PlayerAction.Back, InputWatchType.OnStart, OnMenuBack);

        InputManager.Instance.AddListener<Vector2>(InputMode.CombatTargeting, PlayerAction.Move, InputWatchType.OnStart, OnTargetingMove);
        InputManager.Instance.AddListener<Vector2>(InputMode.CombatTargeting, PlayerAction.Rotate, InputWatchType.OnStart, OnTargetingRotate);
        InputManager.Instance.AddListener<float>(InputMode.CombatTargeting, PlayerAction.Action, InputWatchType.OnStart, OnTargetingAction);
        InputManager.Instance.AddListener<float>(InputMode.CombatTargeting, PlayerAction.Back, InputWatchType.OnStart, OnTargetingBack);
    }

    private void OnMenuMove(Vector2 input)
    {
        if (input.y > 0.5f) {
            _textColumn.HandleUp();
        } else if (input.y < -0.5f) {
            _textColumn.HandleDown();
        }
    }

    private void OnMenuAction(float value)
    {
        if (value > 0.5f) {
            if (_textColumn.CanExpand()) {
                _textColumn.HandleExpand();
            } else {
                _abilityData = _user.Ability.GetAt(_textColumn.GetSelectedLocation());
                _targetLocation = _user.Location.Current.MoveForward(_abilityData.Range);
                DrawHighlight();
                InputManager.Instance.PushMode(InputManager.InputMode.CombatTargeting);
            }
        }
    }

    private void OnMenuBack(float value)
    {
        _textColumn.HandleCollapse();
    }

    private void OnTargetingMove(Vector2 value)
    {
        Vector2Int updatedGridIndex = _targetLocation.GridIndex;
        if (value.x > 0.5f) {
            updatedGridIndex += FacingUtil.E;
        } else if (value.x < -0.5f) {
            updatedGridIndex += FacingUtil.W;
        }
        if (value.y > 0.5f) {
            updatedGridIndex += FacingUtil.NE;
        } else if (value.y < -0.5f) {
            updatedGridIndex += FacingUtil.SW;
        }

        int updatedRange = WorldUtil.FindDistance(_user.Location.Current.GridIndex, updatedGridIndex);
        if (updatedRange <= _abilityData.Range) {
            _targetLocation = new WorldLocation(new Vector3Int(updatedGridIndex.x, 0, updatedGridIndex.y), _targetLocation.RotationAngle);
            DrawHighlight();
        }
    }

    private void OnTargetingRotate(Vector2 value)
    {
        if (value.x > 0.5f) {
            _targetLocation = _targetLocation.RotateRight();
            DrawHighlight();
        } else if (value.x < -0.5f) {
            _targetLocation = _targetLocation.RotateLeft();
            DrawHighlight();
        }
    }

    private void OnTargetingAction(float value)
    {
        //TODO
    }

    private void OnTargetingBack(float value)
    {
        HexWorldBehaviour.Instance.ClearHighlight();
        InputManager.Instance.PopMode();
    }

    private void DrawHighlight()
    {
        HexWorldBehaviour.Instance.UpdateHighlight(_targetLocation.GridIndex, _targetLocation.RotationAngle, _abilityData.Spread, _abilityData.Reach);
    }
}
