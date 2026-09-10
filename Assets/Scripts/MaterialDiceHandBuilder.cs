using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class MaterialDiceHandBuilder : MonoBehaviour
{
    [Header("Dice")]
    [SerializeField] private MaterialDiceSet diceSet;
    [SerializeField] private DieBase logicalDiePrefab;
    [SerializeField] private DieVisual3D visualDiePrefab;

    [Header("Scene References")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Transform dragPlane;
    [SerializeField] private Transform handLaunchPoint;
    [SerializeField] private GateABattleController battle;

    private void Awake()
    {
        BuildHand();
    }

    private void BuildHand()
    {
        if (diceSet == null ||
            logicalDiePrefab == null ||
            visualDiePrefab == null ||
            visualRoot == null)
        {
            Debug.LogWarning(
                $"{name} cannot build the Material Dice hand.",
                this
            );

            return;
        }

        List<Transform> slots = new List<Transform>();

        for (int i = 0; i < transform.childCount; i++)
            slots.Add(transform.GetChild(i));

        slots.Sort(
            (a, b) => a.position.x.CompareTo(b.position.x)
        );

        int diceCount = Mathf.Min(
            diceSet.DiceToDraw,
            slots.Count
        );

        for (int i = 0; i < diceCount; i++)
        {
            MaterialDieDefinition definition =
                diceSet.Dice[i];

            DieBase logicalDie = Instantiate(
                logicalDiePrefab,
                slots[i]
            );

            logicalDie.SetDragPlane(dragPlane);

            if (!logicalDie.Configure(definition))
            {
                Destroy(logicalDie.gameObject);
                continue;
            }

            DieVisual3D visualDie = Instantiate(
                visualDiePrefab,
                visualRoot
            );

            visualDie.name =
                $"DieVisual_{definition.MaterialName}";

            visualDie.Bind(
                logicalDie,
                handLaunchPoint,
                battle
            );
        }

        Debug.Log(
            $"{diceCount} Material Dice created.",
            this
        );
    }
}