public class ObjectiveCompleteLevel : StarObjective
{
    public override bool Evaluate()
    {
        return LevelManager.Instance.levelComplete;
    }
}