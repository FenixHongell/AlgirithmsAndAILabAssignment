using System.Collections.Generic;
using Graphs;
using UnityEngine;

namespace Src.scripts
{
    public static class PrimsAlgorithm
    {
        public class Edge : Graphs.Edge
        {
            public readonly float Distance;

            public Edge(Vertex a, Vertex b) : base(a, b)
            {
                Distance = Vector3.Distance(a.Position, b.Position);
            }

            public static bool operator ==(Edge a, Edge b)
            {
                return a != null && b != null && (a.A.Equals(b.A) && a.B.Equals(b.B)) || (a.A.Equals(b.B) && a.B.Equals(b.A));
            }
            
            public static bool operator !=(Edge a, Edge b) => !(a == b);
            
            public override bool Equals(object obj) => obj is Edge other && Equals(other);
            public override int GetHashCode() => A.GetHashCode() ^ B.GetHashCode();
        }

        /// Generates the Minimum Spanning Tree (MST) of a given graph using Prim's algorithm.
        /// <param name="edges">A list of edges representing the graph. Each edge connects two vertices and includes a weight (distance).</param>
        /// <param name="start">The starting vertex for the MST generation.</param>
        /// <return>A list of edges representing the MST. If the graph is disconnected, a partial MST for the connected component containing the start vertex is returned.</return>
        public static List<Edge> GetMST(List<Edge> edges, Vertex start)
        {
            HashSet<Vertex> open = new HashSet<Vertex>();
            HashSet<Vertex> closed = new HashSet<Vertex>();

            foreach (Edge edge in edges)
            {
                open.Add(edge.A);
                open.Add(edge.B);
            }

            closed.Add(start);

            List<Edge> mst = new List<Edge>();

            while (open.Count > 0)
            {
                bool chosen = false;
                Edge chosenEdge = null;
                float minWeight = float.MaxValue;

                foreach (Edge edge in edges)
                {
                    int closedVertices = 0;
                    if (!closed.Contains(edge.A)) closedVertices++;
                    if (!closed.Contains(edge.B)) closedVertices++;
                    if (closedVertices != 1) continue;

                    if (edge.Distance < minWeight)
                    {
                        chosenEdge = edge;
                        chosen = true;
                        minWeight = edge.Distance;
                    }
                }
                
                if (!chosen) break;
                mst.Add(chosenEdge);
                open.Remove(chosenEdge.A);
                open.Remove(chosenEdge.B);
                closed.Add(chosenEdge.A);
                closed.Add(chosenEdge.B);
            }

            return mst;
        }
    }
}