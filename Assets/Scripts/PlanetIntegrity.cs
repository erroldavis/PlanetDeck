using UnityEngine;
using UnityEngine.Events;

public class PlanetIntegrity : MonoBehaviour
{
    [Header("Integrity")]
    [SerializeField, Min(1)]
    private int maxIntegrity = 100;

    [SerializeField]
    private int currentIntegrity = 100;

    public int MaxIntegrity => maxIntegrity;
    public int CurrentIntegrity => currentIntegrity;

    public UnityEvent<int, int> IntegrityChangedEvent =
        new UnityEvent<int, int>();

    public void TakeDamage(int amount)
    {
        if (amount <= 0)
            return;

        int previousIntegrity = currentIntegrity;

        currentIntegrity = Mathf.Clamp(
            currentIntegrity - amount,
            0,
            maxIntegrity
        );

        if (currentIntegrity == previousIntegrity)
            return;

        Debug.Log(
            $"Planet Integrity: {previousIntegrity} → {currentIntegrity}"
        );

        IntegrityChangedEvent.Invoke(
            currentIntegrity,
            maxIntegrity
        );
    }

    public void Repair(int amount)
    {
        if (amount <= 0)
            return;

        int previousIntegrity = currentIntegrity;

        currentIntegrity = Mathf.Clamp(
            currentIntegrity + amount,
            0,
            maxIntegrity
        );

        if (currentIntegrity == previousIntegrity)
            return;

        IntegrityChangedEvent.Invoke(
            currentIntegrity,
            maxIntegrity
        );
    }

    [ContextMenu("Test/Take Damage 10")]
    private void TestTakeDamage10()
    {
        TakeDamage(10);
    }

    private void OnValidate()
    {
        maxIntegrity = Mathf.Max(1, maxIntegrity);
        currentIntegrity = Mathf.Clamp(
            currentIntegrity,
            0,
            maxIntegrity
        );
    }
}