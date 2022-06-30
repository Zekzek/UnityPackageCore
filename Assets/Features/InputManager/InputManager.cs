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
        Tap
    }

    private static InputManager _instance;
    public static InputManager Instance => _instance;

    private readonly IDictionary<PlayerAction, InputAction> _playerActionMap = new Dictionary<PlayerAction, InputAction>();

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
    }
}
