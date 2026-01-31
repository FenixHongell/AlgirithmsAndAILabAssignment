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

    [Header("Rooms")] public List<Room> Rooms = new List<Room>();

    private GridManager _gridManager;

    private List<(Vector3 a, Vector3 b)> _graph;
    private List<(Vector3 a, Vector3 b, Vector3 c)> _debugTriangles;

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
        _debugTriangles = GraphManager.CreateGraph(Rooms, _gridManager.gridOrigin.y);
        DrawDebugTriangles(_debugTriangles);
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

        Rooms.Add(room);
        _gridManager.AddToOccupiedCells(position, CellType.Room);
    }

    private void Reset()
    {
        if (Rooms != null)
        {
            foreach (var room in Rooms)
            {
                if (room.GameObject != null)
                {
                    DestroyImmediate(room.GameObject);
                }
            }

            Rooms.Clear();
        }

        _gridManager.Clear();
    }

    private void DrawDebugTriangles(List<(Vector3 a, Vector3 b, Vector3 c)> triangles)
    {
        if (triangles == null)
        {
            Debug.LogError($"{nameof(DungeonGenerator)}: DrawDebugTriangles invoked with null triangles.");
            return;
        }

        foreach (var triangle in triangles)
        {
            Debug.DrawLine(triangle.a, triangle.b, Color.red, 10f);
            Debug.DrawLine(triangle.b, triangle.c, Color.red, 10f);
            Debug.DrawLine(triangle.c, triangle.a, Color.red, 10f);
        }
    }
}