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
    [Header("Material Identity")]
    [SerializeField] private MaterialDieType materialType;
    [SerializeField] private MaterialCombatRole combatRole;

    public MaterialDieType MaterialType =>
        materialType;

    public MaterialCombatRole CombatRole =>
        combatRole;

    public bool HasMaterialIdentity =>
        materialType != MaterialDieType.Unassigned &&
        combatRole != MaterialCombatRole.Unassigned;

    [Header("Slot")]
    [SerializeField] private Transform homeSlot;
    [SerializeField] private Transform dragPlane;
    [SerializeField] private Camera interactionCamera;

    [Header("Roll Result")]
    [SerializeField, Min(2)] private int sideCount = 6;
    [SerializeField] private int resultValue;
    [SerializeField] private bool isRevealed;

    [SerializeField] private bool interactionEnabled = true;

    public bool InteractionEnabled => interactionEnabled;

    public int SideCount => Mathf.Max(2, sideCount);
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

    public void RequestLaunch()
    {
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
            $"{name}: Material = {materialType}, " +
            $"Role = {combatRole}, " +
            $"Result = {resultValue}.",
            this
        );
    }

    public void Deselect()
    {
        SetSelected(false);
    }

    public void SetInteractionEnabled(bool value)
    {
        interactionEnabled = value;

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