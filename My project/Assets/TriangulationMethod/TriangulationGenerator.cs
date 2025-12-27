using System.Collections.Generic;
using UnityEngine;
public class Room
{
    public Vector3Int gridPosition;
    public Vector3Int size;
}
public enum Tile
{
    Empty, Room, Hallway
}

public class TriangulationGenerator : MonoBehaviour
{
    [SerializeField]
    private Material m_HallwayMaterial;
    [SerializeField]
    private int m_Seed = 0;
    [SerializeField]
    private Vector3Int m_DungeonBounds = Vector3Int.zero;
    [SerializeField]
    private int m_TargetRoomCount = 20;
    [SerializeField]
    private int m_RoomAttempts = 10;
    [SerializeField]
    private Vector3Int m_MinRoomSize = Vector3Int.zero;
    [SerializeField]
    private Vector3Int m_MaxRoomSize = Vector3Int.zero;
    [SerializeField]
    private int m_EdgesToKeep = 0;

    private System.Random m_Random;
    private Tile[,,] m_DungeonLayout;
    private Room[] m_Rooms;
    private int m_FinalRoomCount;

    private List<Edge> m_Edges = new List<Edge>();
    private List<Edge> m_RoomConnections = new List<Edge>();
    public void Generate()
    {
        Initialize();
        GenerateRooms();
        TriangulateRooms();
        GenerateConnections();
        GenerateHallways();
        Generate3DModels();
    }

