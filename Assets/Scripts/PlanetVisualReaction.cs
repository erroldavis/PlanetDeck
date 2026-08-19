using UnityEngine;

public class PlanetVisualReaction : MonoBehaviour
{
    [SerializeField] private PlanetReactionController reaction;
    [SerializeField] private Transform planetVisual;

    [Header("Growth")]
    [SerializeField] private float growthPerPoint = 0.03f;
    [SerializeField] private float growthSpeed = 6f;
    [SerializeField] private float maximumScaleMultiplier = 1.5f;

    private Vector3 startingScale;
    private Vector3 targetScale;

    private void Awake()
    {
        if (planetVisual == null)
            planetVisual = transform;

        startingScale = planetVisual.localScale;
        targetScale = startingScale;
    }

    private void OnEnable()
    {
        if (reaction == null)
            return;

        reaction.ValueChangedEvent.AddListener(HandleValueChanged);
        HandleValueChanged(reaction.AccumulatedValue);
    }

    private void OnDisable()
    {
        if (reaction != null)
            reaction.ValueChangedEvent.RemoveListener(HandleValueChanged);
    }

    private void Update()
    {
        planetVisual.localScale = Vector3.Lerp(
            planetVisual.localScale,
            targetScale,
            Time.deltaTime * growthSpeed
        );
    }

    private void HandleValueChanged(int total)
    {
        float multiplier = Mathf.Min(
            1f + total * growthPerPoint,
            maximumScaleMultiplier
        );

        targetScale = startingScale * multiplier;
    }
}