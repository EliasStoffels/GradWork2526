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

public class TriangulationMethods
{
    public static List<Tetrahedron> Triangulate(Vector3[] vertices)
    {
        //find "bounding" tetrahedron (all points reside within it)
        float minX = vertices[0].x;
        float minY = vertices[0].y;
        float minZ = vertices[0].z;
        float maxX = minX;
        float maxY = minY;
        float maxZ = minZ;

        foreach (var vertice in vertices)
        {
            if (vertice.x < minX) minX = vertice.x;
            if (vertice.x > maxX) maxX = vertice.x;
            if (vertice.y < minY) minY = vertice.y;
            if (vertice.y > maxY) maxY = vertice.y;
            if (vertice.z < minZ) minZ = vertice.z;
            if (vertice.z > maxZ) maxZ = vertice.z;
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


        foreach (var vertice in vertices)
        {
            List<Tetrahedron> badTetrahedra = new List<Tetrahedron>();

            // if a tetrahedron contains this point it is a "bad" tetrahedron
            foreach (Tetrahedron tetrahedron in tetrahedra)
            {
                if (float.IsInfinity(tetrahedron.CircumRadiusSquared) ||
                    float.IsNaN(tetrahedron.CircumCenter.x))
                    continue; // Skip degenerate

                if (Vector3.SqrMagnitude(vertice - tetrahedron.CircumCenter) < tetrahedron.CircumRadiusSquared)
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

            // add new tetrahedrons that include new point
            foreach (var tri in triangles)
                tetrahedra.Add(new Tetrahedron(tri.v1, tri.v2, tri.v3, vertice));
        }

        // remove all tetrahedrons conatining the starting points
        tetrahedra.RemoveAll(tetrahedron => tetrahedron.Contains(p1) || tetrahedron.Contains(p2)
                               || tetrahedron.Contains(p3) || tetrahedron.Contains(p4));

        return tetrahedra;
    }
    public static IEnumerator TriangulateCo(Vector3[] vertices, System.Action<List<Tetrahedron>> onStepComplete)
    {
        //find "bounding" tetrahedron (all points reside within it)
        float minX = vertices[0].x;
        float minY = vertices[0].y;
        float minZ = vertices[0].z;
        float maxX = minX;
        float maxY = minY;
        float maxZ = minZ;

        foreach (var vertice in vertices)
        {
            if (vertice.x < minX) minX = vertice.x;
            if (vertice.x > maxX) maxX = vertice.x;
            if (vertice.y < minY) minY = vertice.y;
            if (vertice.y > maxY) maxY = vertice.y;
            if (vertice.z < minZ) minZ = vertice.z;
            if (vertice.z > maxZ) maxZ = vertice.z;
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


        foreach (var vertice in vertices)
        {
            List<Tetrahedron> badTetrahedra = new List<Tetrahedron>();

            // if a tetrahedron contains this point it is a "bad" tetrahedron
            foreach (Tetrahedron tetrahedron in tetrahedra)
            {
                if (float.IsInfinity(tetrahedron.CircumRadiusSquared) ||
                    float.IsNaN(tetrahedron.CircumCenter.x))
                    continue; // Skip degenerate

                if (Vector3.SqrMagnitude(vertice - tetrahedron.CircumCenter) < tetrahedron.CircumRadiusSquared)
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

            // add new tetrahedrons that include new point
            foreach (var tri in triangles)
                tetrahedra.Add(new Tetrahedron(tri.v1, tri.v2, tri.v3, vertice));

            onStepComplete?.Invoke(new List<Tetrahedron>(tetrahedra));
            Debug.Log("Triangulation step");
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
        }

        // remove all tetrahedrons conatining the starting points
        tetrahedra.RemoveAll(tetrahedron => tetrahedron.Contains(p1) || tetrahedron.Contains(p2)
                               || tetrahedron.Contains(p3) || tetrahedron.Contains(p4));

        onStepComplete?.Invoke(tetrahedra);
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

    public Vector3 V1 { get { return v1; } }
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
        if (Vector3.SqrMagnitude(point - v1) < 0.01f)
            return true;
        if (Vector3.SqrMagnitude(point - v2) < 0.01f)
            return true;
        if (Vector3.SqrMagnitude(point - v3) < 0.01f)
            return true;
        if (Vector3.SqrMagnitude(point - v4) < 0.01f)
            return true;
        return false;
    }

    public bool Contains(Triangle triangle)
    {
        foreach (Triangle t in triangles)
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

        if (Mathf.Abs(a) < 1e-10f)
        {
            circumCenter = Vector3.positiveInfinity;
            circumRadiusSquared = float.PositiveInfinity;
            return;
        }

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
        if (float.IsNaN(circumCenter.x) || float.IsNaN(circumCenter.y) || float.IsNaN(circumCenter.z))
            Debug.LogWarning("Degenerate tetrahedron detected: " + v1 + ", " + v2 + ", " + v3 + ", " + v4);

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
        if ((v1 - triangle.v1).sqrMagnitude < 0.01f ||
            (v1 - triangle.v2).sqrMagnitude < 0.01f ||
            (v1 - triangle.v3).sqrMagnitude < 0.01f) matches++;
        if ((v2 - triangle.v1).sqrMagnitude < 0.01f ||
            (v2 - triangle.v2).sqrMagnitude < 0.01f ||
            (v2 - triangle.v3).sqrMagnitude < 0.01f) matches++;
        if ((v3 - triangle.v1).sqrMagnitude < 0.01f ||
            (v3 - triangle.v2).sqrMagnitude < 0.01f ||
            (v3 - triangle.v3).sqrMagnitude < 0.01f) matches++;

        return matches >= 3;
    }
}
