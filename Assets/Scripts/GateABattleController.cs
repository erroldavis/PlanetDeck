using UnityEngine;

public class GateABattleController : MonoBehaviour
{
    [Header("Gate A")]
    [SerializeField] private InvaderController activeInvader;

    public InvaderController ActiveInvader =>
        activeInvader;

    public bool TryGetActiveLaunchTarget(
        out Transform launchTarget)
    {
        launchTarget = null;

        if (activeInvader == null)
            return false;

        launchTarget = activeInvader.DieImpactTarget;

        return launchTarget != null;
    }

    public void ReceiveDieImpact(DieBase die)
    {
        if (die == null)
            return;

        if (activeInvader == null)
        {
            Debug.LogWarning(
                "Die impact ignored. Active Invader is unassigned."
            );

            return;
        }

        Debug.Log(
            $"{die.name} impacted {activeInvader.name} " +
            $"with result {die.ResultValue}."
        );
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
}