using System.Collections.Generic;
using NUnit.Framework;
using Src.scripts;
using UnityEngine;

public class PathfinderTest
{
    [Test]
    public void FindPath_WhenAllMovesBlocked_ReturnsNull()
    {
        Pathfinder pathfinder = new Pathfinder(new Vector2Int(4, 4));
        Vector2Int start = new Vector2Int(0, 0);
        Vector2Int end = new Vector2Int(3, 3);

        List<Vector2Int> path = pathfinder.FindPath(start, end, (_, __) => new Pathfinder.PathCost
        {
            Traversable = false,
            Cost = 1f
        });

        Assert.IsNull(path);
    }

    [Test]
    public void FindPath_OnUniformCostGrid_ReturnsContiguousPathStartingAndEndingCorrectly()
    {
        Pathfinder pathfinder = new Pathfinder(new Vector2Int(6, 6));
        Vector2Int start = new Vector2Int(1, 1);
        Vector2Int end = new Vector2Int(4, 3);

        List<Vector2Int> path = pathfinder.FindPath(start, end, (_, __) => new Pathfinder.PathCost
        {
            Traversable = true,
            Cost = 1f
        });

        Assert.IsNotNull(path);
        Assert.IsNotEmpty(path);
        Assert.AreEqual(start, path[0]);
        Assert.AreEqual(end, path[^1]);

        for (int i = 1; i < path.Count; i++)
        {
            Vector2Int delta = path[i] - path[i - 1];
            int manhattanStep = Mathf.Abs(delta.x) + Mathf.Abs(delta.y);
            Assert.AreEqual(1, manhattanStep, $"Non-contiguous step at index {i - 1}->{i}: {path[i - 1]} -> {path[i]}");
        }
    }
    
    [Test]
    public void FindPath_WithWeightedNodes_TakesCheaperLongerRoute()
    {
        Pathfinder pathfinder = new Pathfinder(new Vector2Int(3, 3));
        Vector2Int start = new Vector2Int(0, 0);
        Vector2Int end = new Vector2Int(0, 2);

        List<Vector2Int> path = pathfinder.FindPath(start, end, (pos, _) => {
            bool isExpensive = (pos.Position == new Vector2Int(0, 1));
            return new Pathfinder.PathCost { 
                Traversable = true, 
                Cost = isExpensive ? 10f : 1f 
            };
        });

        Assert.IsFalse(path.Contains(new Vector2Int(0, 1)), "Path should have avoided the expensive tile.");
        Assert.IsTrue(path.Count > 3, "Path should be longer but cheaper.");
    }

    [Test]
    public void FindPath_WhenStartEqualsEnd_ReturnsPathWithSingleNode()
    {
        Pathfinder pathfinder = new Pathfinder(new Vector2Int(5, 5));
        Vector2Int point = new Vector2Int(2, 2);

        List<Vector2Int> path = pathfinder.FindPath(point, point, (_, __) => 
            new Pathfinder.PathCost { Traversable = true, Cost = 1f });

        Assert.AreEqual(1, path.Count);
        Assert.AreEqual(point, path[0]);
    }
}