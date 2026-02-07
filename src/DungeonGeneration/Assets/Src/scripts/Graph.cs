using System;
using System.Collections.Generic;
using UnityEngine;

namespace Graphs
{
    /// <summary>
    /// Represents a vertex in a graph with a specific position in three-dimensional space.
    /// </summary>
    /// <remarks>
    /// The <see cref="Vertex"/> class provides functionality for comparing vertices based on their position.
    /// </remarks>
    public class Vertex : IEquatable<Vertex>
    {
        public readonly Vector3 Position;
        
        public Vertex() {}

        public Vertex(Vector3 position)
        {
            Position = position;
        }
        
        public override bool Equals(object obj) => obj is Vertex other && Equals(other);
        public bool Equals(Vertex other) => other != null && Position.Equals(other.Position);
        public override int GetHashCode() => Position.GetHashCode();
    }

    /// <summary>
    /// Represents a vertex in a graph with a specific position in three-dimensional space and associated data.
    /// </summary>
    /// <typeparam name="T">The type of the data associated with the vertex.</typeparam>
    public class Vertex<T> : Vertex
    {
        public readonly T Item;
        
        public Vertex(Vector3 position, T data) : base(position)
        {
            Item = data;
        }
    }

    /// <summary>
    /// Represents an edge connecting two vertices in a graph.
    /// </summary>
    /// <remarks>
    /// The <see cref="Edge"/> class defines the equality operations and hash code behavior for edges
    /// based on the vertices they connect. It also supports comparison and ensures edges are defined
    /// by their endpoints.
    /// </remarks>
    public class Edge : IEquatable<Edge>
    {
        public readonly Vertex A, B;
        
        public Edge(Vertex a, Vertex b)
        {
            A = a; B = b;
        }
        
        public override bool Equals(object obj) => obj is Edge other && Equals(other);
        public bool Equals(Edge other) => other != null && A.Equals(other.A) && B.Equals(other.B);
        public override int GetHashCode() => A.GetHashCode() ^ B.GetHashCode();
        
        public static bool operator ==(Edge a, Edge b) => a?.Equals(b) ?? b is null;
        public static bool operator !=(Edge a, Edge b) => !(a == b);
    }
}