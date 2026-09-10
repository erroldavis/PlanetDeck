using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public enum MaterialDieType
{
    Unassigned,
    Worldstone,
    Tidal,
    Coreheart,
    SkyForge,
    Lifeweave,
    Worldfire,
    Stormcall
}

public enum MaterialCombatRole
{
    Unassigned,
    Attack,
    Defend,
    Control,
    Interrupt
}

[RequireComponent(typeof(Collider))]
public class DieBase : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("Usage")]
    [SerializeField] private bool isSpent;

    public bool IsSpent => isSpent;

    [Header("Material Identity")]
    [SerializeField]
    private MaterialDieDefinition definition;

    public MaterialDieDefinition Definition =>
        definition;

    public MaterialDieType MaterialType =>
        definition != null
            ? definition.MaterialType
            : MaterialDieType.Unassigned;

    public MaterialCombatRole CombatRole =>
        definition != null
            ? definition.CombatRole
            : MaterialCombatRole.Unassigned;

    public bool HasMaterialIdentity =>
        definition != null &&
        definition.IsValid;

    [Header("Slot")]
    [SerializeField] private Transform homeSlot;
    [SerializeField] private Transform dragPlane;
    [SerializeField] private Camera interactionCamera;

    [Header("Roll Result")]
    [SerializeField] private int resultValue;
    [SerializeField] private bool isRevealed;

    [SerializeField] private bool interactionEnabled = true;

    public bool InteractionEnabled => interactionEnabled;

    public int SideCount =>
    definition != null
        ? definition.SideCount
        : 6;
    public int ResultValue => resultValue;
    public bool IsRevealed => isRevealed;

    [Header("States")]
    public bool isHovering;
    public bool isDragging;
    public bool selected;
    public bool wasDragged;
    public Transform HomeSlot => homeSlot;

    [Header("Events")]
    public UnityEvent<DieBase> PointerEnterEvent =
        new UnityEvent<DieBase>();

    public UnityEvent<DieBase> PointerExitEvent =
        new UnityEvent<DieBase>();

    public UnityEvent<DieBase> PointerDownEvent =
        new UnityEvent<DieBase>();

    public UnityEvent<DieBase> PointerUpEvent =
        new UnityEvent<DieBase>();

    public UnityEvent<DieBase> BeginDragEvent =
        new UnityEvent<DieBase>();

    public UnityEvent<DieBase> EndDragEvent =
        new UnityEvent<DieBase>();

    public UnityEvent<DieBase, bool> SelectEvent =
        new UnityEvent<DieBase, bool>();

    public UnityEvent<DieBase> LaunchRequestedEvent =
    new UnityEvent<DieBase>();

    public UnityEvent<DieBase> LaunchArrivedEvent =
        new UnityEvent<DieBase>();

    [HideInInspector]
    public UnityEvent<DieBase> HandEntryRequestedEvent =
        new UnityEvent<DieBase>();

    [HideInInspector]
    public UnityEvent<DieBase> HandEntryArrivedEvent =
    new UnityEvent<DieBase>();

    private void Awake()
    {
        if (homeSlot == null)
            homeSlot = transform.parent;
        if (interactionCamera == null)
            interactionCamera = Camera.main;
    }

    public bool Configure(
    MaterialDieDefinition newDefinition)
    {
        if (newDefinition == null ||
            !newDefinition.IsValid)
        {
            Debug.LogWarning(
                $"{name} received an invalid Material Die definition.",
                this
            );

            return false;
        }

        definition = newDefinition;
        isSpent = false;

        Deselect();
        ClearResult();
        SetInteractionEnabled(false);

        gameObject.name =
            $"Die_{definition.MaterialName}";

        Debug.Log(
            $"{name} configured as " +
            $"{definition.MaterialType}.",
            this
        );

        return true;
    }

    public void RequestLaunch()
    {
        if (isSpent)
        {
            Debug.LogWarning(
                $"{name} cannot launch because it is spent.",
                this
            );

            return;
        }

        LaunchRequestedEvent.Invoke(this);
    }

    public void NotifyLaunchArrived()
    {
        LaunchArrivedEvent.Invoke(this);
    }

    public void NotifyHandEntryArrived()
    {
        HandEntryArrivedEvent.Invoke(this);
    }

    public void RequestHandEntry()
    {
        HandEntryRequestedEvent.Invoke(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!interactionEnabled)
            return;

        isHovering = true;

        PointerEnterEvent.Invoke(this);
        Debug.Log($"{name}: Pointer Enter");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!interactionEnabled)
            return;

        isHovering = false;

        PointerExitEvent.Invoke(this);
        Debug.Log($"{name}: Pointer Exit");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!interactionEnabled)
            return;

        PointerDownEvent.Invoke(this);
        Debug.Log($"{name}: Pointer Down");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!interactionEnabled)
            return;

        PointerUpEvent.Invoke(this);
        Debug.Log($"{name}: Pointer Up");
    }

    public void SetSelected(bool value)
    {
        if (value && isSpent)
            return;

        if (selected == value)
            return;

        selected = value;

        SelectEvent.Invoke(this, selected);
        Debug.Log($"{name}: Selected = {selected}");
    }

    public void SetResult(int value)
    {
        resultValue = Mathf.Clamp(value, 1, SideCount);
        isRevealed = false;
    }

    public void RevealResult()
    {
        if (resultValue == 0)
            return;

        isRevealed = true;

        Debug.Log($"{name} revealed {resultValue}");
    }

    public void ClearResult()
    {
        resultValue = 0;
        isRevealed = false;
    }

    public void MarkSpent()
    {
        if (isSpent)
            return;

        isSpent = true;

        Deselect();
        ClearResult();
        SetInteractionEnabled(false);

        Debug.Log(
            $"{name} became spent.",
            this
        );
    }

    public void ResetSpent()
    {
        if (!isSpent)
            return;

        isSpent = false;

        ClearResult();
        SetInteractionEnabled(false);

        Debug.Log(
            $"{name} is available for the next hand.",
            this
        );
    }

    [ContextMenu("Test/Mark Spent")]
    private void TestMarkSpent()
    {
        MarkSpent();
    }

    [ContextMenu("Test/Reset Spent")]
    private void TestResetSpent()
    {
        ResetSpent();
    }

    [ContextMenu("Test/Set Result To 4")]
    private void TestSetResult()
    {
        SetResult(4);
    }

    [ContextMenu("Test/Reveal Result")]
    private void TestRevealResult()
    {
        RevealResult();
    }

    [ContextMenu("Test/Clear Result")]
    private void TestClearResult()
    {
        ClearResult();
    }

    [ContextMenu("Test/Show Material Identity")]
    private void TestShowMaterialIdentity()
    {
        if (!HasMaterialIdentity)
        {
            Debug.LogWarning(
                $"{name} has an incomplete Material identity.",
                this
            );

            return;
        }

        Debug.Log(
            $"{name}: Material = {MaterialType}, " +
            $"Role = {CombatRole}, " +
            $"Sides = {SideCount}, " +
            $"Result = {ResultValue}.",
            this
        );
    }

    public void Deselect()
    {
        SetSelected(false);
    }

    public void SetInteractionEnabled(bool value)
    {
        interactionEnabled = value && !isSpent;

        if (!interactionEnabled)
            isHovering = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!interactionEnabled)
            return;

        isDragging = true;
        wasDragged = true;

        BeginDragEvent.Invoke(this);
        Debug.Log($"{name}: Begin Drag");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!interactionEnabled)
            return;

        // Die movement
        if (interactionCamera == null || dragPlane == null)
        return;

        Ray pointerRay =
            interactionCamera.ScreenPointToRay(eventData.position);

        Plane movementPlane = new Plane(
            dragPlane.up,
            dragPlane.position
        );

        if (movementPlane.Raycast(pointerRay, out float distance))
        {
            transform.position = pointerRay.GetPoint(distance);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!interactionEnabled)
            return;

        SnapToHome();

        isDragging = false;

        EndDragEvent.Invoke(this);
        Debug.Log($"{name}: End Drag");

        StartCoroutine(ClearWasDraggedAtEndOfFrame());
    }

    private IEnumerator ClearWasDraggedAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        wasDragged = false;
    }

    public void SnapToHome()
    {
        if (homeSlot == null)
            return;

        transform.SetParent(homeSlot, false);
        transform.localPosition = Vector3.zero;
    }

    public void SetHomeSlot( Transform newHomeSlot, bool snapImmediately = true)
    {
        if (newHomeSlot == null)
            return;

        homeSlot = newHomeSlot;

        if (snapImmediately)
            SnapToHome();
        else
            transform.SetParent(homeSlot, true);
    }

}