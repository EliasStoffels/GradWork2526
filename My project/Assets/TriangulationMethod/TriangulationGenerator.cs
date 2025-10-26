// Adapted from https://github.com/vazgriz/DungeonGenerator/blob/master/Assets/Scripts3D/Delaunay3D.cs

﻿/* Adapted from https://github.com/Bl4ckb0ne/delaunay-triangulation

Copyright (c) 2015-2019 Simon Zeni (simonzeni@gmail.com)


Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:


The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.


THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.*/

using System.Collections.Generic;
using UnityEngine;

public class TriangulationGenerator : MonoBehaviour
{
    struct Room
    {
        public Vector3 position;
        public Vector3Int size;
    }

    [SerializeField]
    private GameObject m_RoomPrefab;
    [SerializeField]
    private int m_Seed = 0;
    [SerializeField]
    private Vector3Int m_DungeonBounds = Vector3Int.zero;
    [SerializeField]
    private int m_RoomAmount = 20;
    [SerializeField]
    private int m_RoomAttempts = 10;
    [SerializeField]
    private Vector3Int m_MinRoomSize = Vector3Int.zero;
    [SerializeField]
    private Vector3Int m_MaxRoomSize = Vector3Int.zero;
    [SerializeField]
    private int m_EdgesToKeep = 0;

    private System.Random m_Random;
    private bool[,,] m_DungeonLayout;
    private Room[] m_Rooms;
    private int m_RoomCount;
    private List<Tetrahedron> m_Tetrahedra;
    void Start()
    {
        Generate();
    }

    public void Generate()
    {
        Initialize();
        GenerateRooms();
        TriangulateRooms();
    }

