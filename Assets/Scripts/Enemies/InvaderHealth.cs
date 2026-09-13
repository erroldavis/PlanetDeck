using UnityEngine;

public class InvaderHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField, Min(1)]
    private int maxHealth = 10;

    [SerializeField]
    private int currentHealth = 10;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0)
            return;

        int previousHealth = currentHealth;

        currentHealth = Mathf.Clamp(
            currentHealth - amount,
            0,
            maxHealth
        );

        Debug.Log(
            $"Invader Health: {previousHealth} → {currentHealth}",
            this
        );
    }

    [ContextMenu("Test/Take 1 Damage")]
    private void TestTakeDamage()
    {
        TakeDamage(1);
    }

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);

        currentHealth = Mathf.Clamp(
            currentHealth,
            0,
            maxHealth
        );
    }
}