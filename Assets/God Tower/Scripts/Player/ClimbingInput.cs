using UnityEngine;
using UnityEngine.InputSystem;

public class ClimbingInput : MonoBehaviour
{
    public bool ClimbHeld { get; private set; }
    public float Horizontal { get; private set; }

    private InputAction climbAction;
    private InputAction horizontalAction;

    private void Awake()
    {
        climbAction = new InputAction(
            name: "Climb",
            type: InputActionType.Button
        );

        // Keyboard + mouse + touch.
        climbAction.AddBinding("<Keyboard>/space");
        climbAction.AddBinding("<Mouse>/leftButton");
        climbAction.AddBinding("<Touchscreen>/primaryTouch/press");

        horizontalAction = new InputAction(
            name: "Horizontal",
            type: InputActionType.Value
        );

        horizontalAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/a")
            .With("Positive", "<Keyboard>/d");

        horizontalAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/leftArrow")
            .With("Positive", "<Keyboard>/rightArrow");
    }

    private void OnEnable()
    {
        climbAction.Enable();
        horizontalAction.Enable();
    }

    private void OnDisable()
    {
        climbAction.Disable();
        horizontalAction.Disable();
    }

    private void Update()
    {
        ClimbHeld = climbAction.IsPressed();
        Horizontal = horizontalAction.ReadValue<float>();
    }
}
