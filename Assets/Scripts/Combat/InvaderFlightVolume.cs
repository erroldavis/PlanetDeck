using UnityEngine;

public enum InvaderFlightZone
{
    PlanetExclusion,
    CombatAirspace,
    StagingAirspace,
    HardBoundaryBuffer,
    OutsideHardBoundary
}

public sealed class InvaderFlightVolume : MonoBehaviour
{

    [Header("Center")]
    [SerializeField]
    private Transform planetCenter;

    [Header("Airspace Radii")]
    [SerializeField, Min(0.1f)]
    private float planetExclusionRadius = 2.5f;

    [SerializeField, Min(0.1f)]
    private float combatRadius = 7f;

    [SerializeField, Min(0.1f)]
    private float stagingRadius = 9f;

    [SerializeField, Min(0.1f)]
    private float hardBoundaryRadius = 11f;

    [Header("Validation")]
    [SerializeField, Min(0.01f)]
    private float minimumRadiusGap = 0.25f;

    [SerializeField, Min(0f)]
    private float pointPadding = 0.25f;

    [Header("Debug Display")]
    [SerializeField]
    private bool showZones = true;
    private bool showPreviews = true;
    private bool hasCombatPreview;
    private bool hasStagingPreview;
    private bool hasRoutePreview;

    private Vector3 combatPreview;
    private Vector3 stagingPreview;

    private Vector3 routeStart;
    private Vector3 routeExit;
    private Vector3 routeReentry;

    [SerializeField]
    private Color exclusionColor = Color.red;

    [SerializeField]
    private Color combatColor = Color.cyan;

    [SerializeField]
    private Color stagingColor = Color.yellow;

    [SerializeField]
    private Color hardBoundaryColor = Color.magenta;

    public Vector3 Center =>
        planetCenter != null
            ? planetCenter.position
            : transform.position;

    public float PlanetExclusionRadius =>
        planetExclusionRadius;

    public float CombatRadius =>
        combatRadius;

    public float StagingRadius =>
        stagingRadius;

    public float HardBoundaryRadius =>
        hardBoundaryRadius;


    public float GetDistanceFromCenter(Vector3 worldPosition)
    {
        return Vector3.Distance(worldPosition, Center);
    }

    public InvaderFlightZone GetZone(Vector3 worldPosition)
    {
        float distance = GetDistanceFromCenter(worldPosition);

        if (distance <= planetExclusionRadius)
            return InvaderFlightZone.PlanetExclusion;

        if (distance <= combatRadius)
            return InvaderFlightZone.CombatAirspace;

        if (distance <= stagingRadius)
            return InvaderFlightZone.StagingAirspace;

        if (distance <= hardBoundaryRadius)
            return InvaderFlightZone.HardBoundaryBuffer;

        return InvaderFlightZone.OutsideHardBoundary;
    }

    public bool IsInCombatAirspace(Vector3 worldPosition)
    {
        return GetZone(worldPosition) ==
               InvaderFlightZone.CombatAirspace;
    }

    public bool IsInStagingAirspace(Vector3 worldPosition)
    {
        return GetZone(worldPosition) ==
               InvaderFlightZone.StagingAirspace;
    }

    public bool IsInsideHardBoundary(Vector3 worldPosition)
    {
        return GetDistanceFromCenter(worldPosition) <=
               hardBoundaryRadius;
    }

    public Vector3 GenerateCombatPoint()
    {
        return GeneratePointInShell(
            planetExclusionRadius,
            combatRadius
        );
    }

    public Vector3 GenerateStagingPoint()
    {
        return GeneratePointInShell(
            combatRadius,
            stagingRadius
        );
    }

    private Vector3 GeneratePointInShell(
        float innerRadius,
        float outerRadius)
    {
        float shellThickness =
            outerRadius - innerRadius;

        float safePadding = Mathf.Min(
            pointPadding,
            shellThickness * 0.49f
        );

        float minimumRadius =
            innerRadius + safePadding;

        float maximumRadius =
            outerRadius - safePadding;

        float minimumCubed =
            minimumRadius * minimumRadius * minimumRadius;

        float maximumCubed =
            maximumRadius * maximumRadius * maximumRadius;

        float selectedRadius = Mathf.Pow(
            Mathf.Lerp(
                minimumCubed,
                maximumCubed,
                Random.value
            ),
            1f / 3f
        );

        return Center +
               Random.onUnitSphere * selectedRadius;
    }