    private void Initialize()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        m_Edges.Clear();
        m_RoomConnections.Clear();
        m_FinalRoomCount = 0;
        m_Rooms = new Room[m_TargetRoomCount];
        m_Random = new System.Random(m_Seed);
        m_DungeonLayout = new Tile[m_DungeonBounds.x,m_DungeonBounds.y,m_DungeonBounds.z];
        
    }

    private void GenerateRooms()
    {
        Vector3Int position = Vector3Int.zero;
        for(int roomIdx = 0;  roomIdx < m_TargetRoomCount; ++roomIdx)
        {
            position.x = m_Random.Next(m_DungeonBounds.x - m_MinRoomSize.x + 1);
            position.y = m_Random.Next(m_DungeonBounds.y - m_MinRoomSize.y + 1);
            position.z = m_Random.Next(m_DungeonBounds.z - m_MinRoomSize.z + 1);

            TrySpawnRoom(position);
        }
    }

    private void TrySpawnRoom(Vector3Int position)
    {
        Room currentRoom = new Room();
        Vector3Int roomSize = Vector3Int.zero;
        for (int attemptIdx = 0; attemptIdx < m_RoomAttempts; ++attemptIdx)
        {
            roomSize.x = m_Random.Next(m_MinRoomSize.x, m_MaxRoomSize.x);
            roomSize.y = m_Random.Next(m_MinRoomSize.y, m_MaxRoomSize.y);
            roomSize.z = m_Random.Next(m_MinRoomSize.z, m_MaxRoomSize.z);

            if(CanPlaceRoom(position, roomSize))
            {
                //register room
                //Debug.Log("succeeded after " + attemptIdx + " attempts");
                RegisterRoom(position, roomSize);
                currentRoom.gridPosition = position;
                currentRoom.size = roomSize;
                m_Rooms[m_FinalRoomCount] = currentRoom;
                ++m_FinalRoomCount;
                return;
            }

            //if(attemptIdx == m_RoomAttempts - 1)
            //{
            //    Debug.Log("failed to spawn a room");
            //}

        }
    }
    
    private bool CanPlaceRoom(Vector3Int pos, Vector3Int size)
    {
        if (pos.x + size.x > m_DungeonBounds.x)
            return false;
        if (pos.y + size.y > m_DungeonBounds.y)
            return false;
        if (pos.z + size.z > m_DungeonBounds.z)
            return false;

        for (int x  = pos.x; x < (pos.x + size.x); ++x)
            for(int y = pos.y; y < (pos.y + size.y); ++y)
                for(int z = pos.z;  z < (pos.z + size.z); ++z)
                {
                    if (m_DungeonLayout[x, y, z] != Tile.Empty)
                        return false;
                }

        return true;
    }

    private void RegisterRoom(Vector3Int pos, Vector3Int size)
    {
        for (int x = pos.x; x < (pos.x + size.x); ++x)
            for (int y = pos.y; y < (pos.y + size.y); ++y)
                for (int z = pos.z; z < (pos.z + size.z); ++z)
                    m_DungeonLayout[x, y, z] = Tile.Room;
    }

    private void TriangulateRooms()
    {
        Vector3[] roomPositions = new Vector3[m_FinalRoomCount];
        for (int roomIdx = 0; roomIdx < m_FinalRoomCount; ++roomIdx)
        {
            roomPositions[roomIdx] = m_Rooms[roomIdx].gridPosition;
        }

        m_Edges = GraphBuilder.GenerateEdges(TriangulationMethods.Triangulate(roomPositions));
    }

    private void GenerateConnections()
    {
        m_RoomConnections = MinimumSpanningTree.GenerateMSP(m_Edges, m_Edges[0].v1);
        List<Edge> outEdges = new List<Edge>();
        foreach(Edge edge in m_Edges)
        {
            if (m_RoomConnections.Contains(edge))
                continue;
            outEdges.Add(edge);
        }

        // n random elements no shuffle https://stackoverflow.com/a/48089
        // also automatically handles the case of edgestokeep being larger then outedges.count

        int needed = m_EdgesToKeep;
        int remaining = outEdges.Count;

        foreach (var edge in outEdges)
        {
            if (needed == 0)
                break;

            if (m_Random.Next(remaining) < needed)
            {
                m_RoomConnections.Add(edge);
                --needed;
            }

            --remaining;
        }
    }

    private void GenerateHallways()
    {
        var pathfinder = new AStar3D(CalculateAStarCost, m_DungeonBounds);

        foreach (Edge edge in m_RoomConnections)
        {
            Vector3Int start = new Vector3Int((int)edge.v1.x, (int)edge.v1.y, (int)edge.v1.z);
            Vector3Int end = new Vector3Int((int)edge.v2.x, (int)edge.v2.y, (int)edge.v2.z);

            List<Vector3Int> path = pathfinder.FindPath(start, end);
            if (path == null) continue;

            foreach (var cell in path)
            {
                if (m_DungeonLayout[cell.x, cell.y, cell.z] != Tile.Empty)
                    continue;

                m_DungeonLayout[cell.x, cell.y, cell.z] = Tile.Hallway;
            }
        }
    }

    int CalculateAStarCost(Vector3Int pos)
    {
        switch(m_DungeonLayout[pos.x,pos.y,pos.z])
        {
            case Tile.Empty:
                return 1;
            case Tile.Hallway:
                return 1;
            case Tile.Room:
                return 5;
        }
        return int.MaxValue;
    }

    private void Generate3DModels()
    {
        for (int x = 0; x < m_DungeonBounds.x; ++x)
            for (int y = 0; y < m_DungeonBounds.y; ++y)
                for (int z = 0; z < m_DungeonBounds.z; ++z)
                {
                    switch(m_DungeonLayout[x,y,z])
                    {
                        case Tile.Empty:
                            continue;
                            case Tile.Hallway:
                            {
                                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                cube.transform.parent = this.transform;
                                cube.transform.position = new Vector3(x, y, z) - (Vector3)m_DungeonBounds / 2 + Vector3.one / 2;
                                cube.GetComponent<Renderer>().material = m_HallwayMaterial;
                            }
                            break;
                            case Tile.Room:
                            {
                                ////create room
                                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                cube.transform.parent = this.transform;
                                cube.transform.position = new Vector3(x,y,z) - (Vector3)m_DungeonBounds / 2 + Vector3.one / 2;
                            }
                            break;
                    }
                }
    }

    private void OnDrawGizmos()
    {
        // bounding box
        Gizmos.color = UnityEngine.Color.white;
        Gizmos.DrawWireCube(transform.position, m_DungeonBounds);
        
        // triangles
        if (m_RoomConnections != null)
        {
            Gizmos.color = UnityEngine.Color.cyan;
            foreach (var e in m_RoomConnections)
            {
                Gizmos.DrawLine(e.v1, e.v2);
            }
        }
    }
}
