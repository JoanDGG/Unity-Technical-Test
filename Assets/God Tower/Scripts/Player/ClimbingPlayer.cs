using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

[RequireComponent(typeof(ClimbingInput))]
public class ClimbingPlayer : MonoBehaviour
{
    [Header("Tower")]
    [SerializeField] private Transform towerCenter;
    [SerializeField] private float towerRadius = 2.15f;

    [Header("Climbing")]
    [SerializeField] private float grabPauseDuration = 0.12f;
    [SerializeField] private float climbStepDuration = 0.55f;
    [SerializeField, Range(0f, 1f)] private float reachEnd = 0.35f;
    [SerializeField, Range(0f, 1f)] private float pullEnd = 0.85f;

    [Header("Hand Targets")]
    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private Transform rightHandTarget;
    [SerializeField] private float handReachArc = 0.15f;

    [Header("Handholds")]
    [SerializeField] private TowerHandholdGenerator handholdGenerator;
    [SerializeField] private float maxVerticalReach = 0.85f;
    [SerializeField] private float playerSurfaceOffset = 0.35f;

    [Header("Body Animation")]
    [SerializeField] private Transform bodyVisual;
    [SerializeField] private Rig climbingRig;
    [SerializeField] private float reachLeanAngle = 8f;
    [SerializeField] private float reachSideLeanAngle = 5f;
    [SerializeField] private float reachDip = 0.04f;
    [SerializeField] private float pullRise = 0.08f;

    [Header("Hand Rotation")]
    [SerializeField] private Vector3 leftHandRotationOffset = new Vector3(-90f, 0f, 0f);
    [SerializeField] private Vector3 rightHandRotationOffset = new Vector3(90f, 0f, 0f);

    [Header("Hit / Fall")]
    [SerializeField] private float hitDuration = 0.2f;
    [SerializeField] private float fallDuration = 0.65f;
    [SerializeField] private float recoveryDuration = 0.35f;

    [SerializeField] private float fallDistance = 2.25f;
    [SerializeField] private float hitPushDistance = 0.25f;
    [SerializeField] private float hitTiltAngle = 15f;

    [Header("Win Condition")]
    [SerializeField] private float summitSurvivalDuration = 5f;
    [SerializeField] private float summitHeightTolerance = 0.05f;

    public event Action OnWin;

    private float summitSurvivalTimer;

    private Vector3 bodyVisualBaseLocalPosition;
    private Quaternion bodyVisualBaseLocalRotation;

    private float activeForceDistance;
    private float activeForceDuration;

    private Handhold currentHandhold;
    private Handhold targetHandhold;
    private float targetAngle;
    private bool isMovingToHandhold;

    private float stateTimer;

    private float fallStartHeight;
    private float fallTargetHeight;

    private Vector3 handReachStartPosition;
    private Quaternion handReachStartRotation;

    private ClimbingInput input;
    private ClimbingHand activeHand = ClimbingHand.Left;

    private Handhold leftHandhold;
    private Handhold rightHandhold;
    
    private Handhold recoveryHandhold;
    private float recoveryStartHeight;
    private float recoveryStartAngle;

    private float grabPauseTimer;
    private float targetHeight;
    private float height;
    private float angle;

    private float movementProgress;
    private float startHeight;
    private float startAngle;

    private Transform ActiveHandTarget =>
        activeHand == ClimbingHand.Left
            ? leftHandTarget
            : rightHandTarget;

    public float Height => height;

    public ClimbingState CurrentState { get; private set; } = ClimbingState.Climbing;

    public float SummitProgress => Mathf.Clamp01(summitSurvivalTimer / summitSurvivalDuration);

    public float SummitTimeRemaining =>
        Mathf.Max(
            0f,
            summitSurvivalDuration -
            summitSurvivalTimer
        );

    #region UNITY FUNCITONS

    private void Awake()
    {
        input = GetComponent<ClimbingInput>();

        Vector3 offset = transform.position - towerCenter.position;

        height = transform.position.y - towerCenter.position.y;
        angle = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
        targetHeight = height;
    }

    private void Start()
    {
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

        InitializeBodyVisual();
        InitializeHandTargets();
    }

