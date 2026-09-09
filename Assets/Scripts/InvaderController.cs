using UnityEngine;


public enum InvaderState
{
    Move,
    Telegraph,
    Attack,
    Reposition
}
public class InvaderController : MonoBehaviour
{
    

    [Header("References")]
    [SerializeField] private Transform planet;
    [SerializeField] private PlanetIntegrity planetIntegrity;
    [SerializeField] private Renderer invaderRenderer;

    [Header("Orbit")]
    [SerializeField, Min(0.1f)] private float orbitRadius = 3f;
    [SerializeField] private float orbitSpeed = 35f;
    [SerializeField] private float startingAngle = 0f;

    [Header("State Timing")]
    [SerializeField, Min(0.1f)] private float moveDuration = 4f;
    [SerializeField, Min(0.1f)] private float telegraphDuration = 1f;

    [Header("Telegraph")]
    [SerializeField] private Color telegraphColor = Color.red;

    [Header("Attack")]
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private Transform projectileVisual;
    [SerializeField, Min(0.1f)] private float projectileSpeed = 6f;
    [SerializeField, Min(1)] private int attackDamage = 10;
    [SerializeField, Min(0.01f)] private float projectileHitDistance = 0.05f;

    private Vector3 attackTargetPosition;

    [Header("Reposition")]
    [SerializeField, Min(1f)] private float repositionAngle = 90f;
    [SerializeField, Min(1f)] private float repositionSpeed = 120f;

    private float repositionTargetAngle;

    public InvaderState CurrentState { get; private set; }
        = InvaderState.Move;

    public event System.Action<InvaderController>
    IntentRevealed;

    public event System.Action<InvaderController>
        AttackStarted;

    private bool attackAuthorized;

    private float orbitAngle;
    private float stateTimer;

    private Material invaderMaterial;
    private Color originalColor;

    [Header("Player Impact")]
    [SerializeField] private Transform dieImpactTarget;

    public Transform DieImpactTarget
    {
        get
        {
            if (dieImpactTarget != null)
                return dieImpactTarget;

            return transform;
        }
    }

    private void Start()
    {
        if (planet == null)
        {
            Debug.LogWarning(
                $"{name} cannot move because Planet is unassigned."
            );

            enabled = false;
            return;
        }

        orbitAngle = startingAngle;
        transform.position = GetOrbitPosition();
        FacePlanet();

        stateTimer = moveDuration;

        if (invaderRenderer != null)
        {
            invaderMaterial = invaderRenderer.material;
            originalColor = invaderMaterial.color;
        }
        else
        {
            Debug.LogWarning(
                $"{name} has no Invader Renderer assigned."
            );
        }
        if (projectileVisual != null)
            projectileVisual.gameObject.SetActive(false);
    }

    private void Update()
    {
        switch (CurrentState)
        {
            case InvaderState.Move:
                UpdateMoveState();
                break;

            case InvaderState.Telegraph:
                UpdateTelegraphState();
                break;

            case InvaderState.Attack:
                UpdateAttackState();
                break;

            case InvaderState.Reposition:
                UpdateRepositionState();
                break;
        }
    }

    private void UpdateMoveState()
    {
        MoveAroundPlanet();

        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
            EnterTelegraphState();
    }

    private void MoveAroundPlanet()
    {
        orbitAngle = Mathf.Repeat(
            orbitAngle + orbitSpeed * Time.deltaTime,
            360f
        );

        transform.position = GetOrbitPosition();
        FacePlanet();
    }

    private Vector3 GetOrbitPosition()
    {
        float radians = orbitAngle * Mathf.Deg2Rad;

        Vector3 orbitOffset = new Vector3(
            Mathf.Cos(radians) * orbitRadius,
            0f,
            Mathf.Sin(radians) * orbitRadius
        );

        return planet.position + orbitOffset;
    }

    private void FacePlanet()
    {
        Vector3 direction = planet.position - transform.position;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        transform.rotation = Quaternion.LookRotation(
            direction.normalized,
            Vector3.up
        );
    }

