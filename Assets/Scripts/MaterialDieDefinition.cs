using UnityEngine;

[CreateAssetMenu(
    fileName = "MaterialDie_",
    menuName = "Planet Prototype/Material Die Definition"
)]
public class MaterialDieDefinition : ScriptableObject
{
    [SerializeField] private string materialName;
    [SerializeField] private string symbol;

    [SerializeField, Min(2)]
    private int sideCount = 6;

    [SerializeField]
    private Color visualColor = Color.white;

    public string MaterialName => materialName;
    public string Symbol => symbol;
    public int SideCount => Mathf.Max(2, sideCount);
    public Color VisualColor => visualColor;
}