    private void Update()
    {
        UpdateGrabPause();

        switch (CurrentState)
        {
            case ClimbingState.Climbing:
                UpdateClimbingState();
                break;

            case ClimbingState.Summit:
                UpdateSummitState();
                break;

            case ClimbingState.Won:
                UpdateWonState();
                break;

            case ClimbingState.Hit:
                UpdateHitState();
                break;

            case ClimbingState.Falling:
                UpdateFallingState();
                break;

            case ClimbingState.Recovering:
                UpdateRecoveringState();
                break;
        }

        UpdatePosition();
        UpdateRotation();
        UpdateGrabbedHands();
    }

    #endregion

    #region INITIALIZE FUNCTIONS

    public void Configure(
        float newClimbDuration,
        float newSummitSurvivalDuration
    )
    {
        climbStepDuration = newClimbDuration;
        summitSurvivalDuration =
            newSummitSurvivalDuration;
    }

    private void InitializeHandTargets()
    {
        if (currentHandhold == null)
            return;

        leftHandhold = currentHandhold;
        rightHandhold = currentHandhold;

        leftHandTarget.SetPositionAndRotation(
            currentHandhold.GrabPosition,
            GetHandGrabRotation(
                currentHandhold,
                ClimbingHand.Left
            )
        );

        rightHandTarget.SetPositionAndRotation(
            currentHandhold.GrabPosition,
            GetHandGrabRotation(
                currentHandhold,
                ClimbingHand.Right
            )
        );
    }

    private void InitializeBodyVisual()
    {
        bodyVisualBaseLocalPosition = bodyVisual.localPosition;
        bodyVisualBaseLocalRotation = bodyVisual.localRotation;
    }

    #endregion

    #region UPDATE FUNCTIONS

    private void UpdateClimbingState()
    {
        HandleClimbing();
        MoveTowardsHandhold();
        climbingRig.weight = 1f;

        if (!isMovingToHandhold)
            ResetBodyAnimation();
    }

    private void UpdateSummitState()
    {
        ResetBodyAnimation();

        summitSurvivalTimer += Time.deltaTime;

        if (summitSurvivalTimer >= summitSurvivalDuration)
        {
            Win();
        }
    }

    private void UpdateWonState()
    {
        ResetBodyAnimation();
    }

    private void UpdateHitState()
    {
        stateTimer += Time.deltaTime;

        float t =
            Mathf.Clamp01(
                stateTimer / hitDuration
            );

        UpdateHitVisual(t);

        climbingRig.weight = 1f - t;

        if (t >= 1f)
        {
            BeginFall();
        }
    }

    private void UpdateFallingState()
    {
        climbingRig.weight = 0f;

        stateTimer += Time.deltaTime;

        float t =
            Mathf.Clamp01(
                stateTimer / activeForceDuration
            );

        float easedT =
            t * t;

        height =
            Mathf.Lerp(
                fallStartHeight,
                fallTargetHeight,
                easedT
            );

        if (bodyVisual != null)
        {
            float wobble =
                Mathf.Sin(t * Mathf.PI * 2f);

            bodyVisual.localRotation =
                bodyVisualBaseLocalRotation *
                Quaternion.Euler(
                    10f,
                    0f,
                    wobble * 8f
                );
        }

        if (t >= 1f)
        {
            BeginRecovery();
        }
    }

    private void UpdateRecoveringState()
    {
        stateTimer += Time.deltaTime;

        float t =
            Mathf.Clamp01(
                stateTimer / recoveryDuration
            );

        float easedT =
            Mathf.SmoothStep(
                0f,
                1f,
                t
            );

        height =
            Mathf.Lerp(
                recoveryStartHeight,
                recoveryHandhold.Height,
                easedT
            );

        float angleDifference =
            Mathf.DeltaAngle(
                recoveryStartAngle,
                recoveryHandhold.Angle
            );

        angle =
            recoveryStartAngle +
            angleDifference * easedT;

        ResetBodyAnimation();

        if (t >= 1f)
        {
            FinishRecovery();
        }
    }

    private void BeginFall()
    {
        CurrentState =
            ClimbingState.Falling;

        stateTimer = 0f;

        fallStartHeight = height;

        fallTargetHeight =
            Mathf.Clamp(
                height + activeForceDistance,
                handholdGenerator.StartHeight,
                handholdGenerator.EndHeight
            );

        // During the fall, neither hand owns a hold.
        leftHandhold = null;
        rightHandhold = null;

        Debug.Log(
            $"FALL: {fallStartHeight:F2} -> " +
            $"{fallTargetHeight:F2}"
        );
    }

