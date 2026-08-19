using System.Collections.Generic;
using UnityEngine;

public class DieHandContainer : MonoBehaviour
{
    [Header("Hand Settings")]
    [SerializeField, Min(1)] private int handCapacity = 4;
    public int HandCapacity => handCapacity;
    public IReadOnlyList<DieBase> Dice => dice;
    public DieBase SelectedDie => selectedDie;

    [Header("Hand Contents")]
    [SerializeField]
    private List<Transform> slots =
        new List<Transform>();

    [SerializeField]
    private List<DieBase> dice =
        new List<DieBase>();

    [Header("Current State")]
    [SerializeField] private DieBase draggedDie;
    [SerializeField] private DieBase selectedDie;

    private void Awake()
    {
        GatherHandContents();
    }

    private void OnEnable()
    {
        SubscribeToDice();
    }

    private void OnDisable()
    {
        UnsubscribeFromDice();
        draggedDie = null;
    }

    public void ClearSelection()
    {
        if (selectedDie != null)
            selectedDie.Deselect();

        selectedDie = null;
    }

    public int GetDiceToDraw(int availableDeckDice)
    {
        int safeAvailableDice = Mathf.Max(0, availableDeckDice);
        int slotCapacity = slots.Count;

        return Mathf.Min(
            handCapacity,
            Mathf.Min(safeAvailableDice, slotCapacity)
        );
    }

    [ContextMenu("Test/Calculate Dice To Draw")]
    public void TestDiceToDraw()
    {
        int availableDice = dice.Count;
        int drawCount = GetDiceToDraw(availableDice);

        Debug.Log(
            $"Hand capacity: {handCapacity}, " +
            $"Available dice: {availableDice}, " +
            $"Slots: {slots.Count}, " +
            $"Dice to draw: {drawCount}"
        );
    }
    public void SetDiceInteractionEnabled(bool value)
    {
        foreach (DieBase die in dice)
        {
            if (die != null)
                die.SetInteractionEnabled(value);
        }
    }

    private void GatherHandContents()
    {
        slots.Clear();

        // Gather direct-child slots only.
        for (int i = 0; i < transform.childCount; i++)
        {
            slots.Add(transform.GetChild(i));
        }

        // Fixed left-to-right slot order.
        slots.Sort(
            (a, b) => a.position.x.CompareTo(b.position.x)
        );

        dice.Clear();

        for (int i = 0; i < slots.Count; i++)
        {
            DieBase die =
                slots[i].GetComponentInChildren<DieBase>();

            dice.Add(die);

            if (die == null)
            {
                Debug.LogWarning(
                    $"{name}: No DieBase found inside {slots[i].name}"
                );
            }
        }
    }

    private void SubscribeToDice()
    {
        foreach (DieBase die in dice)
        {
            if (die == null)
                continue;

            die.BeginDragEvent.AddListener(HandleBeginDrag);
            die.EndDragEvent.AddListener(HandleEndDrag);
            die.PointerUpEvent.AddListener(HandlePointerUp);

        }
    }

    private void UnsubscribeFromDice()
    {
        foreach (DieBase die in dice)
        {
            if (die == null)
                continue;

            die.BeginDragEvent.RemoveListener(HandleBeginDrag);
            die.EndDragEvent.RemoveListener(HandleEndDrag);
            die.PointerUpEvent.RemoveListener(HandlePointerUp);
        }
    }

    private void HandleBeginDrag(DieBase die)
    {
        draggedDie = die;

        Debug.Log(
            $"{name}: Tracking Begin Drag — {die.name}"
        );
    }

    private void HandleEndDrag(DieBase die)
    {
        if (draggedDie != die)
            return;

        Debug.Log(
            $"{name}: Tracking End Drag — {die.name}"
        );

        draggedDie = null;
    }

    private void HandlePointerUp(DieBase die)
    {
        if (die == null || !die.IsRevealed)
            return;

        if (die.wasDragged)
        {
            Debug.Log($"{die.name} was dragged. Selection ignored.");
            return;
        }

        if (selectedDie != null && selectedDie != die)
            selectedDie.Deselect();

        die.SetSelected(true);
        selectedDie = die;
    }

    private void Update()
    {
        if (draggedDie == null)
            return;

        // A die cannot cross more slots than the hand contains.
        int remainingChecks = dice.Count;

        while (
            remainingChecks > 0 &&
            TrySwapWithCrossedNeighbor()
        )
        {
            remainingChecks--;
        }
    }

    private bool TrySwapWithCrossedNeighbor()
    {
        int currentIndex = dice.IndexOf(draggedDie);

        if (currentIndex < 0)
            return false;

        Transform currentHome = draggedDie.HomeSlot;

        if (currentHome == null)
            return false;

        float draggedX = draggedDie.transform.position.x;
        float homeX = currentHome.position.x;

        bool movingRight = draggedX > homeX;
        bool movingLeft = draggedX < homeX;

        if (!movingRight && !movingLeft)
            return false;

        int neighborIndex = movingRight
            ? currentIndex + 1
            : currentIndex - 1;

        // The dragged die has reached an end of the hand.
        if (
            neighborIndex < 0 ||
            neighborIndex >= dice.Count
        )
        {
            return false;
        }

        DieBase neighborDie = dice[neighborIndex];

        if (
            neighborDie == null ||
            neighborDie.HomeSlot == null
        )
        {
            return false;
        }

        float neighborHomeX =
            neighborDie.HomeSlot.position.x;

        float midpointX =
            (homeX + neighborHomeX) * 0.5f;

        bool crossedMidpoint = movingRight
            ? draggedX > midpointX
            : draggedX < midpointX;

        if (!crossedMidpoint)
            return false;

        SwapDice(draggedDie, neighborDie);
        return true;
    }

    private void SwapDice(DieBase dragged, DieBase other)
    {
        int draggedIndex = dice.IndexOf(dragged);
        int otherIndex = dice.IndexOf(other);

        if (draggedIndex < 0 || otherIndex < 0)
            return;

        Transform draggedHome = dragged.HomeSlot;
        Transform otherHome = other.HomeSlot;

        if (draggedHome == null || otherHome == null)
            return;

        // Keep the dragged die under the pointer.
        dragged.SetHomeSlot(
            otherHome,
            snapImmediately: false
        );

        // Move the idle die into the vacated slot.
        other.SetHomeSlot(
            draggedHome,
            snapImmediately: true
        );

        // Preserve dice[i] matching slots[i].
        dice[draggedIndex] = other;
        dice[otherIndex] = dragged;

        Debug.Log(
            $"{name}: Swapped {dragged.name} and {other.name}"
        );
    }
}