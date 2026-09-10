using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "MaterialDiceSet_",
    menuName = "Master of the Die/Material Dice Set"
)]
public sealed class MaterialDiceSet : ScriptableObject
{
    [Header("Draw Settings")]
    [SerializeField, Min(1)]
    private int diceToDraw = 7;

    [Header("Available Material Dice")]
    [SerializeField]
    private List<MaterialDieDefinition> dice =
        new List<MaterialDieDefinition>();

    public int DiceToDraw =>
        Mathf.Min(diceToDraw, dice.Count);

    public IReadOnlyList<MaterialDieDefinition> Dice =>
        dice;

    [ContextMenu("Validation/Check Dice Set")]
    private void ValidateDiceSet()
    {
        if (dice.Count == 0)
        {
            Debug.LogWarning(
                $"{name} contains no Material Dice.",
                this
            );

            return;
        }

        HashSet<MaterialDieType> foundTypes =
            new HashSet<MaterialDieType>();

        for (int i = 0; i < dice.Count; i++)
        {
            MaterialDieDefinition definition = dice[i];

            if (definition == null)
            {
                Debug.LogWarning(
                    $"{name}: Entry {i} is empty.",
                    this
                );

                continue;
            }

            if (!definition.IsValid)
            {
                Debug.LogWarning(
                    $"{name}: {definition.name} is incomplete.",
                    definition
                );
            }

            if (!foundTypes.Add(definition.MaterialType))
            {
                Debug.LogWarning(
                    $"{name}: Duplicate Material type " +
                    $"{definition.MaterialType}.",
                    definition
                );
            }
        }

        Debug.Log(
            $"{name}: {dice.Count} definitions available, " +
            $"{DiceToDraw} dice will be drawn.",
            this
        );
    }
}