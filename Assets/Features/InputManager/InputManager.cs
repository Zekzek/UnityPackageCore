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
    public enum PlayerAction
    {
        Move,
        Rotate,
        Tap,
        Action
    }

    private static InputManager _instance;
    public static InputManager Instance => _instance;

    private readonly IDictionary<Enum, InputAction> _actionMap = new Dictionary<Enum, InputAction>();
    private readonly IList<Enum> _playerActionStart = new List<Enum>();
    private readonly IList<Enum> _playerActionFinish = new List<Enum>();

    private readonly IDictionary<Enum, List<object>> _callbacks = new Dictionary<Enum, List<object>>();

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
    }

    private void ProcessCallback<T>(Enum key, InputCallback<T> callback) where T : struct
    {
        if (callback.WatchType == InputWatchType.OnStart && !IsStarted(key)) { return; }
        if (callback.WatchType == InputWatchType.OnFinish && !IsFinished(key)) { return; }

        T input = _actionMap[key].ReadValue<T>();
        callback.Callback.Invoke(input);
    }

    public Vector2 GetCursorPosition()
    {
        return Mouse.current.position.ReadValue();
    }

    public T Get<T>(PlayerAction key) where T : struct
    {
        if (_actionMap.ContainsKey(key)) { return _actionMap[key].ReadValue<T>(); }
        return default;
    }

    public bool IsStarted(Enum actiontype)
    {
        bool started = _playerActionStart.Contains(actiontype);
        _playerActionStart.Remove(actiontype);
        return started;
    }

    public bool IsFinished(Enum actiontype)
    {
        bool finished = _playerActionFinish.Contains(actiontype);
        _playerActionFinish.Remove(actiontype);
        return finished;
    }

    public void AddListener<T>(Enum action, InputWatchType watchType, Action<T> callback) where T : struct
    {
        if (!_actionMap.ContainsKey(action)) {
            Debug.LogError("No action defined for: " + action);
            return;
        }

        if (!_callbacks.ContainsKey(action)) { _callbacks.Add(action, new List<object>()); }
        _callbacks[action].Add(new InputCallback<T> { WatchType = watchType, Callback = callback });
    }

    private void InitControls()
    {
        InputAction moveAction = new InputAction();
        moveAction.AddCompositeBinding("2DVector")
            .With(UP, "<Keyboard>/w")
            .With(DOWN, "<Keyboard>/s")
            .With(LEFT, "<Keyboard>/a")
            .With(RIGHT, "<Keyboard>/d");
        Init(PlayerAction.Move, moveAction);

        InputAction rotateAction = new InputAction();
        rotateAction.AddCompositeBinding("2DVector")
            .With(UP, "<Keyboard>/upArrow")
            .With(DOWN, "<Keyboard>/downArrow")
            .With(LEFT, "<Keyboard>/leftArrow")
            .With(RIGHT, "<Keyboard>/rightArrow");
        Init(PlayerAction.Rotate, rotateAction);

        InputAction tapAction = new InputAction();
        tapAction.AddBinding("<Mouse>/leftButton");
        Init(PlayerAction.Tap, tapAction);

        InputAction actionAction = new InputAction();
        actionAction.AddBinding("<Keyboard>/space");
        Init(PlayerAction.Action, actionAction);
    }

    private void Init(Enum key, InputAction action)
    {
        action.started += (_) => Begin(key);
        action.canceled += (_) => Finish(key);
        _actionMap.Add(key, action);
        action.Enable();
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
        public InputWatchType WatchType { get; set; }
        public Action<T> Callback { get; set; }
    }
}