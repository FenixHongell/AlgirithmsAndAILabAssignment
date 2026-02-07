using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEngine;

public class GraphTestScript
{
    private const float Epsilon = 1e-3f;

    [Test]
    public void CreateGraph_PredefinedRooms_MatchesExpectedTriangles_AnyOrder()
    {
        var predefinedRooms = new List<Room>
        {
            new Room { Position = new Vector3Int(0, 0, 0) },
            new Room { Position = new Vector3Int(10, 0, 0) },
            new Room { Position = new Vector3Int(10, 0, 8) },
            new Room { Position = new Vector3Int(0, 0, 6) },
            new Room { Position = new Vector3Int(4, 0, 3) }
        };

        var triangles = GraphManager.CreateGraph(predefinedRooms, 0);

        var expectedGraph = new List<Triangle>
        {
            new Triangle(new Vector3(0, 0, 0), new Vector3(10, 0, 0), new Vector3(4, 0, 3)),
            new Triangle(new Vector3(10, 0, 0), new Vector3(10, 0, 8), new Vector3(4, 0, 3)),
            new Triangle(new Vector3(10, 0, 8), new Vector3(0, 0, 6), new Vector3(4, 0, 3)),
            new Triangle(new Vector3(0, 0, 6), new Vector3(0, 0, 0), new Vector3(4, 0, 3)),
        };

        var expectedSet = ToTriangleKeySet(expectedGraph, Epsilon, out var expectedMap);
        var actualSet   = ToTriangleKeySet(triangles,      Epsilon, out var actualMap);

        var missing   = expectedSet.Except(actualSet).ToList();
        var unexpected = actualSet.Except(expectedSet).ToList();

        if (missing.Count == 0 && unexpected.Count == 0)
        {
            Assert.Pass("Triangle sets match (order-agnostic). Total: " + expectedSet.Count);
        }
        else
        {
            var sb = new StringBuilder();
            sb.AppendLine("Triangle mismatch (order-agnostic comparison):");
            sb.AppendLine($"  Expected count: {expectedSet.Count}, Actual count: {actualSet.Count}");
            if (missing.Count > 0)
            {
                sb.AppendLine($"  Missing in actual ({missing.Count}):");
                foreach (var k in missing)
                    sb.AppendLine("    - " + expectedMap[k]);
            }
            if (unexpected.Count > 0)
            {
                sb.AppendLine($"  Unexpected in actual ({unexpected.Count}):");
                foreach (var k in unexpected)
                    sb.AppendLine("    - " + actualMap[k]);
            }

            Assert.Fail(sb.ToString());
        }
    }

    // Helpers

    private static HashSet<string> ToTriangleKeySet(
        IEnumerable<Triangle> tris,
        float eps,
        out Dictionary<string, string> prettyMap)
    {
        var set = new HashSet<string>();
        prettyMap = new Dictionary<string, string>();

        foreach (var t in tris)
        {
            var key = TriangleKey(t.A, t.B, t.C, eps);
            if (!prettyMap.ContainsKey(key))
                prettyMap[key] = PrettyTriangle(t.A, t.B, t.C);
            set.Add(key);
        }

        return set;
    }

    private static string TriangleKey(Vector3 a, Vector3 b, Vector3 c, float eps)
    {
        var k1 = VectorKey(a, eps);
        var k2 = VectorKey(b, eps);
        var k3 = VectorKey(c, eps);

        var arr = new[] { k1, k2, k3 };
        System.Array.Sort(arr, System.StringComparer.Ordinal);
        return string.Join("|", arr);
    }

    private static string VectorKey(Vector3 v, float eps)
    {
        int qx = Mathf.RoundToInt(v.x / eps);
        int qy = Mathf.RoundToInt(v.y / eps);
        int qz = Mathf.RoundToInt(v.z / eps);
        return $"{qx},{qy},{qz}";
    }

    private static string PrettyTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        var arr = new[] { a, b, c };
        System.Array.Sort(arr, LexCompare);
        return $"{Pretty(arr[0])}  -  {Pretty(arr[1])}  -  {Pretty(arr[2])}";
    }

    private static string Pretty(Vector3 v) => $"({v.x:0.###}, {v.y:0.###}, {v.z:0.###})";

    private static int LexCompare(Vector3 p, Vector3 q)
    {
        int cx = p.x.CompareTo(q.x);
        if (cx != 0) return cx;
        int cy = p.y.CompareTo(q.y);
        if (cy != 0) return cy;
        return p.z.CompareTo(q.z);
    }
}