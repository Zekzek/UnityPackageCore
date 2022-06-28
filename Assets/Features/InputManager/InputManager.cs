using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private InputActionAsset _inputActionAsset;
    private InputActionMap _playerActionMap;

    private static InputManager _instance;
    public static InputManager Instance => _instance;

    private void Awake()
    {
        _instance = this;

        InitControls();
    }

    public InputAction Get(string key)
    {
        var rotateAction = _playerActionMap.FindAction(key);
        return rotateAction;
    }

    private void InitControls()
    {
        _inputActionAsset = new InputActionAsset();
        _playerActionMap = _inputActionAsset.AddActionMap("Player");

        // To set mode (2=analog, 1=digital, 0=digitalNormalized):
        InputAction moveAction = _playerActionMap.AddAction("Move");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        InputAction rotateAction = _playerActionMap.AddAction("Rotate");
        rotateAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");
        
        rotateAction.Enable();
        moveAction.Enable();
    }
}
