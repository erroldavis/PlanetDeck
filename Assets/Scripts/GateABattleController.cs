using UnityEngine;

public enum GateABattlePhase
{
    Setup,
    InvaderIntent,
    PlayerPreparation,
    MaterialResolution,
    InvaderAttack,
    ActiveDefense,
    Consequence,
    Reposition
}

public class GateABattleController : MonoBehaviour
{
    [Header("Gate A")]
    [SerializeField] private InvaderController activeInvader;

    [Header("Prepared Defense")]
    [SerializeField] private GameObject shieldVisual;

    [Header("Material Preparation")]
    [SerializeField] private Transform planetMaterialTarget;
    public InvaderController ActiveInvader =>
        activeInvader;
    public GateABattlePhase CurrentPhase =>
    currentPhase;

    public event System.Action<GateABattlePhase>
        PhaseChanged;

    [Header("Battle Phase")]
    [SerializeField]
    private GateABattlePhase currentPhase =
    GateABattlePhase.Setup;

    private void Awake()
    {
        if (shieldVisual != null)
            shieldVisual.SetActive(false);
    }

    public bool TryGetActiveLaunchTarget(
    out Transform launchTarget)
    {
        launchTarget = null;

        if (activeInvader == null)
            return false;

        if (currentPhase !=
            GateABattlePhase.PlayerPreparation)
        {
            return false;
        }

        if (planetMaterialTarget == null)
            return false;

        launchTarget = planetMaterialTarget;
        return true;
    }

    public void ReceiveDieImpact(DieBase die)
    {
        if (die == null)
            return;

        if (currentPhase !=
            GateABattlePhase.PlayerPreparation)
        {
            Debug.LogWarning(
                $"Die impact ignored during {currentPhase}.",
                this
            );

            return;
        }

        if (activeInvader == null)
        {
            Debug.LogWarning(
                "Die impact ignored. Active Invader is unassigned.",
                this
            );

            return;
        }

        PreparedMaterialType = die.MaterialType;
        PreparedCombatRole = die.CombatRole;
        PreparedResult = die.ResultValue;

        Debug.Log(
            $"Prepared {PreparedMaterialType}: " +
            $"Role = {PreparedCombatRole}, " +
            $"Result = {PreparedResult}.",
            this
        );

        if (shieldVisual != null)
        {
            bool preparedShield =
                PreparedCombatRole ==
                MaterialCombatRole.Defend;

            shieldVisual.SetActive(preparedShield);
        }

        SetPhase(GateABattlePhase.MaterialResolution);

        if (!activeInvader.TryAuthorizeAttack())
        {
            Debug.LogWarning(
                "The Invader attack could not be authorized.",
                this
            );

            SetPhase(
                GateABattlePhase.PlayerPreparation
            );
        }
    }

    [ContextMenu("Test/Check Active Invader")]
    private void TestCheckActiveInvader()
    {
        if (TryGetActiveLaunchTarget(
                out Transform launchTarget))
        {
            Debug.Log(
                $"Active Invader: {activeInvader.name}. " +
                $"Launch Target: {launchTarget.name}."
            );
        }
        else
        {
            Debug.LogWarning(
                "No active Invader launch target."
            );
        }
    }

    private void SetPhase(
    GateABattlePhase nextPhase)
    {
        if (currentPhase == nextPhase)
            return;

        GateABattlePhase previousPhase =
            currentPhase;

        currentPhase = nextPhase;

        Debug.Log(
            $"[Gate A] Phase changed: " +
            $"{previousPhase} → {currentPhase}.",
            this
        );

        PhaseChanged?.Invoke(currentPhase);
    }

    [ContextMenu("Test/Advance Battle Phase")]
    private void TestAdvanceBattlePhase()
    {
        GateABattlePhase nextPhase;

        switch (currentPhase)
        {
            case GateABattlePhase.Setup:
                nextPhase =
                    GateABattlePhase.InvaderIntent;
                break;

            case GateABattlePhase.InvaderIntent:
                nextPhase =
                    GateABattlePhase.PlayerPreparation;
                break;

            case GateABattlePhase.PlayerPreparation:
                nextPhase =
                    GateABattlePhase.MaterialResolution;
                break;

            case GateABattlePhase.MaterialResolution:
                nextPhase =
                    GateABattlePhase.InvaderAttack;
                break;

            case GateABattlePhase.InvaderAttack:
                nextPhase =
                    GateABattlePhase.ActiveDefense;
                break;

            case GateABattlePhase.ActiveDefense:
                nextPhase =
                    GateABattlePhase.Consequence;
                break;

            case GateABattlePhase.Consequence:
                nextPhase =
                    GateABattlePhase.Reposition;
                break;

            default:
                nextPhase =
                    GateABattlePhase.InvaderIntent;
                break;
        }

        SetPhase(nextPhase);
    }

    [ContextMenu("Test/Reset Battle Phase")]
    private void TestResetBattlePhase()
    {
        SetPhase(GateABattlePhase.Setup);
    }

    private void OnEnable()
    {
        if (activeInvader == null)
            return;

        activeInvader.IntentRevealed +=
            HandleInvaderIntentRevealed;

        activeInvader.AttackStarted +=
            HandleInvaderAttackStarted;
    }

    private void OnDisable()
    {
        if (activeInvader == null)
            return;

        activeInvader.IntentRevealed -=
            HandleInvaderIntentRevealed;

        activeInvader.AttackStarted -=
            HandleInvaderAttackStarted;
    }

    private void HandleInvaderIntentRevealed(
    InvaderController invader)
    {
        if (invader != activeInvader)
            return;

        SetPhase(GateABattlePhase.InvaderIntent);
        SetPhase(GateABattlePhase.PlayerPreparation);
    }

    private void HandleInvaderAttackStarted(
        InvaderController invader)
    {
        if (invader != activeInvader)
            return;

        SetPhase(GateABattlePhase.InvaderAttack);
    }

    public MaterialDieType PreparedMaterialType
    {
        get;
        private set;
    } = MaterialDieType.Unassigned;

    public MaterialCombatRole PreparedCombatRole
    {
        get;
        private set;
    } = MaterialCombatRole.Unassigned;

    public int PreparedResult
    {
        get;
        private set;
    }
}