    private void Initialize()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        m_RoomCount = 0;
        m_Rooms = new Room[m_RoomAmount];
        m_Random = new System.Random(m_Seed);
        m_DungeonLayout = new bool[m_DungeonBounds.x,m_DungeonBounds.y,m_DungeonBounds.z];
        
    }

    private void GenerateRooms()
    {
        Vector3Int position = Vector3Int.zero;
        for(int roomIdx = 0;  roomIdx < m_RoomAmount; ++roomIdx)
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
                currentRoom.position = (Vector3)position - (Vector3)m_DungeonBounds / 2 + (Vector3)roomSize / 2;
                currentRoom.size = roomSize;
                m_Rooms[m_RoomCount] = currentRoom;
                ++m_RoomCount;

                //create room
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.parent = this.transform;
                cube.transform.position = currentRoom.position;
                cube.transform.localScale = currentRoom.size;
                return;
            }

            //if(attemptIdx  == m_RoomAttempts - 1)
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
                    if (m_DungeonLayout[x, y, z])
                        return false;
                }

        return true;
    }

    private void RegisterRoom(Vector3Int pos, Vector3Int size)
    {
        for (int x = pos.x; x < (pos.x + size.x); ++x)
            for (int y = pos.y; y < (pos.y + size.y); ++y)
                for (int z = pos.z; z < (pos.z + size.z); ++z)
                    m_DungeonLayout[x, y, z] = true;
    }

    private void TriangulateRooms()
    {
        //find "bounding" tetrahedron (all points reside within it)
        float minX = m_Rooms[0].position.x;
        float minY = m_Rooms[0].position.y;
        float minZ = m_Rooms[0].position.z;
        float maxX = minX;
        float maxY = minY;
        float maxZ = minZ;

        for (int roomIdx = 0; roomIdx < m_RoomCount; ++roomIdx)
        {
            var room = m_Rooms[roomIdx];
            if (room.position.x < minX) minX = room.position.x;
            if (room.position.x > maxX) maxX = room.position.x;
            if (room.position.y < minY) minY = room.position.y;
            if (room.position.y > maxY) maxY = room.position.y;
            if (room.position.z < minZ) minZ = room.position.z;
            if (room.position.z > maxZ) maxZ = room.position.z;
        }

        float dx = maxX - minX;
        float dy = maxY - minY;
        float dz = maxZ - minZ;
        float deltaMax = Mathf.Max(dx, dy, dz) * 2;

        Vector3 p1 = new Vector3(minX - 1, minY - 1, minZ - 1);
        Vector3 p2 = new Vector3(maxX + deltaMax, minY - 1, minZ - 1);
        Vector3 p3 = new Vector3(minX - 1, maxY + deltaMax, minZ - 1);
        Vector3 p4 = new Vector3(minX - 1, minY - 1, maxZ + deltaMax);

        //start list with this "bounding" tetrahedron
        List<Tetrahedron> tetrahedra = new List<Tetrahedron>
        {
            new Tetrahedron(p1, p2, p3, p4)
        };


        for (int roomIdx = 0; roomIdx < m_RoomCount; ++roomIdx)
        {
            var room = m_Rooms[roomIdx];
            List<Tetrahedron> badTetrahedra = new List<Tetrahedron>();

            // if a tetrahedron contains this point it is a "bad" tetrahedron
            foreach (Tetrahedron tetrahedron in tetrahedra)
            {
                if (Vector3.SqrMagnitude(room.position - tetrahedron.CircumCenter) < tetrahedron.CircumRadiusSquared)
                    badTetrahedra.Add(tetrahedron);
            }

            // if a tetrahedron is "bad" create the new triangles to connect to the new room
            List<Triangle> triangles = new List<Triangle>();
            foreach (var badTetrahedron in badTetrahedra)
            {
                foreach (var triangle in badTetrahedron.Triangles)
                {
                    bool shared = false;
                    foreach (var otherBad in badTetrahedra)
                    {
                        if (otherBad == badTetrahedron) continue;
                        if (otherBad.Contains(triangle))
                        {
                            shared = true;
                            break;
                        }
                    }
                    if (!shared)
                        triangles.Add(triangle);
                }
            }

            // remove all bad tetrahedrons
            tetrahedra.RemoveAll(tetrahedron => badTetrahedra.Contains(tetrahedron));

            // add new tetrahedrons that include our new point
            foreach (var tri in triangles)
                tetrahedra.Add(new Tetrahedron(tri.v1, tri.v2, tri.v3, room.position));
        }

        // remove all tetrahedrons conatining the starting points
        tetrahedra.RemoveAll(tetrahedron => tetrahedron.Contains(p1) || tetrahedron.Contains(p2)
                               || tetrahedron.Contains(p3) || tetrahedron.Contains(p4));

        m_Tetrahedra = tetrahedra;
    }

    private void OnDrawGizmos()
    {
        // bounding box
        Gizmos.color = UnityEngine.Color.white;
        Gizmos.DrawWireCube(transform.position, m_DungeonBounds);
        
        // triangles
        if (m_Tetrahedra != null)
        {
            Gizmos.color = UnityEngine.Color.cyan;
            foreach (var t in m_Tetrahedra)
            {
                Gizmos.DrawLine(t.V1, t.V2);
                Gizmos.DrawLine(t.V1, t.V3);
                Gizmos.DrawLine(t.V1, t.V4);
                Gizmos.DrawLine(t.V2, t.V3);
                Gizmos.DrawLine(t.V2, t.V4);
                Gizmos.DrawLine(t.V3, t.V4);
            }
        }
    }
}

public class Tetrahedron
{
    private Vector3 v1;
    private Vector3 v2;
    private Vector3 v3;
    private Vector3 v4;
    private Vector3 circumCenter;
    private float circumRadiusSquared;

    private readonly Triangle[] triangles = new Triangle[4];

    public Vector3 V1 { get { return v1; }}
    public Vector3 V2 { get { return v2; } }
    public Vector3 V3 { get { return v3; } }
    public Vector3 V4 { get { return v4; } }
    public Vector3 CircumCenter { get { return circumCenter; } }
    public float CircumRadiusSquared { get { return circumRadiusSquared; } }
    public Triangle[] Triangles { get { return triangles; } }
    public Tetrahedron(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        v1 = a;
        v2 = b;
        v3 = c;
        v4 = d;
        CalculateCircumsphere();
        triangles[0] = new Triangle(v1, v2, v3);
        triangles[1] = new Triangle(v1, v2, v4);
        triangles[2] = new Triangle(v1, v3, v4);
        triangles[3] = new Triangle(v2, v3, v4);
    }

