using UnityEngine;
using UnityEngine.InputSystem;

public class ClimbingInput : MonoBehaviour
{
    [Header("Swipe")]
    [SerializeField] private float swipeThresholdNormalized = 0.03f;

    public bool ClimbHeld { get; private set; }
    public float Horizontal { get; private set; }

    private InputAction climbAction;
    private InputAction horizontalAction;

    private Vector2 touchStartPosition;
    private bool wasTouching;

    private float horizontalIntent;

    private void Awake()
    {
        CreateInputActions();
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
        UpdateClimbInput();
        UpdateSwipeInput();
        UpdateHorizontalInput();

        Horizontal = horizontalIntent;
    }

    private void CreateInputActions()
    {
        // -------------------------
        // CLIMB
        // -------------------------

        climbAction = new InputAction(
            "Climb",
            InputActionType.Button
        );

        climbAction.AddBinding("<Keyboard>/space");
        climbAction.AddBinding("<Mouse>/leftButton");
        climbAction.AddBinding("<Touchscreen>/primaryTouch/press");

        // -------------------------
        // HORIZONTAL
        // -------------------------

        horizontalAction = new InputAction(
            "Horizontal",
            InputActionType.Value
        );

        horizontalAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/a")
            .With("Positive", "<Keyboard>/d");

        horizontalAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/leftArrow")
            .With("Positive", "<Keyboard>/rightArrow");
    }

    private void UpdateClimbInput()
    {
        bool climbPressed =
            climbAction.IsPressed();

        bool horizontalKeyboardPressed = false;

        if (Keyboard.current != null)
        {
            horizontalKeyboardPressed =
                Keyboard.current.aKey.isPressed ||
                Keyboard.current.dKey.isPressed ||
                Keyboard.current.leftArrowKey.isPressed ||
                Keyboard.current.rightArrowKey.isPressed;
        }

        ClimbHeld =
            climbPressed ||
            horizontalKeyboardPressed;
    }

    private void UpdateHorizontalInput()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed)
        {
            horizontalIntent = 1f;
            Horizontal = 1f;
        }
        else if (Keyboard.current.dKey.isPressed ||
                 Keyboard.current.rightArrowKey.isPressed)
        {
            horizontalIntent = -1f;
            Horizontal = -1f;
        }
    }

    private void UpdateSwipeInput()
    {
        Touchscreen touchscreen =
            Touchscreen.current;

        if (touchscreen == null)
            return;

        var touch = touchscreen.primaryTouch;

        bool isPressed =
            touch.press.isPressed;

        Vector2 currentPosition =
            touch.position.ReadValue();

        if (isPressed && !wasTouching)
        {
            touchStartPosition =
                currentPosition;

            wasTouching = true;
        }
        else if (isPressed && wasTouching)
        {
            Vector2 totalDrag =
                currentPosition -
                touchStartPosition;

            if (DetectSwipe(totalDrag))
            {
                // Start measuring the next gesture
                // from the current finger position.
                touchStartPosition =
                    currentPosition;
            }
        }
        else if (!isPressed && wasTouching)
        {
            wasTouching = false;
        }
    }

    private bool DetectSwipe(Vector2 delta)
    {
        float normalizedDelta =
            delta.x / Screen.width;

        if (Mathf.Abs(normalizedDelta) <
            swipeThresholdNormalized)
        {
            return false;
        }

        horizontalIntent =
            Mathf.Sign(normalizedDelta);

        Horizontal =
            horizontalIntent;

        return true;
    }

    public void ConsumeHorizontalIntent()
    {
        horizontalIntent = 0f;
        Horizontal = 0f;
    }
}