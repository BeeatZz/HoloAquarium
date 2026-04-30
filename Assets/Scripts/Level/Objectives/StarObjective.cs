using UnityEngine;

public abstract class StarObjective : MonoBehaviour
{
    [Header("Objective")]
    public string label;
    public bool isComplete;

    protected virtual void Start()
    {
        isComplete = false;
    }

    public virtual bool Evaluate()
    {
        return isComplete;
    }

    protected void Complete()
    {
        isComplete = true;
    }
}