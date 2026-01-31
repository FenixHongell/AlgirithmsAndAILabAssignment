using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public static class GraphManager
{
    public static List<(Vector3 a, Vector3 b, Vector3 c)> CreateGraph(List<Room> rooms, int y)
    {
        var superTriangle = GetSuperTriangle(rooms, y);
        List<(Vector3 a, Vector3 b, Vector3 c)> triangles = new List<(Vector3 a, Vector3 b, Vector3 c)>();
        triangles.Add(superTriangle);

        foreach (var room in rooms)
        {
            List<(Vector3 a, Vector3 b, Vector3 c)> badTriangles = new List<(Vector3 a, Vector3 b, Vector3 c)>();
            foreach (var triangle in triangles)
            {
                if (inCircle(triangle, room.Position))
                    badTriangles.Add(triangle);
            }

            var edgeCount = new Dictionary<Edge, int>();

            foreach (var t in badTriangles)
            {
                AddEdge(edgeCount, t.a, t.b);
                AddEdge(edgeCount, t.b, t.c);
                AddEdge(edgeCount, t.c, t.a);
            }

            var boundaryEdges = edgeCount.Where(kv => kv.Value == 1).Select(kv => kv.Key).ToList();

            triangles.RemoveAll(triangle => badTriangles.Contains(triangle));

            foreach (var boundaryEdge in boundaryEdges)
            {
                Vector3 a = boundaryEdge.A;
                Vector3 b = boundaryEdge.B;
                Vector3 c = room.Position;

                if (OrientXZ(a, b, c) < 0f)
                {
                    (a, b) = (b, a);
                }

                triangles.Add((a, b, c));
            }
        }

        triangles.RemoveAll(triangle => triangle.a == superTriangle.a || triangle.b == superTriangle.a ||
                                        triangle.c == superTriangle.a ||
                                        triangle.a == superTriangle.b || triangle.b == superTriangle.b ||
                                        triangle.c == superTriangle.b ||
                                        triangle.a == superTriangle.c || triangle.b == superTriangle.c ||
                                        triangle.c == superTriangle.c);

        return triangles;
    }

    private static void AddEdge(Dictionary<Edge, int> edgeCount, Vector3 a, Vector3 b)
    {
        var e = new Edge(a, b);
        if (edgeCount.TryGetValue(e, out int count)) edgeCount[e] = count + 1;
        else edgeCount[e] = 1;
    }

    private static float OrientXZ(Vector3 a, Vector3 b, Vector3 c)
    {
        float abx = b.x - a.x;
        float abz = b.z - a.z;
        float acx = c.x - a.x;
        float acz = c.z - a.z;
        return abx * acz - abz * acx;
    }

    private static (Vector3 a, Vector3 b, Vector3 c) GetSuperTriangle(List<Room> rooms, int y)
    {
        if (rooms.Count == 0)
            throw new InvalidOperationException("No rooms generated yet.");

        const float scale = 10f; // Just here in case I need to add a margin.

        int minX = rooms[0].Position.x, maxX = rooms[0].Position.x;
        int minZ = rooms[0].Position.z, maxZ = rooms[0].Position.z;

        for (int i = 1; i < rooms.Count; i++)
        {
            var p = rooms[i].Position;
            minX = Mathf.Min(minX, p.x);
            maxX = Mathf.Max(maxX, p.x);
            minZ = Mathf.Min(minZ, p.z);
            maxZ = Mathf.Max(maxZ, p.z);
        }

        float cx = (minX + maxX) / 2f;
        float cz = (minZ + maxZ) / 2f;

        float dx = maxX - minX;
        float dz = maxZ - minZ;
        float d = Mathf.Max(dx, dz);

        float r = (d <= 0f ? 1f : d) * scale;

        Vector3 a = new Vector3(cx - 2f * r, y, cz - r);
        Vector3 b = new Vector3(cx, y, cz + 2f * r);
        Vector3 c = new Vector3(cx + 2f * r, y, cz - r);

        return (a, b, c);
    }

    private static bool inCircle((Vector3 a, Vector3 b, Vector3 c) triangle, Vector3 p)
    {
        Vector2 A = new Vector2(triangle.a.x, triangle.a.z);
        Vector2 B = new Vector2(triangle.b.x, triangle.b.z);
        Vector2 C = new Vector2(triangle.c.x, triangle.c.z);
        Vector2 P = new Vector2(p.x, p.z);

        double orient = Orient2D(A, B, C);

        double ax = A.x - P.x;
        double ay = A.y - P.y;
        double bx = B.x - P.x;
        double by = B.y - P.y;
        double cx = C.x - P.x;
        double cy = C.y - P.y;

        double a2 = ax * ax + ay * ay;
        double b2 = bx * bx + by * by;
        double c2 = cx * cx + cy * cy;


        double det =
            ax * (by * c2 - b2 * cy)
            - ay * (bx * c2 - b2 * cx)
            + a2 * (bx * cy - by * cx);

        const double eps = 0.0;

        if (orient > 0.0) return det > eps;
        if (orient < 0.0) return det < -eps;

        return false;
    }

    private static double Orient2D(Vector2 a, Vector2 b, Vector2 c)
    {
        return (double)(b.x - a.x) * (c.y - a.y) - (double)(b.y - a.y) * (c.x - a.x);
    }
}