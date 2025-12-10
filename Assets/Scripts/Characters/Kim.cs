using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Kim : CharacterController
{
    [SerializeField] float ContextRadius;

    public class Node
    {
        Grid.Tile Tile;
        Grid.Tile Parent;
        public void node(Grid.Tile tile, Grid.Tile parent)
        {
            Tile = tile;
            Parent = parent;
        }
    }

    public override void StartCharacter()
    {
        base.StartCharacter();
    }

    public override void UpdateCharacter()
    {
        base.UpdateCharacter();

        Zombie closest = GetClosest(GetContextByTag("Zombie"))?.GetComponent<Zombie>();
    }

    Vector3 GetEndPoint()
    {
        return Grid.Instance.WorldPos(Grid.Instance.GetFinishTile());
    }

    GameObject[] GetContextByTag(string aTag)
    {
        Collider[] context = Physics.OverlapSphere(transform.position, ContextRadius);
        List<GameObject> returnContext = new List<GameObject>();
        foreach (Collider c in context)
        {
            if (c.transform.CompareTag(aTag))
            {
                returnContext.Add(c.gameObject);
            }
        }
        return returnContext.ToArray();
    }

    GameObject GetClosest(GameObject[] aContext)
    {
        float dist = float.MaxValue;
        GameObject Closest = null;
        foreach (GameObject z in aContext)
        {
            float curDist = Vector3.Distance(transform.position, z.transform.position);
            if (curDist < dist)
            {
                dist = curDist;
                Closest = z;
            }
        }
        return Closest;
    }
    private void PathFind(Grid.Tile goal, Grid.Tile start)
    {
        float estimatedDistance = Vector3.Distance(Grid.Instance.WorldPos(goal), Grid.Instance.WorldPos(start));
        List<Grid.Tile> open = new List<Grid.Tile>();
        open.Add(start);
        HashSet<Node> closed = new HashSet<Node>();
        while (open.Count > 0)
        {
            open.Sort();
            Grid.Tile current = open[open.Count-1];
            open.RemoveAt(open.Count-1);
        }
    }
}
