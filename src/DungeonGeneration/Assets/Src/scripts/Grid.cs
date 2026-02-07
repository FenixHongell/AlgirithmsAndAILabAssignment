using UnityEngine;

namespace Src.scripts
{
    /// <summary>
    /// Represents a generic grid structure for managing data in a 2D array-like manner.
    /// </summary>
    /// <typeparam name="T">The type of elements to store within the grid.</typeparam>
    public class Grid<T>
    {
        private T[] data;

        public Vector2Int Size;
        public Vector2Int Origin;

        public Grid(Vector2Int size, Vector2Int origin)
        {
            Size = size;
            Origin = origin;
            data = new T[size.x * size.y];
        }
        
        public int GetIndex(Vector2Int pos) => pos.x + pos.y * Size.x;
        public bool InBounds(Vector2Int pos) => new RectInt(Vector2Int.zero, Size).Contains(pos + Origin);

        public T this[int x, int y]
        {
            get
            {
                return this[new Vector2Int(x, y)];
            }
            set
            {
                this[new Vector2Int(x, y)] = value;
            }
        }

        public T this[Vector2Int pos]
        {
            get
            {
                pos += Origin;
                return data[GetIndex(pos)];
            }
            set
            {
                pos += Origin;
                data[GetIndex(pos)] = value;
            }
        }
    }
}