using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public static class GraphManager
{
    private struct EdgeKey : IEquatable<EdgeKey>
    {
        public readonly int A;
        public readonly int B;

        public EdgeKey(int u, int v)
        {
            if (u < v)
            {
                A = u;
                B = v;
            }
            else
            {
                A = v;
                B = u;
            }
        }

        public bool Equals(EdgeKey other) => A == other.A && B == other.B;
        public override bool Equals(object obj) => obj is EdgeKey other && Equals(other);
        public override int GetHashCode() => unchecked((A * 397) ^ B);
    }

    public static List<Triangle> CreateGraph(List<Room> rooms, int y)
    {
        var superTriangle = GetSuperTriangle(rooms, y);
        List<Triangle> triangles = new List<Triangle>();
        triangles.Add(superTriangle);

        foreach (var room in rooms)
        {
            List<Triangle> badTriangles = new List<Triangle>();
            foreach (var triangle in triangles)
            {
                if (InCircle(triangle, room.Position))
                    badTriangles.Add(triangle);
            }

            var edgeCount = new Dictionary<Edge, int>();

            foreach (var t in badTriangles)
            {
                AddEdge(edgeCount, t.A, t.B);
                AddEdge(edgeCount, t.B, t.C);
                AddEdge(edgeCount, t.C, t.A);
            }

            var boundaryEdges = edgeCount.Where(kv => kv.Value == 1).Select(kv => kv.Key).ToList();

            triangles.RemoveAll(triangle => badTriangles.Contains(triangle));

            foreach (var boundaryEdge in boundaryEdges)
            {
                Vector3 a = boundaryEdge.A;
                Vector3 b = boundaryEdge.B;
                Vector3 c = room.Position;

                if (OrientXZ(new Triangle(a, b, c)) < 0f)
                {
                    (a, b) = (b, a);
                }

                triangles.Add(new Triangle(a, b, c));
            }
        }

        triangles.RemoveAll(triangle => triangle.A == superTriangle.A || triangle.B == superTriangle.A ||
                                        triangle.C == superTriangle.A ||
                                        triangle.A == superTriangle.B || triangle.B == superTriangle.B ||
                                        triangle.C == superTriangle.B ||
                                        triangle.A == superTriangle.C || triangle.B == superTriangle.C ||
                                        triangle.C == superTriangle.C);

        return triangles;
    }

    private static void AddEdge(Dictionary<Edge, int> edgeCount, Vector3 a, Vector3 b)
    {
        var e = new Edge(a, b);
        if (edgeCount.TryGetValue(e, out int count)) edgeCount[e] = count + 1;
        else edgeCount[e] = 1;
    }

    private static float OrientXZ(Triangle triangle)
    {
        float abx = triangle.B.x - triangle.A.x;
        float abz = triangle.B.z - triangle.A.z;
        float acx = triangle.C.x - triangle.A.x;
        float acz = triangle.C.z - triangle.A.z;
        return abx * acz - abz * acx;
    }

    private static Triangle GetSuperTriangle(List<Room> rooms, int y)
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

        return new Triangle(a, b, c);
    }

    private static bool InCircle(Triangle triangle, Vector3 p)
    {
        Vector2 A = new Vector2(triangle.A.x, triangle.A.z);
        Vector2 B = new Vector2(triangle.B.x, triangle.B.z);
        Vector2 C = new Vector2(triangle.C.x, triangle.C.z);
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

    public static List<Edge> GetMST(List<Triangle> triangles)
    {
        var mst = new List<Edge>();
        if (triangles == null || triangles.Count == 0)
            return mst;

        var points = new List<Vector2>();
        var pointIndex = new Dictionary<Vector2, int>();
        var uniqueEdgeKeys = new HashSet<EdgeKey>();
        var candidateEdges = new List<(int u, int v, float w)>();

        int GetOrAddPoint(Vector2 p)
        {
            if (!pointIndex.TryGetValue(p, out var idx))
            {
                idx = points.Count;
                points.Add(p);
                pointIndex.Add(p, idx);
            }

            return idx;
        }

        void AddEdge(Vector2 a, Vector2 b)
        {
            int u = GetOrAddPoint(a);
            int v = GetOrAddPoint(b);
            if (u == v) return;

            var key = new EdgeKey(u, v);
            if (uniqueEdgeKeys.Add(key))
            {
                float w = (points[u] - points[v]).sqrMagnitude;
                candidateEdges.Add((u, v, w));
            }
        }

        foreach (var t in triangles)
        {
            var a = t.A;
            var b = t.B;
            var c = t.C;

            AddEdge(a, b);
            AddEdge(b, c);
            AddEdge(c, a);
        }

        int n = points.Count;
        if (n == 0) return mst;

        var adj = new List<(int to, float w)>[n];
        for (int i = 0; i < n; i++) adj[i] = new List<(int to, float w)>();

        foreach (var e in candidateEdges)
        {
            adj[e.u].Add((e.v, e.w));
            adj[e.v].Add((e.u, e.w));
        }

        var inMST = new bool[n];
        int visitedCount = 0;

        int tie = 0;
        var pq = new SortedSet<(float w, int from, int to, int tie)>(
            Comparer<(float w, int from, int to, int tie)>.Create((x, y) =>
            {
                int c = x.w.CompareTo(y.w);
                if (c != 0) return c;
                c = x.from.CompareTo(y.from);
                if (c != 0) return c;
                c = x.to.CompareTo(y.to);
                if (c != 0) return c;
                return x.tie.CompareTo(y.tie);
            })
        );

        void PushNode(int u)
        {
            inMST[u] = true;
            visitedCount++;
            foreach (var (to, w) in adj[u])
            {
                if (!inMST[to])
                    pq.Add((w, u, to, tie++));
            }
        }

        PushNode(0);

        while (visitedCount < n && pq.Count > 0)
        {
            var top = pq.Min;
            pq.Remove(top);

            int from = top.from;
            int to = top.to;

            if (inMST[to]) continue;

            mst.Add(new Edge(points[from], points[to]));

            PushNode(to);
        }

        return mst;
    }
}