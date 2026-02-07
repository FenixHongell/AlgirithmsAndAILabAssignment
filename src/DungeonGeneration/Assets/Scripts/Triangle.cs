using System;
using UnityEngine;

public struct Triangle : IEquatable<Triangle>
{
        public Vector3 A, B, C;
        
        public Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            A = a; B = b; C = c;
        }
        
        public bool Equals(Triangle other) => A.Equals(other.A) && B.Equals(other.B) && C.Equals(other.C);
        public override bool Equals(object obj) => obj is Triangle other && Equals(other);
        
        public override int GetHashCode()
        {
            unchecked
            {
                return (A.GetHashCode() * 397) ^ B.GetHashCode();
            }
        }
}