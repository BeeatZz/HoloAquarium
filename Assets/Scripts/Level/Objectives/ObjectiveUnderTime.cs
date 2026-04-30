public class ObjectiveUnderTime : StarObjective
{
    public float timeLimitSeconds = 120f;

    public override bool Evaluate()
    {
        return LevelManager.Instance.elapsedTime <= timeLimitSeconds;
    }
}