using UnityEngine;

public class DiceRollService : MonoBehaviour
{
    public int Roll(int sideCount)
    {
        int safeSideCount = Mathf.Max(2, sideCount);

        return Random.Range(1, safeSideCount + 1);
    }

    [ContextMenu("Test/Roll D6")]
    public void TestRollD6()
    {
        int result = Roll(6);
        Debug.Log($"D6 rolled {result}");
    }
}