    public bool Contains(Vector3 point)
    {
        if (Vector3.SqrMagnitude(point - v1) < 0.001f)
            return true;
        if (Vector3.SqrMagnitude(point - v2) < 0.001f)
            return true;
        if (Vector3.SqrMagnitude(point - v3) < 0.001f)
            return true;
        if (Vector3.SqrMagnitude(point - v4) < 0.001f)
            return true;
        return false;
    }

    public bool Contains(Triangle triangle)
    {
        foreach(Triangle t in triangles)
        {
            if (t.AlmostEquals(triangle))
                return true;
        }
        return false;
    }

    void CalculateCircumsphere()
    {
        //calculate the circumsphere of a tetrahedron
        //http://mathworld.wolfram.com/Circumsphere.html

        float a = new Matrix4x4(
            new Vector4(v1.x, v2.x, v3.x, v4.x),
            new Vector4(v1.y, v2.y, v3.y, v4.y),
            new Vector4(v1.z, v2.z, v3.z, v4.z),
            new Vector4(1, 1, 1, 1)
        ).determinant;

        float v1PosSqr = v1.sqrMagnitude;
        float v2PosSqr = v2.sqrMagnitude;
        float v3PosSqr = v3.sqrMagnitude;
        float v4PosSqr = v4.sqrMagnitude;

        float Dx = new Matrix4x4(
            new Vector4(v1PosSqr, v2PosSqr, v3PosSqr, v4PosSqr),
            new Vector4(v1.y, v2.y, v3.y, v4.y),
            new Vector4(v1.z, v2.z, v3.z, v4.z),
            new Vector4(1, 1, 1, 1)
        ).determinant;

        float Dy = -(new Matrix4x4(
            new Vector4(v1PosSqr, v2PosSqr, v3PosSqr, v4PosSqr),
            new Vector4(v1.x, v2.x, v3.x, v4.x),
            new Vector4(v1.z, v2.z, v3.z, v4.z),
            new Vector4(1, 1, 1, 1)
        ).determinant);

        float Dz = new Matrix4x4(
            new Vector4(v1PosSqr, v2PosSqr, v3PosSqr, v4PosSqr),
            new Vector4(v1.x, v2.x, v3.x, v4.x),
            new Vector4(v1.y, v2.y, v3.y, v4.y),
            new Vector4(1, 1, 1, 1)
        ).determinant;

        float c = new Matrix4x4(
            new Vector4(v1PosSqr, v2PosSqr, v3PosSqr, v4PosSqr),
            new Vector4(v1.x, v2.x, v3.x, v4.x),
            new Vector4(v1.y, v2.y, v3.y, v4.y),
            new Vector4(v1.z, v2.z, v3.z, v4.z)
        ).determinant;

        circumCenter = new Vector3(
            Dx / (2 * a),
            Dy / (2 * a),
            Dz / (2 * a)
        );

        circumRadiusSquared = ((Dx * Dx) + (Dy * Dy) + (Dz * Dz) - (4 * a * c)) / (4 * a * a);
    }
}

public class Triangle
{
    public Vector3 v1;
    public Vector3 v2;
    public Vector3 v3;

    public Triangle(Vector3 u, Vector3 v, Vector3 w)
    {
        v1 = u;
        v2 = v;
        v3 = w;
    }

    public bool AlmostEquals(Triangle triangle)
    {
        int matches = 0;
        if ((v1 - triangle.v1).sqrMagnitude < 0.001f ||
            (v1 - triangle.v2).sqrMagnitude < 0.001f ||
            (v1 - triangle.v3).sqrMagnitude < 0.001f) matches++;
        if ((v2 - triangle.v1).sqrMagnitude < 0.001f ||
            (v2 - triangle.v2).sqrMagnitude < 0.001f ||
            (v2 - triangle.v3).sqrMagnitude < 0.001f) matches++;
        if ((v3 - triangle.v1).sqrMagnitude < 0.001f ||
            (v3 - triangle.v2).sqrMagnitude < 0.001f ||
            (v3 - triangle.v3).sqrMagnitude < 0.001f) matches++;

        return matches >= 3;
    }
}