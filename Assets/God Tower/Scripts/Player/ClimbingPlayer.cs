using UnityEngine;

[RequireComponent(typeof(ClimbingInput))]
public class ClimbingPlayer : MonoBehaviour
{
    [Header("Tower")]
    [SerializeField] private Transform towerCenter;
    [SerializeField] private float towerRadius = 2.15f;

    [Header("Climbing")]
    [SerializeField] private float climbSpeed = 1.5f;
    [SerializeField] private float angularSpeed = 180f;
    [SerializeField] private float grabPauseDuration = 0.12f;

    [Header("Climbing Steps")]
    [SerializeField] private float stepHeight = 0.75f;
    [SerializeField] private float stepDuration = 0.3f;

    [Header("Hand Targets")]
    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private Transform rightHandTarget;
    [SerializeField] private float handReachArc = 0.15f;

    [Header("Climbing Timing")]
    [SerializeField] private float climbStepDuration = 0.55f;
    [SerializeField, Range(0f, 1f)] private float reachEnd = 0.35f;
    [SerializeField, Range(0f, 1f)] private float pullEnd = 0.85f;
    [SerializeField] private float anticipationDip = 0.04f;

    [Header("Handholds")]
    [SerializeField] private TowerHandholdGenerator handholdGenerator;
    [SerializeField] private float maxVerticalReach = 0.85f;
    [SerializeField] private float playerSurfaceOffset = 0.35f;
    [SerializeField] private float movementSpeed = 2.5f;

    private Handhold currentHandhold;
    private Handhold targetHandhold;
    private float targetAngle;
    private bool isMovingToHandhold;

    private Vector3 handReachStartPosition;
    private Quaternion handReachStartRotation;

    private Transform ActiveHandTarget =>
        activeHand == ClimbingHand.Left
            ? leftHandTarget
            : rightHandTarget;

    private ClimbingInput input;
    private ClimbingHand activeHand = ClimbingHand.Left;

    private Handhold leftHandhold;
    private Handhold rightHandhold;

    private float grabPauseTimer;
    private float targetHeight;
    private float height;
    private float angle;

    private float stepTimer;

    private float movementProgress;
    private float startHeight;
    private float startAngle;

    private void Awake()
    {
        input = GetComponent<ClimbingInput>();

        Vector3 offset = transform.position - towerCenter.position;

        height = transform.position.y - towerCenter.position.y;
        angle = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
        targetHeight = height;

        currentHandhold =
            handholdGenerator.FindStartingHandhold(
                height,
                angle
            );

        if (currentHandhold != null)
        {
            height = currentHandhold.Height;
            angle = currentHandhold.Angle;
        }
    }

    private void Start()
    {
        InitializeHandTargets();
    }

    private void Update()
    {
        UpdateGrabPause();
        HandleClimbing();
        MoveTowardsHandhold();

        UpdatePosition();
        UpdateRotation();

        UpdateGrabbedHands();
    }

    private void InitializeHandTargets()
    {
        if (currentHandhold == null)
            return;

        leftHandhold = currentHandhold;
        rightHandhold = currentHandhold;

        leftHandTarget.SetPositionAndRotation(
            currentHandhold.GrabPosition,
            currentHandhold.GrabRotation
        );

        rightHandTarget.SetPositionAndRotation(
            currentHandhold.GrabPosition,
            currentHandhold.GrabRotation
        );
    }

    private void HandleHorizontalMovement()
    {
        //float horizontal = input.Horizontal;
        //angle += horizontal * horizontalSpeed * Time.deltaTime;

        // Horizontal input is now used when selecting
        // the next handhold rather than directly rotating
        // around the tower.
    }

    private void HandleClimbing()
    {
        if (isMovingToHandhold)
            return;

        if (IsInGrabPause())
            return;

        if (!input.ClimbHeld)
            return;

        TryStartClimb();
    }

    private void TryStartClimb()
    {
        Handhold nextHandhold =
        handholdGenerator.FindNextHandhold(
            height,
            angle,
            input.Horizontal,
            maxVerticalReach
        );

        if (nextHandhold == null)
            return;

        targetHandhold = nextHandhold;

        startHeight = height;
        startAngle = angle;

        targetHeight = targetHandhold.Height;
        targetAngle = targetHandhold.Angle;

        movementProgress = 0f;
        handReachStartPosition =
            ActiveHandTarget.position;

        handReachStartRotation =
            ActiveHandTarget.rotation;

        isMovingToHandhold = true;
    }

