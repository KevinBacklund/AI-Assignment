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
        public Grid.Tile Tile;
        public Node Parent;
        public float Wheight;

        public Node (Grid.Tile tile, Node parent = null, float wheight = 0)
        {
            Tile = tile;
            Parent = parent;
            Wheight = wheight;
        }
    }
    List<Burger> burgers = new List<Burger>();
    public float pathfindCooldown = 0;

    public override void StartCharacter()
    {
        base.StartCharacter();

        burgers = FindObjectsOfType<Burger>(true).ToList();

    }

    public override void UpdateCharacter()
    {
        base.UpdateCharacter();

        Zombie closestZombie = GetClosest(GetContextByTag("Zombie"))?.GetComponent<Zombie>();
        Burger closestBurger = GetClosest(GetContextByTag("Burger"))?.GetComponent<Burger>();
        pathfindCooldown -= Time.deltaTime;

        //Test
        if ((myWalkBuffer.Count == 0 || closestZombie != null) && pathfindCooldown <= 0 && burgers.Count < 0)
        {
            PathFind(myCurrentTile, Grid.Instance.TryGetTile(new Vector2Int((int)closestBurger.transform.position.x, (int)closestBurger.transform.position.y)));
            pathfindCooldown = 0.5f;
        }
        else if ((myWalkBuffer.Count == 0 || closestZombie != null) && pathfindCooldown <= 0)
        {
            PathFind(myCurrentTile, Grid.Instance.GetFinishTile());
            pathfindCooldown = 0.5f;
        }
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
    private void PathFind(Grid.Tile start, Grid.Tile goal)
    {
        float distanceToStart = 0;
        List<Node> open = new List<Node>();
        open.Add(new Node(start));
        List <Grid.Tile> openTiles = new List<Grid.Tile>();
        openTiles.Add(start);
        HashSet<Grid.Tile> closed = new HashSet<Grid.Tile>();
        Zombie closestZombie = GetClosest(GetContextByTag("Zombie"))?.GetComponent<Zombie>();
        while (open.Count > 0)
        {
            open.Sort((b,a) => (a.Wheight.CompareTo(b.Wheight)));
            Node current = open[open.Count-1];
            open.RemoveAt(open.Count-1);
            distanceToStart += 1;
            if (current.Tile == goal)
            {
                List<Grid.Tile> path = new List<Grid.Tile>();
                path.Add(current.Tile);
                while (current.Parent != null)
                {
                    path.Add(current.Parent.Tile);
                    current = current.Parent;
                }
                path.Reverse();
                myWalkBuffer = path;
                return;
            }
            foreach (Grid.Tile neighbor in FindNeighbors(current.Tile))
            {
                if (!openTiles.Contains(neighbor) && neighbor != null && !neighbor.occupied)
                {
                    float wheight = Vector3.Distance(Grid.Instance.WorldPos(goal), Grid.Instance.WorldPos(neighbor));
                    wheight += distanceToStart;
                    if (closestZombie != null)
                    {
                        wheight += Mathf.Max(0, 10 - Vector3.Distance(closestZombie.transform.position, Grid.Instance.WorldPos(neighbor)));
                    }
                    open.Add(new Node(neighbor, current, wheight));
                    openTiles.Add(neighbor);
                }
            }
            closed.Add(current.Tile);
        }
    }

    public List<Grid.Tile> FindNeighbors(Grid.Tile tile)
    {
        List<Grid.Tile> result = new List<Grid.Tile>();
        result.Add(Grid.Instance.TryGetTile(new Vector2Int(tile.x + 1, tile.y)));
        result.Add(Grid.Instance.TryGetTile(new Vector2Int(tile.x - 1, tile.y)));
        result.Add(Grid.Instance.TryGetTile(new Vector2Int(tile.x, tile.y + 1)));
        result.Add(Grid.Instance.TryGetTile(new Vector2Int(tile.x, tile.y - 1)));

        return result;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        foreach (Grid.Tile tile in myWalkBuffer)
        {
            Vector3 cubePos = new Vector3();
            cubePos = Grid.Instance.WorldPos(tile);
            Gizmos.DrawCube(cubePos, Vector3.one * 0.5f);
        }
    }
}
