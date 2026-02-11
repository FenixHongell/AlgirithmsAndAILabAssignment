using System;
using System.Collections.Generic;
using Graphs;
using UnityEngine;

namespace Src.scripts
{
    /// <summary>
    /// Provides the implementation of the Delaunay triangulation algorithm, which produces a mesh of non-overlapping triangles
    /// connecting a set of vertices in 2D or 3D space.
    /// </summary>
    /// <remarks>
    /// The Delaunay class is responsible for creating and managing a set of vertices, edges, and triangles that adhere to the
    /// Delaunay triangulation properties. These properties ensure that for each triangle, the circumcircle does not contain
    /// any other vertices in its interior.
    /// </remarks>
    public class Delaunay
    {
        // Class nesting is such an interesting concept, unsure how I feel about it.
        
        /// <summary>
        /// Represents a triangle defined by three vertices in a 2D or 3D space.
        /// </summary>
        public class Triangle : IEquatable<Triangle>
        {
            public readonly Vertex A, B, C;
            public bool IsBad;

            public Triangle(Vertex a, Vertex b, Vertex c)
            {
                A = a;
                B = b;
                C = c;
            }

            public override bool Equals(object obj) => obj is Triangle other && Equals(other);

            public bool Equals(Triangle other) =>
                other != null && A.Equals(other.A) && B.Equals(other.B) && C.Equals(other.C);

            public override int GetHashCode() => A.GetHashCode() ^ B.GetHashCode() ^ C.GetHashCode();

            public bool ContainsVertex(Vector3 p) => Vector3.Distance(p, A.Position) < Globals.SmallNumber ||
                                                     Vector3.Distance(p, B.Position) < Globals.SmallNumber ||
                                                     Vector3.Distance(p, C.Position) < Globals.SmallNumber;
            
            public bool InCircle(Vector3 p)
            {
                double ax = (double)A.Position.x - p.x;
                double ay = (double)A.Position.y - p.y;
                double bx = (double)B.Position.x - p.x;
                double by = (double)B.Position.y - p.y;
                double cx = (double)C.Position.x - p.x;
                double cy = (double)C.Position.y - p.y;
                
                double det = (ax * ax + ay * ay) * (bx * cy - cx * by) -
                             (bx * bx + by * by) * (ax * cy - cx * ay) +
                             (cx * cx + cy * cy) * (ax * by - bx * ay);

                double circumcenterSide = (bx * cy - cx * by) - (ax * cy - cx * ay) + (ax * by - bx * ay);
    
                if (circumcenterSide > 0) return det > 0.000001;
                return det < -0.000001;
            }

            public static bool operator ==(Triangle A, Triangle B) {
                return (A.A == B.A || A.A == B.B || A.A == B.C)
                       && (A.B == B.A || A.B == B.B || A.B == B.C)
                       && (A.C == B.A || A.C == B.B || A.C == B.C);
            }

            public static bool operator !=(Triangle a, Triangle b) => !(a == b);
        }

        /// <summary>
        /// Represents an edge connecting two vertices in a 2D or 3D space.
        /// </summary>
        /// <remarks>
        /// The <see cref="Edge"/> class defines equality and hash operations based on its vertices, treating
        /// edges as being undirected. This means that the order of vertices does not affect equality.
        /// </remarks>
        public class Edge
        {
            public readonly Vertex A, B;
            public bool IsBad;
            
            public Edge(Vertex a, Vertex b)
            {
                A = a; B = b;
            }
            
            public static bool operator ==(Edge a, Edge b)
            {
                return a != null && b != null && a.Equals(b);
            }
            public static bool operator !=(Edge a, Edge b) => !(a == b);

            public override bool Equals(object obj)
            {
                if (obj is Edge e) return e.Equals(this);
                return false;
            }
            public bool Equals(Edge other) =>  (A.Equals(other.A) && B.Equals(other.B)) || (A.Equals(other.B) && B.Equals(other.A));
            public override int GetHashCode() => A.GetHashCode() ^ B.GetHashCode();
            
        }

