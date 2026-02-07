using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public struct Room
{
    public Vector3Int Position;
    public GameObject GameObject;
}

[RequireComponent(typeof(GridManager))]
public class DungeonGenerator : MonoBehaviour
{
    [Header("Settings")] public int roomCount = 10;
    public int roomBuffer = 5;

    [SerializeField] private List<GameObject> roomPrefabs;

    [Header("Rooms")] public Dictionary<Vector3Int, Room> Rooms = new Dictionary<Vector3Int, Room>();

    private GridManager _gridManager;

    private List<Triangle> _triangles;

    private List<Edge> _mst = new List<Edge>();

    private void OnEnable()
    {
        if (_gridManager == null)
            _gridManager = GetComponent<GridManager>();
    }

    private bool EnsureInitialized()
    {
        if (_gridManager == null)
            _gridManager = GetComponent<GridManager>();

        if (_gridManager == null)
        {
            Debug.LogError($"{nameof(DungeonGenerator)}: Missing GridManager component on the same GameObject.");
            return false;
        }

        if (roomPrefabs == null || roomPrefabs.Count == 0)
        {
            Debug.LogError($"{nameof(DungeonGenerator)}: roomPrefabs list is null or empty.");
            return false;
        }

        if (roomPrefabs.Any(p => p == null))
        {
            Debug.LogError($"{nameof(DungeonGenerator)}: roomPrefabs contains null entries.");
            return false;
        }

        return true;
    }

    [ContextMenu("Generate Dungeon")]
    public void ButtonEventGenerateDungeon()
    {
        Debug.Log($"{nameof(DungeonGenerator)}: GenerateDungeon invoked.");
        if (!EnsureInitialized()) return;

        GenerateCompleteDungeon();
    }

    [ContextMenu("Generate Rooms")]
    public void ButtonEventGenerateRooms()
    {
        Debug.Log($"{nameof(DungeonGenerator)}: GenerateRooms invoked.");
        if (!EnsureInitialized()) return;

        GenerateRooms();
    }
    
    [ContextMenu("Draw Triangles")]
    public void ButtonEventDrawTriangles()
    {
        Debug.Log($"{nameof(DungeonGenerator)}: Draw Triangles invoked.");
        if (!EnsureInitialized()) return;

        DrawDebugTriangles(_triangles);
    }
    
    [ContextMenu("Draw MST")]
    public void ButtonEventDrawMST()
    {
        Debug.Log($"{nameof(DungeonGenerator)}: DrawMST invoked.");
        if (!EnsureInitialized()) return;
        
        DrawDebugMST(_mst);
    }
    

    [ContextMenu("Reset")]
    public void ButtonEventReset()
    {
        Debug.Log($"{nameof(DungeonGenerator)}: Reset invoked.");
        if (!EnsureInitialized()) return;

        Reset();
    }

    private void GenerateCompleteDungeon()
    {
        GenerateRooms();
        var roomsList = Rooms.Select(r => r.Value).ToList();
        _triangles = GraphManager.CreateGraph(roomsList, _gridManager.gridOrigin.y);
        _mst = GraphManager.GetMST(_triangles);
    }

    private void GenerateRooms()
    {
        for (int i = 0; i < roomCount; i++)
        {
            CreateRoom();
        }
    }

    private void CreateRoom()
    {
        int randomX = Random.Range(0, _gridManager.gridSize.x) + _gridManager.gridOrigin.x;
        int randomY = Random.Range(0, _gridManager.gridSize.y) + _gridManager.gridOrigin.y;
        int randomZ = Random.Range(0, _gridManager.gridSize.z) + _gridManager.gridOrigin.z;

        Vector3Int position = new Vector3Int(randomX, randomY, randomZ);

        var occupied = _gridManager.GetOccupiedCells();
        if (occupied != null && occupied.Any(cell =>
                Mathf.Abs(cell.Item1.x - position.x) <= roomBuffer &&
                Mathf.Abs(cell.Item1.y - position.y) <= roomBuffer &&
                Mathf.Abs(cell.Item1.z - position.z) <= roomBuffer))
        {
            return;
        }

        GameObject prefab = roomPrefabs[Random.Range(0, roomPrefabs.Count)];
        if (prefab == null)
        {
            Debug.LogError($"{nameof(DungeonGenerator)}: Selected prefab is null.");
            return;
        }

        GameObject roomObject = Instantiate(prefab, position, Quaternion.identity);

        Room room = new Room
        {
            Position = position,
            GameObject = roomObject
        };

        Rooms.Add(room.Position, room);
        _gridManager.AddToOccupiedCells(position, CellType.Room);
    }

    private void Reset()
    {
        if (Rooms != null)
        {
            foreach (var room in Rooms)
            {
                if (room.Value.GameObject != null)
                {
                    DestroyImmediate(room.Value.GameObject);
                }
            }

            Rooms.Clear();
        }

        _gridManager.Clear();
    }

    private void DrawDebugTriangles(List<Triangle> triangles)
    {
        if (triangles == null)
        {
            Debug.LogError($"{nameof(DungeonGenerator)}: DrawDebugTriangles invoked with null triangles.");
            return;
        }

        foreach (var triangle in triangles)
        {
            Debug.DrawLine(triangle.A, triangle.B, Color.red, 10f);
            Debug.DrawLine(triangle.B, triangle.C, Color.red, 10f);
            Debug.DrawLine(triangle.C, triangle.B, Color.red, 10f);
        }
    }
    
    private void DrawDebugMST(List<Edge> mst)
    {
        if (mst == null)
        {
            Debug.LogError($"{nameof(DungeonGenerator)}: DrawDebugMST invoked with null mst.");
            return;
        }
        
        foreach (var edge in mst)
        {
            Debug.DrawLine(edge.A, edge.B, Color.green, 10f);
        }
    }
}