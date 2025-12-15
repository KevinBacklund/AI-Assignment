using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class KimBT : BehaviourTree
{
    public Kim kim;

    protected override Node SetupTree()
    {
        Node root = new Sequence(new List<Node>
        {
            new TaskAvoidZombie(this),
            new Sequence(new List<Node>
            {
                new TaskChooseBurger(this),
                new TaskMoveToBurger(this),
            }),
            new TaskMoveToFinish(this)
        });

        return root;
    }
}
public class TaskChooseBurger : Node
{
    public TaskChooseBurger(BehaviourTree behaviourTree) { myTree = behaviourTree; }
    public override NodeState Evaluate()
    {
        KimBT bt = myTree as KimBT;
        if (GamesManager.Instance.GetBurgerCount < GamesManager.Instance.GetCollectedBurgers)
        {
            Parent.SetData("Target", bt.kim.burgerTiles[GamesManager.Instance.GetCollectedBurgers]);
            return NodeState.Success;
        }
        return NodeState.Failure;
    }
}
public class TaskMoveToBurger : Node
{
    public TaskMoveToBurger(BehaviourTree behaviourTree) { myTree = behaviourTree; }
    public override NodeState Evaluate()
    {
        KimBT bt = myTree as KimBT;
        bt.kim.PathFind(bt.kim.GetCurrentTile(), bt.kim.burgerTiles[(int)Parent.GetData("Target")]);
        return NodeState.Success;
    }
}
public class TaskMoveToFinish : Node
{
    public TaskMoveToFinish(BehaviourTree behaviourTree) { myTree = behaviourTree; }
    public override NodeState Evaluate()
    {
        KimBT bt = myTree as KimBT;
        bt.kim.PathFind(bt.kim.GetCurrentTile(), Grid.Instance.GetFinishTile());
        return NodeState.Success;
    }
}
public class TaskAvoidZombie : Node
{
    public TaskAvoidZombie(BehaviourTree behaviourTree) : base() { myTree = behaviourTree; }

    public override NodeState Evaluate()
    {
        KimBT bt = myTree as KimBT;
        Zombie closestZombie = bt.kim.GetClosest(bt.kim.GetContextByTag("Zombie"))?.GetComponent<Zombie>();
        if (closestZombie == null)
        {
            return NodeState.Failure;
        }
        else
        {
            bt.kim.ClearWalkBuffer();
            return NodeState.Success;
        }
    }
}