        public List<Vertex> Vertices { get; private set; }
        public List<Edge> Edges { get; private set; }
        public List<Triangle> Triangles { get; private set; }

        Delaunay()
        {
            Edges = new List<Edge>();
            Triangles = new List<Triangle>();
        }

        public static Delaunay Create(List<Vertex> vertices)
        {
            Delaunay delaunay = new Delaunay();
            delaunay.Vertices = new List<Vertex>(vertices);
            delaunay.Create();
            
            return delaunay;
        }

        void Create()
        {
            float minX = Vertices[0].Position.x;
            float minY = Vertices[0].Position.y;
            float maxX = minX;
            float maxY = minY;

            // I refuse to use var, I want to see the types >:(
            foreach (Vertex v in Vertices)
            {
                if (v.Position.x < minX) minX = v.Position.x;
                if (v.Position.x > maxX) maxX = v.Position.x;
                if (v.Position.y < minY) minY = v.Position.y;
                if (v.Position.y > maxY) maxY = v.Position.y;
            }
            
            float dx = maxX - minX;
            float dy = maxY - minY;
            float deltaMax = Mathf.Max(dx, dy) * 2;
            
            Vertex p1 = new Vertex(new Vector2(minX - 1, minY - 1));
            Vertex p2 = new Vertex(new Vector2(minX - 1, maxY + deltaMax));
            Vertex p3 = new Vertex(new Vector2(maxX + deltaMax, minY - 1));
            
            Debug.DrawLine(new Vector3(p1.Position.x, 0, p1.Position.y),
                new Vector3(p2.Position.x, 0, p2.Position.y),
                Color.blue,
                100f);
            Debug.DrawLine(new Vector3(p2.Position.x, 0, p2.Position.y),
                new Vector3(p3.Position.x, 0, p3.Position.y),
                Color.blue,
                100f);
            Debug.DrawLine(new Vector3(p3.Position.x, 0, p3.Position.y),
                new Vector3(p1.Position.x, 0, p1.Position.y),
                Color.blue,
                100f);
            
            Triangles.Add(new Triangle(p1, p2, p3));

            foreach (Vertex v in Vertices)
            {
                List<Edge> polygon = new List<Edge>();

                foreach (Triangle t in Triangles)
                {
                    if (t.InCircle(v.Position))
                    {
                        t.IsBad = true;
                        polygon.Add(new Edge(t.A, t.B));
                        polygon.Add(new Edge(t.B, t.C));
                        polygon.Add(new Edge(t.C, t.A));
                    }
                }
                
                Triangles.RemoveAll(t => t.IsBad);

                for (int i = 0; i < polygon.Count; i++)
                {
                    for (int j = i + 1; j < polygon.Count; j++)
                    {
                        if (polygon[i].Equals(polygon[j]))
                        {
                            polygon[i].IsBad = true;
                            polygon[j].IsBad = true;
                        }
                    }
                }
                
                polygon.RemoveAll(e => e.IsBad);
                
                foreach (Edge e in polygon)
                {
                    Triangles.Add(new Triangle(e.A, e.B, v));
                }
            }
            
            Triangles.RemoveAll((Triangle t) => t.ContainsVertex(p1.Position) || t.ContainsVertex(p2.Position) || t.ContainsVertex(p3.Position));

            HashSet<Edge> edgeHashSet = new HashSet<Edge>();

            foreach (Triangle t in Triangles)
            {
                Edge ab = new Edge(t.A, t.B);
                Edge bc = new Edge(t.B, t.C);
                Edge ca = new Edge(t.C, t.A);

                if (edgeHashSet.Add(ab)) Edges.Add(ab);
                if (edgeHashSet.Add(bc)) Edges.Add(bc); 
                if (edgeHashSet.Add(ca)) Edges.Add(ca);
            }
        }
    }
}