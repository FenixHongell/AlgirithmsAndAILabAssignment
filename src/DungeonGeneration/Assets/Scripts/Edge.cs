using System;
using UnityEngine;

public readonly struct Edge : IEquatable<Edge>
{
    public readonly Vector3 A;
    public readonly Vector3 B;

    public Edge(Vector3 a, Vector3 b)
    {
        if (Compare(a, b) <= 0) { A = a; B = b; }
        else { A = b; B = a; }
    }

    public bool Equals(Edge other) => A.Equals(other.A) && B.Equals(other.B);
    public override bool Equals(object obj) => obj is Edge other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            return (A.GetHashCode() * 397) ^ B.GetHashCode();
        }
    }

    private static int Compare(Vector3 p, Vector3 q)
    {
        int cx = p.x.CompareTo(q.x);
        if (cx != 0) return cx;
        int cy = p.y.CompareTo(q.y);
        if (cy != 0) return cy;
        return p.z.CompareTo(q.z);
    }
}