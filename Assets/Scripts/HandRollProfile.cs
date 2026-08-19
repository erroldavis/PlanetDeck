using UnityEngine;

[CreateAssetMenu(
    fileName = "HandRollProfile",
    menuName = "Planet Prototype/Hand Roll Profile"
)]
public class HandRollProfile : ScriptableObject
{
    [SerializeField]
    private HandRollMode mode =
        HandRollMode.Staggered;

    [SerializeField, Min(0f)] private float staggerDelay = 0.12f;

    public HandRollMode Mode => mode;
    public float StaggerDelay => staggerDelay;
}