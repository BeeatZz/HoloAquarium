using UnityEngine;

public class ObjectiveOwnUnits : StarObjective
{
    [Header("Settings")]
    public GremData targetGremType;
    public int requiredCount = 3;

    private void Update()
    {
        if (isComplete) return;

        int count = CountMatchingGrems();
        if (count >= requiredCount)
            Complete();
    }

    private int CountMatchingGrems()
    {
        Gremurin[] grems = FindObjectsByType<Gremurin>(FindObjectsSortMode.None);
        int count = 0;

        foreach (Gremurin g in grems)
        {
            if (!g.isDead && g.data == targetGremType)
                count++;
        }

        return count;
    }

    public override bool Evaluate()
    {
        return isComplete;
    }
}