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
    public List<Grid.Tile> burgerTiles = new List<Grid.Tile>();
    private float pathfindCooldown = 0;
    int targetBurger = 0;
    public override void StartCharacter()
    {
        base.StartCharacter();

        burgers = FindObjectsOfType<Burger>(true).ToList();
        foreach (Burger burger in burgers)
        {
           burgerTiles.Add(Grid.Instance.GetClosest(burger.transform.position));
        }

    }

    public override void UpdateCharacter()
    {
        base.UpdateCharacter();

        //Zombie closestZombie = GetClosest(GetContextByTag("Zombie"))?.GetComponent<Zombie>();
        pathfindCooldown -= Time.deltaTime;
        targetBurger = GamesManager.Instance.GetCollectedBurgers;


        //Test
        /*
        if ((myWalkBuffer.Count == 0 || closestZombie != null) && pathfindCooldown <= 0 && GamesManager.Instance.GetCollectedBurgers < burgers.Count)
        {
            PathFind(myCurrentTile, burgerTiles[targetBurger], closestZombie);
            pathfindCooldown = 1;
        }
        else if ((myWalkBuffer.Count == 0 || closestZombie != null) && pathfindCooldown <= 0)
        {
            PathFind(myCurrentTile, Grid.Instance.GetFinishTile(), closestZombie);
            pathfindCooldown = 1;
        }*/
    }

    Vector3 GetEndPoint()
    {
        return Grid.Instance.WorldPos(Grid.Instance.GetFinishTile());
    }

    public GameObject[] GetContextByTag(string aTag)
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

    public GameObject GetClosest(GameObject[] aContext)
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
    public void PathFind(Grid.Tile start, Grid.Tile goal)
    {
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
                    float cost = Vector3.Distance(Grid.Instance.WorldPos(goal), Grid.Instance.WorldPos(neighbor));
                    cost += current.Wheight + Vector3.Distance(Grid.Instance.WorldPos(current.Tile), Grid.Instance.WorldPos(neighbor));
                    if (closestZombie != null)
                    {
                        cost += Mathf.Max(0, 20 - 5*Vector3.Distance(closestZombie.transform.position, Grid.Instance.WorldPos(neighbor)));
                    }
                    open.Add(new Node(neighbor, current, cost));
                    openTiles.Add(neighbor);
                }
            }
            closed.Add(current.Tile);
        }
    }

    public List<Grid.Tile> FindNeighbors(Grid.Tile tile)
    {
        List<Grid.Tile> result = new List<Grid.Tile>();
        for (int x = tile.x - 1; x < tile.x + 2; x++)
        {
            for (int y = tile.y - 1; y < tile.y + 2; y++)
            {
                if (y != 0 && x != 0)
                {
                    result.Add(Grid.Instance.TryGetTile(new Vector2Int(x, y)));
                }
            }
        }
        return result;
    }

    public void ClearWalkBuffer()
    {
        myWalkBuffer.Clear();
    }

    public Grid.Tile GetCurrentTile()
    {
        return myCurrentTile;
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
