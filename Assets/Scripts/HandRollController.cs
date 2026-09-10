using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class HandRollController : MonoBehaviour
{
    [Header("Startup")]
    [SerializeField] private bool rollOnStart = true;
    [SerializeField] private PlanetReactionController planet;
    [SerializeField] private DiceRollService rollService;
    [SerializeField] private DieHandContainer hand;
    [SerializeField] private HandRollProfile rollProfile;
    [SerializeField] private GateABattleController battle;

    private Coroutine entrySequence;

    private readonly List<DieBase> rollingDice = new();
    private int arrivedDiceCount;

    public bool IsRolling { get; private set; }

    private IEnumerator Start()
    {
        // Allow the hand and die visuals to finish initializing.
        yield return null;

        if (rollOnStart)
            RollHand();
    }

    [ContextMenu("Test/Roll Hand")]
    public void RollHand()
    {
        if (IsRolling)
            return;

        hand.ClearSelection();

        arrivedDiceCount = 0;

        if (rollService == null || hand == null)
        {
            Debug.LogWarning(
                "HandRollController is missing a reference."
            );

            return;
        }

        List<DieBase> availableDice = new List<DieBase>();

        foreach (DieBase die in hand.Dice)
        {
            if (die != null && !die.IsSpent)
                availableDice.Add(die);
        }

        if (availableDice.Count == 0)
        {
            Debug.Log(
                "No unused Material Dice remain."
            );

            return;
        }

        foreach (DieBase die in availableDice)
        {
            if (die != null && die.isDragging)
            {
                Debug.LogWarning(
                    "Cannot roll the hand while a die is being dragged."
                );

                return;
            }
        }

        int diceToDraw =
            hand.GetDiceToDraw(availableDice.Count);

        IsRolling = true;
        rollingDice.Clear();

        hand.SetDiceInteractionEnabled(false);

        foreach (DieBase die in availableDice)
        {
            if (die == null)
                continue;

            die.Deselect();
            die.ClearResult();
        }

        for (int i = 0; i < diceToDraw; i++)
        {
            DieBase die = availableDice[i];

            if (die == null)
                continue;

            die.HandEntryArrivedEvent.RemoveListener(HandleDieArrived);

            die.HandEntryArrivedEvent.AddListener(HandleDieArrived);

            int result = rollService.Roll(die.SideCount);

            die.SetResult(result);
            rollingDice.Add(die);

            Debug.Log(
                $"{die.name} stored hidden result {result}"
            );
        }
        entrySequence = StartCoroutine(PlayHandEntrySequence());
        Debug.Log(
            $"Hand roll prepared {rollingDice.Count} dice."
        );
    }

    private void HandleDieArrived(DieBase die)
    {
        if (!IsRolling || !rollingDice.Contains(die))
            return;

        die.HandEntryArrivedEvent.RemoveListener(
            HandleDieArrived
        );

        die.RevealResult();
        arrivedDiceCount++;

        Debug.Log(
            $"{die.name} landed and revealed " +
            $"{die.ResultValue}. " +
            $"{arrivedDiceCount}/{rollingDice.Count}"
        );

        if (arrivedDiceCount >= rollingDice.Count)
            CompleteHandRoll();
    }

    private void CompleteHandRoll()
    {
        foreach (DieBase die in hand.Dice)
        {
            if (die == null)
                continue;

            // Undrawn dice remain locked because they have no revealed result.
            die.SetInteractionEnabled(die.IsRevealed);
        }

        IsRolling = false;
        rollingDice.Clear();

        Debug.Log("All drawn dice landed. Hand is ready.");
    }

    public bool TryGetReadyDie(out DieBase die)
    {
        die = hand != null ? hand.SelectedDie : null;

        return !IsRolling
            && die != null
            && !die.IsSpent
            && die.IsRevealed
            && die.selected
            && die.ResultValue > 0;
    }

    [ContextMenu("Test/Check Ready Die")]
    private void TestReadyDie()
    {
        if (TryGetReadyDie(out DieBase die))
            Debug.Log($"{die.name} is ready to launch with result {die.ResultValue}.");
        else
            Debug.Log("No die is ready to launch.");
    }

    [ContextMenu("Test/Launch")]
    public void Launch()
    {
        if (!TryGetReadyDie(out DieBase die))
        {
            Debug.Log("Launch ignored. No revealed selected die.");
            return;
        }

        Debug.Log(
            $"{die.name} launched with result {die.ResultValue}."
        );

        die.LaunchArrivedEvent.RemoveListener(HandleLaunchArrived);
        die.LaunchArrivedEvent.AddListener(HandleLaunchArrived);
        die.RequestLaunch();
    }

    private void HandleLaunchArrived(DieBase die)
    {
        if (die == null)
            return;

        die.LaunchArrivedEvent.RemoveListener(
            HandleLaunchArrived
        );

        Debug.Log($"{die.name} arrived.");

        // Preserve the result on the planet before
        // MarkSpent clears the die's rolled value.
        if (planet != null)
        {
            planet.ReceiveDie(die);
        }
        else
        {
            Debug.LogWarning(
                "Launch arrived, but Planet is unassigned.",
                this
            );
        }

        if (battle != null)
        {
            battle.ReceiveDieImpact(die);
        }
        else
        {
            Debug.LogWarning(
                "Launch arrived, but Battle is unassigned.",
                this
            );
        }

        die.MarkSpent();
    }

    private IEnumerator PlayHandEntrySequence()
    {
        // Use a snapshot because CompleteHandRoll clears rollingDice.
        List<DieBase> sequence =
            new List<DieBase>(rollingDice);

        HandRollMode mode = rollProfile != null
            ? rollProfile.Mode
            : HandRollMode.Simultaneous;

        for (int i = 0; i < sequence.Count; i++)
        {
            DieBase die = sequence[i];

            if (die == null)
                continue;

            die.RequestHandEntry();

            if (mode == HandRollMode.Staggered)
            {
                bool hasAnotherDie = i < sequence.Count - 1;

                if (hasAnotherDie)
                {
                    float delay = rollProfile != null
                        ? rollProfile.StaggerDelay
                        : 0f;

                    yield return new WaitForSeconds(delay);
                }
            }
            else if (mode == HandRollMode.Sequential)
            {
                yield return new WaitUntil(
                    () => die == null ||
                          die.IsRevealed ||
                          !IsRolling
                );
            }
        }

        entrySequence = null;
    }
}