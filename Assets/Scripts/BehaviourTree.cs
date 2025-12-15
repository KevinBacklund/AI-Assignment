using System.Collections.Generic;
using UnityEngine;

public enum NodeState
{
    Running,
    Success,
    Failure
}

public class Node
{
    protected NodeState state;
    public Node Parent;
    protected List<Node> Children = new List<Node>();
    protected BehaviourTree myTree;

    private Dictionary<string, object> Context = new Dictionary<string, object>();

    public Node()
    {
        Parent = null;
    }
    public Node(List<Node> someChildren)
    {
        foreach (Node node in someChildren)
        {
            AddChild(node);
        }
    }
    private void AddChild(Node node)
    {
        node.Parent = this;
        Children.Add(node);
    }
    public void SetData(string key, object data)
    {
        Context.Add(key, data);
    }
    public object GetData(string key)
    {
        return Context[key];
    }
    public virtual NodeState Evaluate() => NodeState.Failure;
}

public class Selector : Node
{
    public Selector() : base() { }
    public Selector(List<Node> someChildren) : base(someChildren) { }

    public override NodeState Evaluate()
    {
        foreach (Node node in Children)
        {
            switch (node.Evaluate())
            {
                case NodeState.Running:
                    return  NodeState.Running;
                case NodeState.Success:
                    return NodeState.Success;
                case NodeState.Failure:
                    continue;
            }
        }
        return NodeState.Failure;
    }
}

public class Sequence : Node
{
    public Sequence() : base() { }
    public Sequence(List<Node> someChildren) : base(someChildren) { }
    public override NodeState Evaluate()
    {
        bool anyChildrenRunning = false;

        foreach (Node node in Children)
        {
            switch (node.Evaluate())
            {
                case NodeState.Running:
                    anyChildrenRunning = true;
                    break;
                case NodeState.Success:
                    continue;
                case NodeState.Failure:
                    return NodeState.Failure;
            }
        }
        return anyChildrenRunning ? NodeState.Running : NodeState.Success;
    }
}

public abstract class BehaviourTree : MonoBehaviour
{
    private Node Root = null;

    protected void Start()
    {
        Root = SetupTree();
    }

    private void Update()
    {
        if (Root != null) 
        {
            Root.Evaluate();
        }
    }

    protected abstract Node SetupTree();
}
