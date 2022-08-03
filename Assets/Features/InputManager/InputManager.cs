using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private const string UP = "Up";
    private const string DOWN = "Down";
    private const string LEFT = "Left";
    private const string RIGHT = "Right";

    public enum PlayerAction
    {
        Move,
        Rotate,
        Tap,
        Action
    }

    private static InputManager _instance;
    public static InputManager Instance => _instance;

    private readonly IDictionary<PlayerAction, InputAction> _playerActionMap = new Dictionary<PlayerAction, InputAction>();
    private readonly IList<PlayerAction> _playerActionStart = new List<PlayerAction>();
    private readonly IList<PlayerAction> _playerActionFinish = new List<PlayerAction>();

    private void Awake()
    {
        _instance = this;
        InitControls();
    }

    public Vector2 GetCursorPosition()
    {
        return Mouse.current.position.ReadValue();
    }

    public T Get<T>(PlayerAction key) where T : struct
    {
        if (_playerActionMap.ContainsKey(key)) { return _playerActionMap[key].ReadValue<T>(); }
        return default;
    }

    public bool IsStarted(PlayerAction actiontype)
    {
        bool started = _playerActionStart.Contains(actiontype);
        _playerActionStart.Remove(actiontype);
        return started;
    }

    public bool IsFinished(PlayerAction actiontype)
    {
        bool finished = _playerActionFinish.Contains(actiontype);
        _playerActionFinish.Remove(actiontype);
        return finished;
    }


    private void InitControls()
    {
        InputAction moveAction = new InputAction();
        moveAction.AddCompositeBinding("2DVector")
            .With(UP, "<Keyboard>/w")
            .With(DOWN, "<Keyboard>/s")
            .With(LEFT, "<Keyboard>/a")
            .With(RIGHT, "<Keyboard>/d");
        _playerActionMap.Add(PlayerAction.Move, moveAction);
        moveAction.Enable();

        InputAction rotateAction = new InputAction();
        rotateAction.AddCompositeBinding("2DVector")
            .With(UP, "<Keyboard>/upArrow")
            .With(DOWN, "<Keyboard>/downArrow")
            .With(LEFT, "<Keyboard>/leftArrow")
            .With(RIGHT, "<Keyboard>/rightArrow");
        _playerActionMap.Add(PlayerAction.Rotate, rotateAction);
        rotateAction.Enable();

        InputAction tapAction = new InputAction();
        tapAction.AddBinding("<Mouse>/leftButton");
        _playerActionMap.Add(PlayerAction.Tap, tapAction);
        tapAction.Enable();

        InputAction actionAction = new InputAction();
        actionAction.AddBinding("<Keyboard>/space");
        actionAction.started += (_) => Begin(PlayerAction.Action);
        actionAction.canceled += (_) => Finish(PlayerAction.Action);
        _playerActionMap.Add(PlayerAction.Action, actionAction);
        actionAction.Enable();
    }

    private void Begin(PlayerAction actionType)
    {
        _playerActionStart.Add(actionType);
        _playerActionFinish.Remove(actionType);
    }

    private void Finish(PlayerAction actionType)
    {
        _playerActionStart.Remove(actionType);
        _playerActionFinish.Add(actionType);
    }
}
