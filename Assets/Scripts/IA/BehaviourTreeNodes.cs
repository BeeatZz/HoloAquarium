using System;
using System.Collections.Generic;

public enum BTResult { Success, Failure, Running }

public abstract class BTNode
{
    public abstract BTResult Tick();
}

public class BTSelector : BTNode
{
    private List<BTNode> children;
    public BTSelector(List<BTNode> children) { this.children = children; }

    public override BTResult Tick()
    {
        foreach (BTNode child in children)
        {
            BTResult result = child.Tick();
            if (result != BTResult.Failure) return result;
        }
        return BTResult.Failure;
    }
}

public class BTSequence : BTNode
{
    private List<BTNode> children;
    public BTSequence(List<BTNode> children) { this.children = children; }

    public override BTResult Tick()
    {
        foreach (BTNode child in children)
        {
            BTResult result = child.Tick();
            if (result != BTResult.Success) return result;
        }
        return BTResult.Success;
    }
}

public class BTInverter : BTNode
{
    private BTNode child;
    public BTInverter(BTNode child) { this.child = child; }

    public override BTResult Tick()
    {
        BTResult result = child.Tick();
        if (result == BTResult.Success) return BTResult.Failure;
        if (result == BTResult.Failure) return BTResult.Success;
        return BTResult.Running;
    }
}

public class BTReactor : BTNode
{
    private BTNode mainChild;
    private BTNode reactChild;
    private Func<bool> condition;

    public BTReactor(BTNode mainChild, BTNode reactChild, Func<bool> condition)
    {
        this.mainChild = mainChild;
        this.reactChild = reactChild;
        this.condition = condition;
    }

    public override BTResult Tick()
    {
        if (condition()) return reactChild.Tick();
        return mainChild.Tick();
    }
}

public class BTAction : BTNode
{
    private Func<BTResult> action;
    public BTAction(string name, Func<BTResult> action) { this.action = action; }
    public override BTResult Tick() => action();
}

public class BTCondition : BTNode
{
    private Func<bool> condition;
    public BTCondition(Func<bool> condition) { this.condition = condition; }
    public override BTResult Tick() => condition() ? BTResult.Success : BTResult.Failure;
}