    private void EnterTelegraphState()
    {
        CurrentState = InvaderState.Telegraph;
        stateTimer = telegraphDuration;
        attackAuthorized = false;

        if (invaderMaterial != null)
            invaderMaterial.color = telegraphColor;

        Debug.Log(
            "Invader revealed its attack intent and is waiting.",
            this
        );

        IntentRevealed?.Invoke(this);
    }

    private void UpdateTelegraphState()
    {
        if (stateTimer > 0f)
            stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f && attackAuthorized)
            EnterAttackState();
    }

    public bool TryAuthorizeAttack()
    {
        if (CurrentState != InvaderState.Telegraph)
        {
            Debug.LogWarning(
                $"{name} cannot attack because it is not Telegraphing.",
                this
            );

            return false;
        }

        attackAuthorized = true;

        Debug.Log(
            "Invader attack authorized.",
            this
        );

        if (stateTimer <= 0f)
            EnterAttackState();

        return true;
    }

    private void EnterAttackState()
    {

        CurrentState = InvaderState.Attack;
        attackAuthorized = false;

        if (invaderMaterial != null)
            invaderMaterial.color = originalColor;

        if (projectileVisual == null)
        {
            Debug.LogWarning(
                $"{name} cannot attack because Projectile Visual is unassigned."
            );

            
            EnterMoveState();
            return;
        }

        Vector3 spawnPosition = projectileSpawnPoint != null
            ? projectileSpawnPoint.position
            : transform.position;

        projectileVisual.position = spawnPosition;
        projectileVisual.gameObject.SetActive(true);

        Debug.Log("Invader fired at the planet.");
        AttackStarted?.Invoke(this);
    }

    private void UpdateAttackState()
    {
        if (projectileVisual == null)
            return;

        projectileVisual.position = Vector3.MoveTowards(
            projectileVisual.position,
            planet.position,
            projectileSpeed * Time.deltaTime
        );

        float distanceToPlanet = Vector3.Distance(
            projectileVisual.position,
            planet.position
        );

        if (distanceToPlanet <= projectileHitDistance)
            CompleteProjectileAttack();
    }

    private void CompleteProjectileAttack()
    {
        projectileVisual.gameObject.SetActive(false);

        if (planetIntegrity != null)
        {
            planetIntegrity.TakeDamage(attackDamage);
        }
        else
        {
            Debug.LogWarning(
                $"{name} cannot deal damage because Planet Integrity is unassigned."
            );
        }

        Debug.Log("Invader projectile reached the planet.");

        // Temporary until Reposition is implemented.
        EnterRepositionState();
    }

    private void EnterRepositionState()
    {
        CurrentState = InvaderState.Reposition;

        repositionTargetAngle =
            orbitAngle + repositionAngle;

        Debug.Log("Invader reposition started.");
    }

    private void UpdateRepositionState()
    {
        orbitAngle = Mathf.MoveTowards(
            orbitAngle,
            repositionTargetAngle,
            repositionSpeed * Time.deltaTime
        );

        transform.position = GetOrbitPosition();
        FacePlanet();

        if (Mathf.Abs(
                orbitAngle - repositionTargetAngle
            ) <= 0.01f)
        {
            orbitAngle = Mathf.Repeat(
                repositionTargetAngle,
                360f
            );

            transform.position = GetOrbitPosition();
            FacePlanet();

            EnterMoveState();
        }
    }

    private void EnterMoveState()
    {
        CurrentState = InvaderState.Move;
        stateTimer = moveDuration;

        if (invaderMaterial != null)
            invaderMaterial.color = originalColor;

        Debug.Log("Invader returned to movement.");
    }

    private void OnDestroy()
    {
        if (invaderMaterial != null)
            Destroy(invaderMaterial);
    }


    [ContextMenu("Test/Authorize Telegraphed Attack")]
    private void TestAuthorizeTelegraphedAttack()
    {
        TryAuthorizeAttack();
    }

}