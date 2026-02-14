using System.Collections.Generic;
using Graphs;
using NUnit.Framework;
using Src.scripts;
using UnityEngine;

public class DelauneyTest
{
    [Test]
    public void Create_WithSquareAndCenter_ProducesDelaunayTriangles_AndNonDegenerateUniqueEdges()
    {
        var vertices = new List<Vertex>
        {
            new Vertex(new Vector2(0f, 0f)),
            new Vertex(new Vector2(1f, 0f)),
            new Vertex(new Vector2(1f, 1f)),
            new Vertex(new Vector2(0f, 1f)),
            new Vertex(new Vector2(0.5f, 0.5f)),
        };

        Delaunay d = Delaunay.Create(vertices);

        Assert.NotNull(d);
        Assert.NotNull(d.Triangles);
        Assert.NotNull(d.Edges);

        Assert.Greater(d.Triangles.Count, 0, "Expected at least one triangle.");
        Assert.Greater(d.Edges.Count, 0, "Expected at least one edge.");

        const float eps = 1e-5f;

        foreach (var t in d.Triangles)
        {
            Vector2 a = new Vector2(t.A.Position.x, t.A.Position.y);
            Vector2 b = new Vector2(t.B.Position.x, t.B.Position.y);
            Vector2 c = new Vector2(t.C.Position.x, t.C.Position.y);

            float twiceArea = Cross(b - a, c - a);
            Assert.Greater(Mathf.Abs(twiceArea), eps, "Found degenerate (zero-area) triangle.");

            foreach (var v in vertices)
            {
                if (ReferenceEquals(v, t.A) || ReferenceEquals(v, t.B) || ReferenceEquals(v, t.C))
                    continue;

                Assert.False(t.InCircle(v.Position),
                    "Found a vertex inside a triangle circumcircle; violates Delaunay condition.");
            }
        }

        for (int i = 0; i < d.Edges.Count; i++)
        {
            for (int j = i + 1; j < d.Edges.Count; j++)
            {
                Assert.False(d.Edges[i].Equals(d.Edges[j]), "Duplicate undirected edge found in Edges list.");
            }
        }
    }

    private static float Cross(Vector2 u, Vector2 v) => u.x * v.y - u.y * v.x;
}