    private void BeginRecovery()
    {
        CurrentState =
            ClimbingState.Recovering;

        stateTimer = 0f;

        recoveryStartHeight = height;
        recoveryStartAngle = angle;

        recoveryHandhold =
            handholdGenerator.FindStartingHandhold(
                height,
                angle
            );

        if (recoveryHandhold == null)
        {
            climbingRig.weight = 1f;
            ResetBodyAnimation();

            CurrentState =
                ClimbingState.Climbing;

            return;
        }
    }

    private void FinishRecovery()
    {
        height =
            recoveryHandhold.Height;

        angle =
            recoveryHandhold.Angle;

        currentHandhold =
            recoveryHandhold;

        leftHandhold =
            recoveryHandhold;

        rightHandhold =
            recoveryHandhold;

        leftHandTarget.SetPositionAndRotation(
            recoveryHandhold.GrabPosition,
            GetHandGrabRotation(
                recoveryHandhold,
                ClimbingHand.Left
            )
        );

        rightHandTarget.SetPositionAndRotation(
            recoveryHandhold.GrabPosition,
            GetHandGrabRotation(
                recoveryHandhold,
                ClimbingHand.Right
            )
        );

        recoveryHandhold = null;

        grabPauseTimer =
            grabPauseDuration;

        CurrentState =
            ClimbingState.Climbing;

        climbingRig.weight = 1f;

        CheckForSummit();
    }

    public bool ReceiveWebhookForce(
    float heightDelta,
    float duration)
    {
        if (CurrentState == ClimbingState.Won ||
            CurrentState == ClimbingState.Hit ||
            CurrentState == ClimbingState.Falling ||
            CurrentState == ClimbingState.Recovering)
        {
            return false;
        }

        activeForceDistance = heightDelta;
        activeForceDuration = duration;

        summitSurvivalTimer = 0f;

        isMovingToHandhold = false;
        targetHandhold = null;

        CurrentState = ClimbingState.Hit;
        stateTimer = 0f;

        return true;
    }

