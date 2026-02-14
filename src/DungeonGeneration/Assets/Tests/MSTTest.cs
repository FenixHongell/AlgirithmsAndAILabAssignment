using System.Collections.Generic;
using System.Linq;
using Graphs;
using NUnit.Framework;
using Src.scripts;
using UnityEngine;

public class MSTTest
{
    [Test]
    public void GetMST_WithDisconnectedGraph_ReturnsPartialTree_AndDoesNotThrow()
    {
        var a = new Vertex(new Vector2(0f, 0f));
        var b = new Vertex(new Vector2(1f, 0f));
        var c = new Vertex(new Vector2(10f, 0f));
        var d = new Vertex(new Vector2(11f, 0f));

        var edges = new List<PrimsAlgorithm.Edge>
        {
            new PrimsAlgorithm.Edge(a, b),
            new PrimsAlgorithm.Edge(c, d),
        };

        List<PrimsAlgorithm.Edge> mst = PrimsAlgorithm.GetMST(edges, a);

        Assert.NotNull(mst);
        Assert.AreEqual(1, mst.Count, "Disconnected graph should produce a partial spanning tree for the reachable component.");

        var only = mst[0];
        Assert.True((only.A.Equals(a) && only.B.Equals(b)) || (only.A.Equals(b) && only.B.Equals(a)));
    }
}