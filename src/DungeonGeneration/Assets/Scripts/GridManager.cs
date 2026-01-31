using System;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }
    
    [Header("Settings")]
    public Vector3Int gridSize;
    public Vector3Int gridOrigin;
    public static readonly Vector3Int GridCellSize = new Vector3Int(1, 1, 1);

    private List<Tuple<Vector3Int, CellType>> _occupiedCells = new List<Tuple<Vector3Int, CellType>>();    
    

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }


    public void AddToOccupiedCells(Vector3Int cell, CellType type)
    {
        _occupiedCells.Add(new Tuple<Vector3Int, CellType>(cell, type));
    }

    public List<Tuple<Vector3Int, CellType>> GetOccupiedCells()
    {
        return _occupiedCells;
    }
    
    public bool RemoveFromOccupiedCells(Vector3Int cell)
    {
        var item = new Tuple<Vector3Int, CellType>(cell, CellType.Room);
        if (!_occupiedCells.Contains(item)) return false;
        _occupiedCells.Remove(item);
        return true;
    }

    public void Clear()
    {
        _occupiedCells.Clear();
    }
}