    private void MoveTowardsHandhold()
    {
        if (!isMovingToHandhold)
            return;

        movementProgress +=
            Time.deltaTime / climbStepDuration;

        float t =
            Mathf.Clamp01(movementProgress);

        // -------------------------
        // HAND
        // -------------------------

        UpdateHandReach(t);

        // -------------------------
        // BODY
        // -------------------------

        float bodyProgress =
            GetBodyProgress(t);

        height =
            Mathf.Lerp(
                startHeight,
                targetHeight,
                bodyProgress
            );

        float angleDifference =
            Mathf.DeltaAngle(
                startAngle,
                targetAngle
            );

        angle =
            startAngle +
            angleDifference * bodyProgress;

        if (t < reachEnd)
        {
            float reachProgress =
                Mathf.InverseLerp(
                    0f,
                    reachEnd,
                    t
                );

            float dip =
                Mathf.Sin(reachProgress * Mathf.PI)
                * anticipationDip;

            height -= dip;
        }

        // -------------------------
        // COMPLETE
        // -------------------------

        if (t >= 1f)
        {
            height = targetHeight;
            angle = targetAngle;

            currentHandhold = targetHandhold;

            SetActiveHandhold(targetHandhold);

            targetHandhold = null;

            isMovingToHandhold = false;

            grabPauseTimer = grabPauseDuration;

            activeHand =
                activeHand == ClimbingHand.Left
                    ? ClimbingHand.Right
                    : ClimbingHand.Left;
        }
    }

    private void UpdateGrabPause()
    {
        if (grabPauseTimer <= 0f)
            return;

        grabPauseTimer -= Time.deltaTime;

        if (grabPauseTimer < 0f)
            grabPauseTimer = 0f;
    }

    private void UpdatePosition()
    {
        Vector3 center = towerCenter.position;

        float radians = angle * Mathf.Deg2Rad;

        Vector3 outward = new Vector3(
            Mathf.Sin(radians),
            0f,
            Mathf.Cos(radians)
        );

        Vector3 position =
            center +
            outward * (towerRadius + playerSurfaceOffset);

        position.y += height;

        transform.position = position;
    }

    private void UpdateRotation()
    {
        Vector3 outward =
            transform.position - towerCenter.position;

        outward.y = 0f;

        if (outward.sqrMagnitude < 0.001f)
            return;

        transform.rotation =
            Quaternion.LookRotation(
                -outward.normalized,
                Vector3.up
            );
    }

    private void UpdateHandReach(float stepProgress)
    {
        if (targetHandhold == null)
            return;

        Transform handTarget = ActiveHandTarget;

        float handProgress =
            GetHandProgress(stepProgress);

        float easedProgress =
            Mathf.SmoothStep(
                0f,
                1f,
                handProgress
            );

        Vector3 targetPosition =
            targetHandhold.GrabPosition;

        Vector3 position =
            Vector3.Lerp(
                handReachStartPosition,
                targetPosition,
                easedProgress
            );

        // Arc only exists while reaching.
        float arc =
            Mathf.Sin(handProgress * Mathf.PI)
            * handReachArc;

        position += Vector3.up * arc;

        handTarget.position = position;

        handTarget.rotation =
            Quaternion.Slerp(
                handReachStartRotation,
                targetHandhold.GrabRotation,
                easedProgress
            );
    }

    private void UpdateGrabbedHands()
    {
        if (leftHandhold != null &&
            activeHand != ClimbingHand.Left)
        {
            leftHandTarget.position =
                leftHandhold.GrabPosition;

            leftHandTarget.rotation =
                leftHandhold.GrabRotation;
        }

        if (rightHandhold != null &&
            activeHand != ClimbingHand.Right)
        {
            rightHandTarget.position =
                rightHandhold.GrabPosition;

            rightHandTarget.rotation =
                rightHandhold.GrabRotation;
        }
    }

    #region HELPER FUNCTIONS
    private Handhold GetActiveHandhold()
    {
        return activeHand == ClimbingHand.Left
            ? leftHandhold
            : rightHandhold;
    }

    private void SetActiveHandhold(Handhold handhold)
    {
        if (activeHand == ClimbingHand.Left)
            leftHandhold = handhold;
        else
            rightHandhold = handhold;
    }

    private float GetHandProgress(float stepProgress)
    {
        return Mathf.InverseLerp(
            0f,
            reachEnd,
            stepProgress
        );
    }

    private float GetBodyProgress(float stepProgress)
    {
        if (stepProgress <= reachEnd)
            return 0f;

        if (stepProgress <= pullEnd)
        {
            float pullProgress =
                Mathf.InverseLerp(
                    reachEnd,
                    pullEnd,
                    stepProgress
                );

            // Reach about 92% of the destination during the pull.
            return Mathf.SmoothStep(
                0f,
                0.92f,
                pullProgress
            );
        }

        float settleProgress =
            Mathf.InverseLerp(
                pullEnd,
                1f,
                stepProgress
            );

        return Mathf.Lerp(
            0.92f,
            1f,
            Mathf.SmoothStep(
                0f,
                1f,
                settleProgress
            )
        );
    }

    private bool IsInGrabPause()
    {
        return grabPauseTimer > 0f;
    }
    #endregion
}

#region HELPER CLASSES

public enum ClimbingHand
{
    Left,
    Right
}

#endregion