    private void UpdateHitVisual(float t)
    {
        if (bodyVisual == null)
            return;

        float punch =
            Mathf.Sin(t * Mathf.PI);

        bodyVisual.localPosition =
            bodyVisualBaseLocalPosition +
            Vector3.back *
            (punch * hitPushDistance);

        bodyVisual.localRotation =
            bodyVisualBaseLocalRotation *
            Quaternion.Euler(
                hitTiltAngle * punch,
                0f,
                hitTiltAngle * 0.4f * punch
            );
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
        float horizontalIntent =
            input.Horizontal;

        Handhold nextHandhold =
            handholdGenerator.FindNextHandhold(
                height,
                angle,
                horizontalIntent,
                maxVerticalReach
            );

        if (nextHandhold == null)
            return;

        targetHandhold = nextHandhold;

        // Consume after successfully finding a hold.
        if (Mathf.Abs(horizontalIntent) > 0.1f)
        {
            input.ConsumeHorizontalIntent();
        }

        startHeight = height;
        startAngle = angle;

        targetHeight =
            targetHandhold.Height;

        targetAngle =
            targetHandhold.Angle;

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
        UpdateBodyAnimation(t);

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

            CheckForSummit();
        }
    }

    private void CheckForSummit()
    {
        if (currentHandhold == null)
            return;

        float summitHeight =
            handholdGenerator.EndHeight;

        if (currentHandhold.Height >=
            summitHeight - summitHeightTolerance)
        {
            EnterSummitState();
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

        float rotationProgress =
            Mathf.InverseLerp(
                0.25f,
                1f,
                handProgress
            );

        rotationProgress =
            Mathf.SmoothStep(
                0f,
                1f,
                rotationProgress
            );

        Quaternion targetRotation =
            GetHandGrabRotation(
                targetHandhold,
                activeHand
            );

        handTarget.rotation =
            Quaternion.Slerp(
                handReachStartRotation,
                targetRotation,
                rotationProgress
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
                GetHandGrabRotation(
                    leftHandhold,
                    ClimbingHand.Left
                );
        }

        if (rightHandhold != null &&
            activeHand != ClimbingHand.Right)
        {
            rightHandTarget.position =
                rightHandhold.GrabPosition;

            rightHandTarget.rotation =
                GetHandGrabRotation(
                    rightHandhold,
                    ClimbingHand.Right
                );
        }
    }

    private void UpdateBodyAnimation(float stepProgress)
    {
        if (bodyVisual == null)
            return;

        Vector3 positionOffset = Vector3.zero;
        Quaternion rotationOffset = Quaternion.identity;

        // -------------------------
        // REACH
        // -------------------------

        if (stepProgress <= reachEnd)
        {
            float reachT =
                Mathf.InverseLerp(
                    0f,
                    reachEnd,
                    stepProgress
                );

            float easedReach =
                Mathf.SmoothStep(0f, 1f, reachT);

            // Lean forward toward the tower.
            float forwardLean =
                reachLeanAngle * easedReach;

            // Lean slightly toward the reaching hand.
            float sideDirection =
                activeHand == ClimbingHand.Left
                    ? 1f
                    : -1f;

            float sideLean =
                reachSideLeanAngle *
                sideDirection *
                easedReach;

            rotationOffset =
                Quaternion.Euler(
                    forwardLean,
                    0f,
                    sideLean
                );

            // Tiny anticipation dip.
            positionOffset.y =
                -reachDip *
                Mathf.Sin(reachT * Mathf.PI);
        }

        // -------------------------
        // PULL
        // -------------------------

        else if (stepProgress <= pullEnd)
        {
            float pullT =
                Mathf.InverseLerp(
                    reachEnd,
                    pullEnd,
                    stepProgress
                );

            float easedPull =
                Mathf.SmoothStep(0f, 1f, pullT);

            // Start leaned, then straighten during pull.
            float forwardLean =
                Mathf.Lerp(
                    reachLeanAngle,
                    0f,
                    easedPull
                );

            float sideDirection =
                activeHand == ClimbingHand.Left
                    ? 1f
                    : -1f;

            float sideLean =
                Mathf.Lerp(
                    reachSideLeanAngle * sideDirection,
                    0f,
                    easedPull
                );

            rotationOffset =
                Quaternion.Euler(
                    forwardLean,
                    0f,
                    sideLean
                );

            // Slight visual lift during the pull.
            positionOffset.y =
                Mathf.Sin(pullT * Mathf.PI) *
                pullRise;
        }

        // -------------------------
        // SETTLE
        // -------------------------

        else
        {
            float settleT =
                Mathf.InverseLerp(
                    pullEnd,
                    1f,
                    stepProgress
                );

            float easedSettle =
                Mathf.SmoothStep(0f, 1f, settleT);

            positionOffset.y =
                Mathf.Lerp(
                    pullRise * 0.15f,
                    0f,
                    easedSettle
                );
        }

        bodyVisual.localPosition =
            bodyVisualBaseLocalPosition +
            positionOffset;

        bodyVisual.localRotation =
            bodyVisualBaseLocalRotation *
            rotationOffset;
    }

    private void EnterSummitState()
    {
        CurrentState =
            ClimbingState.Summit;

        summitSurvivalTimer = 0f;

        input.ConsumeHorizontalIntent();

        Debug.Log(
            "SUMMIT REACHED! Hold for 5 seconds to win."
        );
    }

    private void ResetBodyAnimation()
    {
        if (bodyVisual == null)
            return;

        bodyVisual.localPosition =
            Vector3.Lerp(
                bodyVisual.localPosition,
                bodyVisualBaseLocalPosition,
                12f * Time.deltaTime
            );

        bodyVisual.localRotation =
            Quaternion.Slerp(
                bodyVisual.localRotation,
                bodyVisualBaseLocalRotation,
                12f * Time.deltaTime
            );
    }

    public void ReceiveBump()
    {
        ReceiveWebhookForce(
            -fallDistance,
            fallDuration
        );
    }

    private void Win()
    {
        if (CurrentState ==
            ClimbingState.Won)
        {
            return;
        }

        CurrentState = ClimbingState.Won;
        OnWin?.Invoke();

        summitSurvivalTimer =
            summitSurvivalDuration;

        input.ConsumeHorizontalIntent();
    }

    #endregion

    #region HELPER FUNCTIONS

    private float GetMinimumClimbHeight()
    {
        return handholdGenerator.StartHeight;
    }

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

    private Quaternion GetHandGrabRotation(
        Handhold handhold,
        ClimbingHand hand)
    {
        Vector3 rotationOffset =
            hand == ClimbingHand.Left
                ? leftHandRotationOffset
                : rightHandRotationOffset;

        return
            handhold.GrabRotation *
            Quaternion.Euler(rotationOffset);
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

public enum ClimbingState
{
    Climbing,
    Summit,
    Won,
    Hit,
    Falling,
    Recovering
}

#endregion