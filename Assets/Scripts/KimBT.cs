using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class KimBT : BehaviourTree
{
    public Kim kim;
    [HideInInspector]
    public float waitTime = 0;

    protected override Node SetupTree()
    {
        Node root = new Selector(new List<Node>
        {
            new TaskWaitForMove(this),
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

public class TaskWaitForMove : Node
{
    public TaskWaitForMove(BehaviourTree behaviourTree) { myTree = behaviourTree; }
    public override NodeState Evaluate()
    {
        KimBT bt = myTree as KimBT;
        if (bt.waitTime > 0)
        {
            bt.waitTime -= Time.deltaTime;
            return NodeState.Success;
        }
        bt.waitTime = 0.25f;
        return NodeState.Failure;
    }
}
public class TaskChooseBurger : Node
{
    public TaskChooseBurger(BehaviourTree behaviourTree) { myTree = behaviourTree; }
    public override NodeState Evaluate()
    {
        KimBT bt = myTree as KimBT;
        if (GamesManager.Instance.GetBurgerCount > GamesManager.Instance.GetCollectedBurgers)
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
        bt.kim.previewColor = Color.blue;
        bt.kim.PathFind(bt.kim.GetCurrentTile(), (Grid.Tile)Parent.GetData("Target"));
        return NodeState.Success;
    }
}
public class TaskMoveToFinish : Node
{
    public TaskMoveToFinish(BehaviourTree behaviourTree) { myTree = behaviourTree; }
    public override NodeState Evaluate()
    {
        KimBT bt = myTree as KimBT;
        bt.kim.previewColor = Color.green;
        bt.kim.PathFind(bt.kim.GetCurrentTile(), Grid.Instance.GetFinishTile());
        return NodeState.Success;
    }
}
public class TaskAvoidZombie : Node
{
    public TaskAvoidZombie(BehaviourTree behaviourTree) : base() { myTree = behaviourTree; }

    public override NodeState Evaluate()
    {
        float safeDistance = 2.5f;
        KimBT bt = myTree as KimBT;
        Zombie closestZombie = bt.kim.GetClosest(bt.kim.GetContextByTag("Zombie"))?.GetComponent<Zombie>();
        if (closestZombie == null)
        {
            return NodeState.Failure;
        }
        else if (Vector3.Distance(closestZombie.transform.position, bt.kim.transform.position) < safeDistance)
        {
            bt.kim.previewColor = Color.red;
            Grid.Tile awayTile = Grid.Instance.GetClosest(bt.kim.transform.position + (bt.kim.transform.position - closestZombie.transform.position).normalized);
            bt.kim.PathFind(bt.kim.GetCurrentTile(), awayTile);
            return NodeState.Success;
        }
        else
        {
            return NodeState.Failure;
        }
    }
}