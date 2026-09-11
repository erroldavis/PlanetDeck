using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class DieVisual3D : MonoBehaviour
{
    [Header("Hand Entry")]
    [SerializeField] private Transform handLaunchPoint;

    [Header("Material Appearance")]
    [SerializeField] private Renderer dieRenderer;

    [Header("Planet Launch")]
    [SerializeField] private GateABattleController battle;

    private Transform activeLaunchTarget;

    private bool isEnteringHand;
    private bool isLaunching;
    private bool isAtPlanet;
    [SerializeField] private DieBase target;

    [Header("Follow")]
    [SerializeField] private float followSpeed = 20f;

    [Header("Movement Tilt")]
    [SerializeField] private Transform rotationPivot;
    [SerializeField] private float tiltAmount = 1f;
    [SerializeField] private float maxTiltAngle = 12f;
    [SerializeField] private float tiltSpeed = 10f;

    [Header("Interaction State")]
    [SerializeField] private bool isHovering;
    [SerializeField] private bool isPressed;
    [SerializeField] private bool isDragging;
    [SerializeField] private bool isSelected;

    [Header("Hover Tilt")]
    [SerializeField] private float hoverTiltAngle = 5f;
    [SerializeField] private float hoverDistancePixels = 150f;

    [Header("Idle Wobble")]
    [SerializeField] private float idleWobbleAngle = 2f;
    [SerializeField] private float idleWobbleSpeed = 1.2f;

    [Header("Feedback")]
    [SerializeField] private Transform feedbackPivot;
    [SerializeField] private Camera interactionCamera;

    [SerializeField] private float feedbackSpeed = 15f;

    [SerializeField] private float hoverLift = 0.08f;
    [SerializeField] private float selectedLift = 0.12f;

    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float dragScale = 1.12f;

    [SerializeField] private float dragCameraOffset = 0.08f;

    [Header("Hand Roll Motion")]
    [SerializeField] private Transform rollPivot;

    [SerializeField]
    private Vector2 travelDurationRange =
        new Vector2(0.65f, 0.9f);

    [SerializeField]
    private Vector2 arcHeightRange =
        new Vector2(0.4f, 1f);

    [SerializeField]
    private Vector2Int spinTurnsRange =
        new Vector2Int(2, 5);

    private Coroutine handEntryRoutine;
    private Coroutine planetLaunchRoutine;


    private Vector3 lastPosition;

    private void OnEnable()
    {
        lastPosition = transform.position;

        if (interactionCamera == null)
            interactionCamera = Camera.main;

        if (target == null)
            return;

        isHovering = target.isHovering;
        isDragging = target.isDragging;
        isSelected = target.selected;
        isPressed = false;

        SubscribeToTarget();

    }

    private void OnDisable()
    {
        UnsubscribeFromTarget();

        isHovering = false;
        isPressed = false;
        isDragging = false;
    }

    public void Bind(
    DieBase newTarget,
    Transform newHandLaunchPoint,
    GateABattleController newBattle)
    {
        if (newTarget == null)
        {
            Debug.LogWarning(
                $"{name} cannot bind without a logical die.",
                this
            );

            return;
        }

        UnsubscribeFromTarget();

        target = newTarget;
        handLaunchPoint = newHandLaunchPoint;
        battle = newBattle;

        if (dieRenderer != null && target.Definition != null)
        {
            dieRenderer.material.color =
                target.Definition.VisualColor;
        }

        transform.SetPositionAndRotation(
            target.transform.position,
            target.transform.rotation
        );

        isHovering = target.isHovering;
        isDragging = target.isDragging;
        isSelected = target.selected;
        isPressed = false;

        if (isActiveAndEnabled)
            SubscribeToTarget();
    }

    private void SubscribeToTarget()
    {
        if (target == null)
            return;

        target.PointerEnterEvent.AddListener(HandlePointerEnter);
        target.PointerExitEvent.AddListener(HandlePointerExit);
        target.PointerDownEvent.AddListener(HandlePointerDown);
        target.PointerUpEvent.AddListener(HandlePointerUp);
        target.BeginDragEvent.AddListener(HandleBeginDrag);
        target.EndDragEvent.AddListener(HandleEndDrag);
        target.SelectEvent.AddListener(HandleSelect);
        target.HandEntryRequestedEvent.AddListener(HandleHandEntryRequested);
        target.LaunchRequestedEvent.AddListener(HandleLaunchRequested);
    }

    private void UnsubscribeFromTarget()
    {
        if (target == null)
            return;

        target.PointerEnterEvent.RemoveListener(HandlePointerEnter);
        target.PointerExitEvent.RemoveListener(HandlePointerExit);
        target.PointerDownEvent.RemoveListener(HandlePointerDown);
        target.PointerUpEvent.RemoveListener(HandlePointerUp);
        target.BeginDragEvent.RemoveListener(HandleBeginDrag);
        target.EndDragEvent.RemoveListener(HandleEndDrag);
        target.SelectEvent.RemoveListener(HandleSelect);
        target.HandEntryRequestedEvent.RemoveListener(HandleHandEntryRequested);
        target.LaunchRequestedEvent.RemoveListener(HandleLaunchRequested);
    }

    private void HandleHandEntryRequested(DieBase die)
    {
        if (die != target ||
            handLaunchPoint == null ||
            rollPivot == null)
        {
            return;
        }

        // Release the visual from the planet.
        isAtPlanet = false;
        isLaunching = false;
        activeLaunchTarget = null;

        if (planetLaunchRoutine != null)
        {
            StopCoroutine(planetLaunchRoutine);
            planetLaunchRoutine = null;
        }

        rollPivot.localRotation = Quaternion.identity;

        if (handEntryRoutine != null)
        {
            StopCoroutine(handEntryRoutine);
            handEntryRoutine = null;
        }

        handEntryRoutine = StartCoroutine(
            PlayHandRoll()
        );
    }

    private void HandleLaunchRequested(DieBase die)
    {
        if (die != target || rollPivot == null)
            return;

        if (battle == null)
        {
            Debug.LogWarning(
                $"{name} cannot launch because Battle is unassigned."
            );

            return;
        }

        if (!battle.TryGetActiveLaunchTarget(
                out activeLaunchTarget))
        {
            Debug.LogWarning(
                $"{name} cannot launch because there is no active target."
            );

            return;
        }

        if (planetLaunchRoutine != null)
            StopCoroutine(planetLaunchRoutine);

        isAtPlanet = false;

        planetLaunchRoutine = StartCoroutine(
            PlayPlanetLaunch()
        );
    }

    private void HandlePointerEnter(DieBase die)
    {
        isHovering = true;
    }

    private void HandlePointerExit(DieBase die)
    {
        isHovering = false;
    }

    private void HandlePointerDown(DieBase die)
    {
        isPressed = true;
    }

    private void HandlePointerUp(DieBase die)
    {
        isPressed = false;
    }

    private void HandleBeginDrag(DieBase die)
    {
        isDragging = true;
    }

    private void HandleEndDrag(DieBase die)
    {
        isDragging = false;
    }

    private void HandleSelect(DieBase die, bool state)
    {
        isSelected = state;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        if (isAtPlanet && activeLaunchTarget != null)
        {
            transform.position = activeLaunchTarget.position;
            return;
        }

        if (!isEnteringHand && !isLaunching && !isAtPlanet)
            SmoothFollow();

        if (!isLaunching && !isAtPlanet)
            SmoothRootRotation();

        ApplyPivotRotation();
        ApplyFeedbackPose();
    }

    private void SmoothFollow()
    {
        transform.position = Vector3.Lerp(
            transform.position,
            target.transform.position,
            Time.deltaTime * followSpeed
        );
    }

    private void SmoothRootRotation()
    {
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            target.transform.rotation,
            Time.deltaTime * followSpeed
        );
    }

    private void ApplyPivotRotation()
    {
        if (rotationPivot == null)
            return;

        Quaternion movementTilt = CalculateMovementTilt();
        Quaternion hoverTilt = CalculateHoverTilt();
        Quaternion idleTilt = CalculateIdleTilt();

        Quaternion targetPivotRotation =
            movementTilt *
            hoverTilt *
            idleTilt;

        rotationPivot.localRotation = Quaternion.Slerp(
            rotationPivot.localRotation,
            targetPivotRotation,
            Time.deltaTime * tiltSpeed
        );
    }

    private Quaternion CalculateMovementTilt()
    {
        float deltaTime = Time.deltaTime;

        if (deltaTime <= 0f)
            return Quaternion.identity;

        Vector3 velocity =
            (transform.position - lastPosition) / deltaTime;

        lastPosition = transform.position;

        // Ignore vertical feedback movement.
        velocity.y = 0f;

        if (velocity.sqrMagnitude < 0.0001f)
            return Quaternion.identity;

        float pitch = Mathf.Clamp(
            -velocity.z * tiltAmount,
            -maxTiltAngle,
            maxTiltAngle
        );

        float roll = Mathf.Clamp(
            velocity.x * tiltAmount,
            -maxTiltAngle,
            maxTiltAngle
        );

        return Quaternion.Euler(pitch, 0f, roll);
    }

    private Quaternion CalculateHoverTilt()
    {
        if (!isHovering || isDragging)
            return Quaternion.identity;

        if (interactionCamera == null || Mouse.current == null)
            return Quaternion.identity;

        Vector3 dieScreenPosition =
            interactionCamera.WorldToScreenPoint(transform.position);

        // The die is behind the camera.
        if (dieScreenPosition.z <= 0f)
            return Quaternion.identity;

        Vector2 pointerPosition =
            Mouse.current.position.ReadValue();

        Vector2 screenDelta =
            pointerPosition -
            new Vector2(dieScreenPosition.x, dieScreenPosition.y);

        screenDelta /= Mathf.Max(
            hoverDistancePixels,
            1f
        );

        screenDelta = Vector2.ClampMagnitude(
            screenDelta,
            1f
        );

        float pitch =
            -screenDelta.y * hoverTiltAngle;

        float roll =
            screenDelta.x * hoverTiltAngle;

        return Quaternion.Euler(
            pitch,
            0f,
            roll
        );
    }

    private Quaternion CalculateIdleTilt()
    {
        if (isDragging)
            return Quaternion.identity;

        float phase =
            Time.time * idleWobbleSpeed +
            transform.GetSiblingIndex();

        float pitch =
            Mathf.Sin(phase) * idleWobbleAngle;

        float yaw =
            Mathf.Cos(phase * 0.8f) *
            idleWobbleAngle * 0.5f;

        float roll =
            Mathf.Cos(phase) *
            idleWobbleAngle * 0.75f;

        return Quaternion.Euler(
            pitch,
            yaw,
            roll
        );
    }

    private void ApplyFeedbackPose()
    {
        if (feedbackPivot == null)
            return;

        Vector3 targetLocalPosition = Vector3.zero;
        float targetScale = 1f;

        if (isHovering)
        {
            targetLocalPosition += Vector3.up * hoverLift;
            targetScale = Mathf.Max(targetScale, hoverScale);
        }

        if (isSelected)
        {
            targetLocalPosition += Vector3.up * selectedLift;
        }

        if (isDragging)
        {
            targetScale = Mathf.Max(targetScale, dragScale);

            if (interactionCamera != null)
            {
                Vector3 towardCameraWorld =
                    interactionCamera.transform.position -
                    transform.position;

                if (towardCameraWorld.sqrMagnitude > 0f)
                {
                    towardCameraWorld.Normalize();

                    Vector3 towardCameraLocal =
                        transform.InverseTransformDirection(
                            towardCameraWorld
                        );

                    targetLocalPosition +=
                        towardCameraLocal * dragCameraOffset;
                }
            }
        }

        Vector3 targetLocalScale =
            Vector3.one * targetScale;

        feedbackPivot.localPosition = Vector3.Lerp(
            feedbackPivot.localPosition,
            targetLocalPosition,
            Time.deltaTime * feedbackSpeed
        );

        feedbackPivot.localScale = Vector3.Lerp(
            feedbackPivot.localScale,
            targetLocalScale,
            Time.deltaTime * feedbackSpeed
        );
    }


    private IEnumerator PlayHandRoll()
    {
        isEnteringHand = true;

        Vector3 startPosition = handLaunchPoint.position;

        float minimumDuration = Mathf.Min(
            travelDurationRange.x,
            travelDurationRange.y
        );

        float maximumDuration = Mathf.Max(
            travelDurationRange.x,
            travelDurationRange.y
        );

        float duration = Random.Range(
            minimumDuration,
            maximumDuration
        );

        float minimumArc = Mathf.Min(
            arcHeightRange.x,
            arcHeightRange.y
        );

        float maximumArc = Mathf.Max(
            arcHeightRange.x,
            arcHeightRange.y
        );

        float arcHeight = Random.Range(
            minimumArc,
            maximumArc
        );

        int minimumTurns = Mathf.Min(
            spinTurnsRange.x,
            spinTurnsRange.y
        );

        int maximumTurns = Mathf.Max(
            spinTurnsRange.x,
            spinTurnsRange.y
        );

        int spinTurns = Random.Range(
            minimumTurns,
            maximumTurns + 1
        );

        Vector3 spinAxis = Random.onUnitSphere.normalized;

        transform.position = startPosition;
        rollPivot.localRotation = Quaternion.identity;
        lastPosition = transform.position;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float normalizedTime = Mathf.Clamp01(
                elapsed / duration
            );

            float movementProgress = Mathf.SmoothStep(
                0f,
                1f,
                normalizedTime
            );

            Vector3 targetPosition =
                target.transform.position;

            Vector3 position = Vector3.Lerp(
                startPosition,
                targetPosition,
                movementProgress
            );

            float arcOffset =
                Mathf.Sin(normalizedTime * Mathf.PI) *
                arcHeight;

            position += Vector3.up * arcOffset;

            transform.position = position;

            // Fast initial tumble that slows as the die approaches.
            float spinProgress =
                1f - Mathf.Pow(1f - normalizedTime, 2f);

            float spinAngle =
                360f * spinTurns * spinProgress;

            rollPivot.localRotation =
                Quaternion.AngleAxis(
                    spinAngle,
                    spinAxis
                );

            yield return null;
        }

        transform.position = target.transform.position;

        // Integer spin turns guarantee this returns cleanly to rest.
        rollPivot.localRotation = Quaternion.identity;

        lastPosition = transform.position;
        isEnteringHand = false;
        handEntryRoutine = null;

        target.NotifyHandEntryArrived();
    }

    private IEnumerator PlayPlanetLaunch()
    {
        isLaunching = true;
        isAtPlanet = false;

        Vector3 startPosition = transform.position;
        if (activeLaunchTarget == null)
        {
            isLaunching = false;
            planetLaunchRoutine = null;
            yield break;
        }

        Vector3 endPosition =
            activeLaunchTarget.position;

        float minimumDuration = Mathf.Min(
            travelDurationRange.x,
            travelDurationRange.y
        );

        float maximumDuration = Mathf.Max(
            travelDurationRange.x,
            travelDurationRange.y
        );

        float duration = Random.Range(
            minimumDuration,
            maximumDuration
        );

        float minimumArc = Mathf.Min(
            arcHeightRange.x,
            arcHeightRange.y
        );

        float maximumArc = Mathf.Max(
            arcHeightRange.x,
            arcHeightRange.y
        );

        float arcHeight = Random.Range(
            minimumArc,
            maximumArc
        );

        int minimumTurns = Mathf.Min(
            spinTurnsRange.x,
            spinTurnsRange.y
        );

        int maximumTurns = Mathf.Max(
            spinTurnsRange.x,
            spinTurnsRange.y
        );

        int spinTurns = Random.Range(
            minimumTurns,
            maximumTurns + 1
        );

        Vector3 spinAxis = Random.onUnitSphere.normalized;

        rollPivot.localRotation = Quaternion.identity;
        lastPosition = transform.position;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float normalizedTime = Mathf.Clamp01(
                elapsed / duration
            );

            float movementProgress = Mathf.SmoothStep(
                0f,
                1f,
                normalizedTime
            );

            Vector3 position = Vector3.Lerp(
                startPosition,
                endPosition,
                movementProgress
            );

            float arcOffset =
                Mathf.Sin(normalizedTime * Mathf.PI) *
                arcHeight;

            position += Vector3.up * arcOffset;

            transform.position = position;

            // Fast initial tumble that slows as the die approaches.
            float spinProgress =
                1f - Mathf.Pow(1f - normalizedTime, 2f);

            float spinAngle =
                360f * spinTurns * spinProgress;

            rollPivot.localRotation =
                Quaternion.AngleAxis(
                    spinAngle,
                    spinAxis
                );

            yield return null;
        }

        if (activeLaunchTarget != null)
            transform.position = activeLaunchTarget.position;
        rollPivot.localRotation = Quaternion.identity;

        lastPosition = transform.position;
        isLaunching = false;
        isAtPlanet = true;
        planetLaunchRoutine = null;

        target.NotifyLaunchArrived();
    }
}