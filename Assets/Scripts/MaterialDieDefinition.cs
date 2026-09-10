using UnityEngine;

[CreateAssetMenu(
    fileName = "MaterialDie_",
    menuName = "Master of the Die/Material Die Definition"
)]
public sealed class MaterialDieDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField]
    private MaterialDieType materialType =
        MaterialDieType.Unassigned;

    [SerializeField]
    private string materialName;

    [SerializeField]
    private string symbol;

    [SerializeField]
    private MaterialCombatRole combatRole =
        MaterialCombatRole.Unassigned;

    [Header("Roll")]
    [SerializeField, Min(2)]
    private int sideCount = 6;

    [Header("Presentation")]
    [SerializeField]
    private Color visualColor = Color.white;

    public MaterialDieType MaterialType =>
        materialType;

    public string MaterialName =>
        materialName;

    public string Symbol =>
        symbol;

    public MaterialCombatRole CombatRole =>
        combatRole;

    public int SideCount =>
        Mathf.Max(2, sideCount);

    public Color VisualColor =>
        visualColor;

    public bool IsValid =>
        materialType != MaterialDieType.Unassigned &&
        combatRole != MaterialCombatRole.Unassigned &&
        !string.IsNullOrWhiteSpace(materialName);
}