    private void OnValidate()
    {
        pointPadding = Mathf.Max(0f, pointPadding);
        minimumRadiusGap =
            Mathf.Max(0.01f, minimumRadiusGap);

        planetExclusionRadius =
            Mathf.Max(0.1f, planetExclusionRadius);

        combatRadius =
            Mathf.Max(
                combatRadius,
                planetExclusionRadius + minimumRadiusGap
            );

        stagingRadius =
            Mathf.Max(
                stagingRadius,
                combatRadius + minimumRadiusGap
            );

        hardBoundaryRadius =
            Mathf.Max(
                hardBoundaryRadius,
                stagingRadius + minimumRadiusGap
            );
    }

    [ContextMenu("Preview/Combat Destination")]
    private void PreviewCombatDestination()
    {
        combatPreview = GenerateCombatPoint();
        hasCombatPreview = true;

        Debug.Log(
            $"[Flight Volume] Combat point: {combatPreview}",
            this
        );
    }

    [ContextMenu("Preview/Staging Destination")]
    private void PreviewStagingDestination()
    {
        stagingPreview = GenerateStagingPoint();
        hasStagingPreview = true;

        Debug.Log(
            $"[Flight Volume] Staging point: {stagingPreview}",
            this
        );
    }

    [ContextMenu("Preview/Exit and Re-entry Route")]
    private void PreviewExitAndReentryRoute()
    {
        routeStart = GenerateCombatPoint();
        routeExit = GenerateStagingPoint();
        routeReentry = GenerateCombatPoint();

        hasRoutePreview = true;

        Debug.Log(
            "[Flight Volume] Exit and re-entry route generated.",
            this
        );
    }

    [ContextMenu("Preview/Clear Previews")]
    private void ClearPreviews()
    {
        hasCombatPreview = false;
        hasStagingPreview = false;
        hasRoutePreview = false;
    }


    private void OnDrawGizmos()
    {
        if (showZones)
            DrawZones();

        if (showPreviews)
            DrawPreviews();
    }

    private void DrawZones()
    {
        Gizmos.color = exclusionColor;
        Gizmos.DrawWireSphere(
            Center,
            planetExclusionRadius
        );

        Gizmos.color = combatColor;
        Gizmos.DrawWireSphere(
            Center,
            combatRadius
        );

        Gizmos.color = stagingColor;
        Gizmos.DrawWireSphere(
            Center,
            stagingRadius
        );

        Gizmos.color = hardBoundaryColor;
        Gizmos.DrawWireSphere(
            Center,
            hardBoundaryRadius
        );
    }

    private void DrawPreviews()
    {
        float markerSize =
            Mathf.Max(0.1f, hardBoundaryRadius * 0.025f);

        if (hasCombatPreview)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(
                combatPreview,
                markerSize
            );
        }

        if (hasStagingPreview)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(
                stagingPreview,
                markerSize
            );
        }

        if (!hasRoutePreview)
            return;

        Gizmos.color = Color.white;
        Gizmos.DrawLine(routeStart, routeExit);
        Gizmos.DrawLine(routeExit, routeReentry);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(routeStart, markerSize);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(routeExit, markerSize);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(routeReentry, markerSize);
    }

    [ContextMenu("Validation/Check Airspace Setup")]
    private void ValidateAirspaceSetup()
    {
        bool hasPlanetCenter =
            planetCenter != null;

        Debug.Assert(
            hasPlanetCenter,
            "[Flight Volume] Planet Center must be assigned.",
            this
        );

        if (!hasPlanetCenter)
            return;

        bool radiiAreOrdered =
            planetExclusionRadius < combatRadius &&
            combatRadius < stagingRadius &&
            stagingRadius < hardBoundaryRadius;

        Debug.Assert(
            radiiAreOrdered,
            "[Flight Volume] Radii are not correctly ordered.",
            this
        );

        if (!radiiAreOrdered)
            return;

        Vector3 testCombatPoint =
            GenerateCombatPoint();

        Vector3 testStagingPoint =
            GenerateStagingPoint();

        bool combatPointIsValid =
            GetZone(testCombatPoint) ==
            InvaderFlightZone.CombatAirspace;

        bool stagingPointIsValid =
            GetZone(testStagingPoint) ==
            InvaderFlightZone.StagingAirspace;

        Debug.Assert(
            combatPointIsValid,
            "[Flight Volume] Generated combat point is invalid.",
            this
        );

        Debug.Assert(
            stagingPointIsValid,
            "[Flight Volume] Generated staging point is invalid.",
            this
        );

        if (!combatPointIsValid || !stagingPointIsValid)
            return;

        Debug.Log(
            "[Flight Volume] Setup valid. " +
            $"Exclusion: {planetExclusionRadius}, " +
            $"Combat: {combatRadius}, " +
            $"Staging: {stagingRadius}, " +
            $"Hard Boundary: {hardBoundaryRadius}.",
            this
        );
    }
}