using System;
using System.Collections.Generic;
using BlueRaja;
using UnityEngine;

namespace Src.scripts
{
    public class Pathfinder
    {
        public class Node
        {
            public readonly Vector2Int Position;
            public Node Previous;
            public float Cost;
            
            public Node(Vector2Int position)
            {
                Position = position;
            }
        }

        public struct PathCost
        {
            public bool Traversable;
            public float Cost;
        }

        private static readonly Vector2Int[] Neighbors =
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
        };
        
        private Grid<Node> _grid;
        private SimplePriorityQueue<Node, float> _openSet;
        private HashSet<Node> _visited;
        private Stack<Vector2Int> _stack;

        public Pathfinder(Vector2Int size)
        {
            _grid = new Grid<Node>(size, Vector2Int.zero);
            _openSet = new SimplePriorityQueue<Node, float>();
            _visited = new HashSet<Node>();
            _stack = new Stack<Vector2Int>();
            
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    _grid[x, y] = new Node(new Vector2Int(x, y));
                }
            }
        }

        void Reset()
        {
            Vector2Int size = _grid.Size;

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Node node = _grid[x, y];
                    node.Previous = null;
                    node.Cost = float.PositiveInfinity;
                }
            }
        }

        /// Finds the shortest path between two points within a grid using a specified heuristic function.
        /// <param name="start">The starting position of the path as a Vector2Int.</param>
        /// <param name="end">The ending position of the path as a Vector2Int.</param>
        /// <param name="heuristic">A function defining the heuristic to calculate the traversal cost between two nodes.</param>
        /// <returns>
        /// A list of Vector2Int representing the path from the start to the end position.
        /// Returns null if no valid path exists.
        /// </returns>
        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end, Func<Node, Node, PathCost> heuristic)
        {
            Reset();
            _openSet.Clear();
            _visited.Clear();
            
            _openSet = new SimplePriorityQueue<Node, float>();
            _visited = new HashSet<Node>();
            
            _grid[start].Cost = 0;
            _openSet.Enqueue(_grid[start], 0);
            
            while (_openSet.Count > 0)
            {
                Node current = _openSet.Dequeue();
                _visited.Add(current);

                if (current.Position == end)
                {
                    return ConstructPath(current);
                }

                foreach (Vector2Int offset in Neighbors)
                {
                    if (!_grid.InBounds(current.Position + offset)) continue;
                    Node neighbor = _grid[current.Position + offset];
                    if (_visited.Contains(neighbor)) continue;
                    
                    PathCost pathCost = heuristic(current, neighbor);
                    if (!pathCost.Traversable) continue;
                    
                    float score = current.Cost + pathCost.Cost;

                    if (score < neighbor.Cost)
                    {
                        neighbor.Cost = score;
                        neighbor.Previous = current;

                        if (_openSet.TryGetPriority(current, out float priority))
                        {
                            _openSet.UpdatePriority(neighbor, score);
                        }
                        else
                        {
                            _openSet.Enqueue(neighbor, neighbor.Cost);
                        }
                    }
                }
            }

            return null;
        }

        private List<Vector2Int> ConstructPath(Node node)
        {
            List<Vector2Int> path = new List<Vector2Int>();

            while (node != null)
            {
                _stack.Push(node.Position);
                node = node.Previous;
            }

            while (_stack.Count > 0)
            {
                path.Add(_stack.Pop());
            }
            
            return path;
        }
    }
}