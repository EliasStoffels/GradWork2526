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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimumSpanningTree
{
    public static List<Edge> GenerateMSP(List<Edge> edges, Vector3 start)
    {
        HashSet<Vector3> openSet = new HashSet<Vector3>();
        HashSet<Vector3> closedSet = new HashSet<Vector3>();

        foreach (var edge in edges)
        {
            openSet.Add(edge.v1);
            openSet.Add(edge.v2);
        }

        closedSet.Add(start);

        List<Edge> results = new List<Edge>();

        while (openSet.Count > 0)
        {
            bool chosen = false;
            Edge chosenEdge = null;
            float minWeight = float.PositiveInfinity;

            foreach (var edge in edges)
            {
                int closedVertices = 0;
                if (!closedSet.Contains(edge.v1)) closedVertices++;
                if (!closedSet.Contains(edge.v2)) closedVertices++;
                if (closedVertices != 1) continue;

                if (edge.distance < minWeight)
                {
                    chosenEdge = edge;
                    chosen = true;
                    minWeight = edge.distance;
                }
            }

            if (!chosen) break;
            results.Add(chosenEdge);
            openSet.Remove(chosenEdge.v1);
            openSet.Remove(chosenEdge.v2);
            closedSet.Add(chosenEdge.v1);
            closedSet.Add(chosenEdge.v2);
        }

        return results;
    }
    public static IEnumerator GenerateMSPCo(List<Edge> edges, Vector3 start, System.Action<List<Edge>> onStepComplete)
    {
        HashSet<Vector3> openSet = new HashSet<Vector3>();
        HashSet<Vector3> closedSet = new HashSet<Vector3>();

        foreach (var edge in edges)
        {
            openSet.Add(edge.v1);
            openSet.Add(edge.v2);
        }

        closedSet.Add(start);

        List<Edge> results = new List<Edge>();

        while (openSet.Count > 0)
        {
            bool chosen = false;
            Edge chosenEdge = null;
            float minWeight = float.PositiveInfinity;

            foreach (var edge in edges)
            {
                int closedVertices = 0;
                if (!closedSet.Contains(edge.v1)) closedVertices++;
                if (!closedSet.Contains(edge.v2)) closedVertices++;
                if (closedVertices != 1) continue;

                if (edge.distance < minWeight)
                {
                    chosenEdge = edge;
                    chosen = true;
                    minWeight = edge.distance;
                }
            }

            if (!chosen) break;
            results.Add(chosenEdge);
            openSet.Remove(chosenEdge.v1);
            openSet.Remove(chosenEdge.v2);
            closedSet.Add(chosenEdge.v1);
            closedSet.Add(chosenEdge.v2);

            onStepComplete.Invoke(results);

            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
        }

        onStepComplete.Invoke(results);
    }
}
