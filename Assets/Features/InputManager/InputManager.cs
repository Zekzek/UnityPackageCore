using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private const string UP = "Up";
    private const string DOWN = "Down";
    private const string LEFT = "Left";
    private const string RIGHT = "Right";

    public enum InputWatchType
    {
        Constant,
        OnStart,
        OnFinish
    }
    public enum InputMode
    {
        WorldNavigation,
        CombatMenu,
        CombatTargeting
    }
    public enum PlayerAction
    {
        Move,
        Rotate,
        Tap,
        Action,
        Back
    }

    public InputMode Mode { get; private set; } = InputMode.CombatMenu;

    private static InputManager _instance;
    public static InputManager Instance => _instance;

    private readonly IDictionary<Enum, InputAction> _actionMap = new Dictionary<Enum, InputAction>();
    private readonly IList<Enum> _playerActionStart = new List<Enum>();
    private readonly IList<Enum> _playerActionFinish = new List<Enum>();
    private readonly IDictionary<Enum, List<object>> _callbacks = new Dictionary<Enum, List<object>>();
    private readonly IList<InputMode> _modeHistory = new List<InputMode>();

    public Vector2 GetCursorPosition()
    {
        return Mouse.current.position.ReadValue();
    }

    public void AddListener<T>(InputMode mode, Enum action, InputWatchType watchType, Action<T> callback) where T : struct
    {
        if (!_actionMap.ContainsKey(action)) {
            Debug.LogError("No action defined for: " + action);
            return;
        }

        if (!_callbacks.ContainsKey(action)) { _callbacks.Add(action, new List<object>()); }
        _callbacks[action].Add(new InputCallback<T> { Mode = mode, WatchType = watchType, Callback = callback });
    }

    public void PushMode(InputMode mode)
    {
        _modeHistory.Add(Mode);
        Mode = mode;
    }

    public void PopMode()
    {
        int lastIndex = _modeHistory.Count - 1;
        Mode = _modeHistory[lastIndex];
        _modeHistory.RemoveAt(lastIndex);
    }

    private void Awake()
    {
        _instance = this;
        InitControls();
    }

    private void FixedUpdate()
    {
        foreach (KeyValuePair<Enum, List<object>> callbackPair in _callbacks) {
            foreach (object callbackObject in callbackPair.Value) {
                if (callbackObject is InputCallback<Vector2> vector2Callback) {
                    ProcessCallback(callbackPair.Key, vector2Callback);
                } else if (callbackObject is InputCallback<float> floatCallback) {
                    ProcessCallback(callbackPair.Key, floatCallback);
                } else {
                    Debug.LogWarning($"Unable to fire callback for {callbackObject.GetType()}. Does InputManager need another handler?");
                }
            }
        }
        _playerActionStart.Clear();
        _playerActionFinish.Clear();
    }

    private void ProcessCallback<T>(Enum key, InputCallback<T> callback) where T : struct
    {
        if (callback.Mode != Mode) { return; }
        if (callback.WatchType == InputWatchType.OnStart && !IsStarted(key)) { return; }
        if (callback.WatchType == InputWatchType.OnFinish && !IsFinished(key)) { return; }

        T input = _actionMap[key].ReadValue<T>();
        callback.Callback.Invoke(input);
    }

    private bool IsStarted(Enum actiontype)
    {
        return _playerActionStart.Contains(actiontype);
    }

    private bool IsFinished(Enum actiontype)
    {
        return _playerActionFinish.Contains(actiontype);
    }

    private void InitControls()
    {
        Create2DVectorControl(PlayerAction.Move, up: "<Keyboard>/w", down: "<Keyboard>/s", left: "<Keyboard>/a", right: "<Keyboard>/d");
        Create2DVectorControl(PlayerAction.Rotate, "<Keyboard>/upArrow", down: "<Keyboard>/downArrow", left: "<Keyboard>/leftArrow", right: "<Keyboard>/rightArrow");
        CreateFloatControl(PlayerAction.Tap, "<Mouse>/leftButton");
        CreateFloatControl(PlayerAction.Action, "<Keyboard>/space");
        CreateFloatControl(PlayerAction.Back, "<Keyboard>/escape");
    }

    private void Create2DVectorControl(Enum key, string up, string down, string left, string right)
    {
        InputAction action = new InputAction();
        action.AddCompositeBinding("2DVector").With(UP, up).With(DOWN, down).With(LEFT, left).With(RIGHT, right);
        InitInputFor(key, action);
    }

    private void CreateFloatControl(Enum key, string button)
    {
        InputAction action = new InputAction();
        action.AddBinding(button);
        InitInputFor(key, action);
    }

    private InputAction InitInputFor(Enum key, InputAction action)
    {
        action.started += (_) => Begin(key);
        action.canceled += (_) => Finish(key);
        _actionMap.Add(key, action);
        action.Enable();
        return action;
    }

    private void Begin(Enum actionType)
    {
        _playerActionStart.Add(actionType);
        _playerActionFinish.Remove(actionType);
    }

    private void Finish(Enum actionType)
    {
        _playerActionStart.Remove(actionType);
        _playerActionFinish.Add(actionType);
    }

    private class InputCallback<T> where T : struct
    {
        public InputMode Mode { get; set; }
        public InputWatchType WatchType { get; set; }
        public Action<T> Callback { get; set; }
    }
}