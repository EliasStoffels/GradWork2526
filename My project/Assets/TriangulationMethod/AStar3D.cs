//// Adapted from https://github.com/vazgriz/DungeonGenerator/blob/master/Assets/Scripts3D/DungeonPathfinder3D.cs#L6

//using System.Collections.Generic;
//using UnityEngine;

//public class AStar3D
//{
//    private readonly System.Func<Vector3Int, int> m_Cost;
//    private readonly Vector3Int m_Bounds;

//    private readonly Vector3Int[] m_Directions = new Vector3Int[]
//    {
//        new Vector3Int(1,0,0), new Vector3Int(-1,0,0),
//        new Vector3Int(0,1,0), new Vector3Int(0,-1,0),
//        new Vector3Int(0,0,1), new Vector3Int(0,0,-1)
//    };

//    public AStar3D(System.Func<Vector3Int, int> cost, Vector3Int dungeonBounds)
//    {
//        m_Cost = cost;
//        m_Bounds = dungeonBounds;
//    }

//    public List<Vector3Int> FindPath(Vector3Int start, Vector3Int end)
//    {
//        Dictionary<Vector3Int, AStarNode> allNodes = new Dictionary<Vector3Int, AStarNode>();
//        List<AStarNode> openList = new List<AStarNode>();
//        HashSet<Vector3Int> closedSet = new HashSet<Vector3Int>();

//        AStarNode startNode = new AStarNode(start) { gCost = 0, hCost = Heuristic(start, end) };
//        openList.Add(startNode);
//        allNodes[start] = startNode;

//        while (openList.Count > 0)
//        {
//            // Find node with lowest fCost
//            AStarNode current = openList[0];
//            foreach (var node in openList)
//            {
//                if (node.fCost < current.fCost || (node.fCost == current.fCost && node.hCost < current.hCost))
//                    current = node;
//            }

//            openList.Remove(current);
//            closedSet.Add(current.position);

//            if (current.position == end)
//                return ReconstructPath(current);

//            foreach (var dir in m_Directions)
//            {
//                Vector3Int neighborPos = current.position + dir;
//                if (!IsInBounds(neighborPos) || closedSet.Contains(neighborPos))
//                    continue;

//                int tileCost = m_Cost(neighborPos);
//                int gCost = current.gCost + tileCost;

//                if (!allNodes.TryGetValue(neighborPos, out AStarNode neighbor))
//                {
//                    neighbor = new AStarNode(neighborPos);
//                    allNodes[neighborPos] = neighbor;
//                }
//                else if (gCost >= neighbor.gCost)
//                    continue;

//                neighbor.gCost = gCost;
//                neighbor.hCost = Heuristic(neighborPos, end);
//                neighbor.parent = current;

//                if (!openList.Contains(neighbor))
//                    openList.Add(neighbor);
//            }
//        }

//        return null; // no path found
//    }


//    private int Heuristic(Vector3Int a, Vector3Int b)
//    {
//        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z);
//    }

//    private bool IsInBounds(Vector3Int pos)
//    {
//        return pos.x >= 0 && pos.y >= 0 && pos.z >= 0 &&
//               pos.x < m_Bounds.x &&
//               pos.y < m_Bounds.y &&
//               pos.z < m_Bounds.z;
//    }

//    private List<Vector3Int> ReconstructPath(AStarNode endNode)
//    {
//        List<Vector3Int> path = new List<Vector3Int>();
//        AStarNode current = endNode;
//        while (current != null)
//        {
//            path.Add(current.position);
//            current = current.parent;
//        }
//        path.Reverse();
//        return path;
//    }
//}

//class AStarNode
//{
//    public Vector3Int position;
//    public int gCost;
//    public int hCost;
//    public int fCost => gCost + hCost;
//    public AStarNode parent;

//    public AStarNode(Vector3Int pos)
//    {
//        position = pos;
//    }
//}

using System.Collections.Generic;
using UnityEngine;

public class AStar3D
{
    private readonly System.Func<Vector3Int, int> m_CostFunc;
    private readonly int m_SizeX;
    private readonly int m_SizeY;
    private readonly int m_SizeZ;
    private readonly int m_LayerSize;

    private readonly Node[] m_Nodes;
    private readonly MinHeap m_OpenHeap;

    private static readonly Vector3Int[] Directions =
    {
        new Vector3Int( 1, 0, 0), new Vector3Int(-1, 0, 0),
        new Vector3Int( 0, 1, 0), new Vector3Int( 0,-1, 0),
        new Vector3Int( 0, 0, 1), new Vector3Int( 0, 0,-1)
    };

    struct Node
    {
        public int gCost;
        public int hCost;
        public int parent;
        public bool closed;

        public int fCost => gCost + hCost;
    }

