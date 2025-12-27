// Adapted from https://github.com/vazgriz/DungeonGenerator/blob/master/Assets/Scripts3D/DungeonPathfinder3D.cs#L6

using System.Collections.Generic;
using UnityEngine;

public class AStar3D
{
    private readonly System.Func<Vector3Int, int> m_Cost;
    private readonly Vector3Int m_Bounds;

    private readonly Vector3Int[] m_Directions = new Vector3Int[]
    {
        new Vector3Int(1,0,0), new Vector3Int(-1,0,0),
        new Vector3Int(0,1,0), new Vector3Int(0,-1,0),
        new Vector3Int(0,0,1), new Vector3Int(0,0,-1)
    };

    public AStar3D(System.Func<Vector3Int, int> cost, Vector3Int dungeonBounds)
    {
        m_Cost = cost;
        m_Bounds = dungeonBounds;
    }

    public List<Vector3Int> FindPath(Vector3Int start, Vector3Int end)
    {
        Dictionary<Vector3Int, AStarNode> allNodes = new Dictionary<Vector3Int, AStarNode>();
        List<AStarNode> openList = new List<AStarNode>();
        HashSet<Vector3Int> closedSet = new HashSet<Vector3Int>();

        AStarNode startNode = new AStarNode(start) { gCost = 0, hCost = Heuristic(start, end) };
        openList.Add(startNode);
        allNodes[start] = startNode;

        while (openList.Count > 0)
        {
            // Find node with lowest fCost
            AStarNode current = openList[0];
            foreach (var node in openList)
            {
                if (node.fCost < current.fCost || (node.fCost == current.fCost && node.hCost < current.hCost))
                    current = node;
            }

            openList.Remove(current);
            closedSet.Add(current.position);

            if (current.position == end)
                return ReconstructPath(current);

            foreach (var dir in m_Directions)
            {
                Vector3Int neighborPos = current.position + dir;
                if (!IsInBounds(neighborPos) || closedSet.Contains(neighborPos))
                    continue;

                int tileCost = m_Cost(neighborPos);
                int gCost = current.gCost + tileCost;

                if (!allNodes.TryGetValue(neighborPos, out AStarNode neighbor))
                {
                    neighbor = new AStarNode(neighborPos);
                    allNodes[neighborPos] = neighbor;
                }
                else if (gCost >= neighbor.gCost)
                    continue;

                neighbor.gCost = gCost;
                neighbor.hCost = Heuristic(neighborPos, end);
                neighbor.parent = current;

                if (!openList.Contains(neighbor))
                    openList.Add(neighbor);
            }
        }

        return null; // no path found
    }


    private int Heuristic(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z);
    }

    private bool IsInBounds(Vector3Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.z >= 0 &&
               pos.x < m_Bounds.x &&
               pos.y < m_Bounds.y &&
               pos.z < m_Bounds.z;
    }

    private List<Vector3Int> ReconstructPath(AStarNode endNode)
    {
        List<Vector3Int> path = new List<Vector3Int>();
        AStarNode current = endNode;
        while (current != null)
        {
            path.Add(current.position);
            current = current.parent;
        }
        path.Reverse();
        return path;
    }
}

class AStarNode
{
    public Vector3Int position;
    public int gCost;
    public int hCost;
    public int fCost => gCost + hCost;
    public AStarNode parent;

    public AStarNode(Vector3Int pos)
    {
        position = pos;
    }
}
