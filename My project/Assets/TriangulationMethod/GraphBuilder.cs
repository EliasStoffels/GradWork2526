// Adapted from https://github.com/vazgriz/DungeonGenerator/blob/master/Assets/Scripts3D/Delaunay3D.cs

/* Adapted from https://github.com/Bl4ckb0ne/delaunay-triangulation

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

public class GraphBuilder
{
    public static List<Edge> GenerateEdges(List<Tetrahedron> tetrahedra)
    {
        HashSet<Edge> edgeSet = new HashSet<Edge>();
        foreach (var t in tetrahedra)
        {
            void AddEdge(Vector3 a, Vector3 b)
            {
                // consistent order so no duplicates
                Edge edge = a.sqrMagnitude < b.sqrMagnitude ? new Edge(a, b) : new Edge(b, a);
                edgeSet.Add(edge);
            }

            AddEdge(t.V1, t.V2);
            AddEdge(t.V1, t.V3);
            AddEdge(t.V1, t.V4);
            AddEdge(t.V2, t.V3);
            AddEdge(t.V2, t.V4);
            AddEdge(t.V3, t.V4);
        }

        List<Edge> edges = new List<Edge>();
        foreach (var e in edgeSet)
            edges.Add(e);

        return edges;
    }
}

public class Edge
{
    public Edge(Vector3 vector1, Vector3 vector2)
    {
        v1 = vector1;
        v2 = vector2;
        distance = Vector3.Distance(v1, v2);
    }
    public Vector3 v1;
    public Vector3 v2;
    public float distance;
}