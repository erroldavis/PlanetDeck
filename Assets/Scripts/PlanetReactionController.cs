using UnityEngine;
using UnityEngine.Events;

public class PlanetReactionController : MonoBehaviour
{
    [SerializeField] private int accumulatedValue;

    public UnityEvent<int> ValueChangedEvent =
        new UnityEvent<int>();

    public int AccumulatedValue => accumulatedValue;

    public void ReceiveDie(DieBase die)
    {
        if (die == null || !die.IsRevealed)
            return;

        accumulatedValue += die.ResultValue;

        ValueChangedEvent.Invoke(accumulatedValue);

        Debug.Log(
            $"Planet received {die.name}: {die.ResultValue}. " +
            $"Total: {accumulatedValue}"
        );

    }
}