    public AStar3D(System.Func<Vector3Int, int> cost, Vector3Int bounds)
    {
        m_CostFunc = cost;

        m_SizeX = bounds.x;
        m_SizeY = bounds.y;
        m_SizeZ = bounds.z;
        m_LayerSize = m_SizeX * m_SizeY;

        m_Nodes = new Node[m_SizeX * m_SizeY * m_SizeZ];
        m_OpenHeap = new MinHeap(m_Nodes.Length);
    }

    public List<Vector3Int> FindPath(Vector3Int start, Vector3Int end)
    {
        int startIndex = ToIndex(start);
        int endIndex = ToIndex(end);

        ResetNodes();
        m_OpenHeap.Clear();

        m_Nodes[startIndex].gCost = 0;
        m_Nodes[startIndex].hCost = Heuristic(start, end);
        m_Nodes[startIndex].parent = -1;

        m_OpenHeap.Push(startIndex, m_Nodes[startIndex].fCost);

        while (m_OpenHeap.Count > 0)
        {
            int current = m_OpenHeap.Pop();

            if (m_Nodes[current].closed)
                continue;

            m_Nodes[current].closed = true;

            if (current == endIndex)
                return ReconstructPath(endIndex);

            Vector3Int currentPos = FromIndex(current);

            foreach (var dir in Directions)
            {
                Vector3Int nextPos = currentPos + dir;

                if (!InBounds(nextPos))
                    continue;

                int tileCost = m_CostFunc(nextPos);
                if (tileCost < 0)
                    continue;

                int nextIndex = ToIndex(nextPos);
                if (m_Nodes[nextIndex].closed)
                    continue;

                int newG = m_Nodes[current].gCost + tileCost;

                if (m_Nodes[nextIndex].parent == 0 && nextIndex != startIndex || newG < m_Nodes[nextIndex].gCost)
                {
                    m_Nodes[nextIndex].gCost = newG;
                    m_Nodes[nextIndex].hCost = Heuristic(nextPos, end);
                    m_Nodes[nextIndex].parent = current;

                    m_OpenHeap.Push(nextIndex, m_Nodes[nextIndex].fCost);
                }
            }
        }

        return null;
    }

    private void ResetNodes()
    {
        for (int i = 0; i < m_Nodes.Length; i++)
        {
            m_Nodes[i].gCost = int.MaxValue;
            m_Nodes[i].hCost = 0;
            m_Nodes[i].parent = 0;
            m_Nodes[i].closed = false;
        }
    }

    private int Heuristic(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x)
             + Mathf.Abs(a.y - b.y)
             + Mathf.Abs(a.z - b.z);
    }

    private bool InBounds(Vector3Int p)
    {
        return p.x >= 0 && p.y >= 0 && p.z >= 0 &&
               p.x < m_SizeX && p.y < m_SizeY && p.z < m_SizeZ;
    }

    private int ToIndex(Vector3Int p)
    {
        return p.x + p.y * m_SizeX + p.z * m_LayerSize;
    }

    private Vector3Int FromIndex(int index)
    {
        int z = index / m_LayerSize;
        int rem = index % m_LayerSize;
        int y = rem / m_SizeX;
        int x = rem % m_SizeX;
        return new Vector3Int(x, y, z);
    }

    private List<Vector3Int> ReconstructPath(int endIndex)
    {
        List<Vector3Int> path = new List<Vector3Int>();
        int current = endIndex;

        while (current != -1)
        {
            path.Add(FromIndex(current));
            current = m_Nodes[current].parent;
        }

        path.Reverse();
        return path;
    }
}

class MinHeap
{
    private readonly int[] m_Heap;
    private readonly int[] m_Priorities;
    private int m_Count;

    public int Count => m_Count;

    public MinHeap(int capacity)
    {
        m_Heap = new int[capacity];
        m_Priorities = new int[capacity];
    }

    public void Clear()
    {
        m_Count = 0;
    }

    public void Push(int value, int priority)
    {
        int i = m_Count++;
        m_Heap[i] = value;
        m_Priorities[i] = priority;

        while (i > 0)
        {
            int parent = (i - 1) / 2;
            if (m_Priorities[parent] <= m_Priorities[i])
                break;

            Swap(i, parent);
            i = parent;
        }
    }

    public int Pop()
    {
        int result = m_Heap[0];
        m_Count--;

        m_Heap[0] = m_Heap[m_Count];
        m_Priorities[0] = m_Priorities[m_Count];

        int i = 0;
        while (true)
        {
            int left = i * 2 + 1;
            int right = left + 1;
            if (left >= m_Count)
                break;

            int smallest = left;
            if (right < m_Count && m_Priorities[right] < m_Priorities[left])
                smallest = right;

            if (m_Priorities[i] <= m_Priorities[smallest])
                break;

            Swap(i, smallest);
            i = smallest;
        }

        return result;
    }

    private void Swap(int a, int b)
    {
        (m_Heap[a], m_Heap[b]) = (m_Heap[b], m_Heap[a]);
        (m_Priorities[a], m_Priorities[b]) = (m_Priorities[b], m_Priorities[a]);
    }
}
