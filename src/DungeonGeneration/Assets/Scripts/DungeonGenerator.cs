using System.Collections.Generic;
using UnityEngine;

public struct Room
{
    public Vector3Int Position, Size;
}

public class DungeonGenerator : MonoBehaviour
{
    [Header("Settings")]
    public int roomCount = 10;
    public Vector3Int maxRoomSize = new Vector3Int(5, 5, 1);
    public Vector3Int minRoomSize = new Vector3Int(1, 1, 1);
    
    [Header("Rooms")]
    public List<Room> Rooms;
    
    [ContextMenu("Generate Dungeon")]
    public void ButtonEventGenerateDungeon()
    {
        Debug.Log($"{nameof(DungeonGenerator)}: GenerateDungeon invoked.");
        
        GenerateCompleteDungeon();
    }
    
    [ContextMenu("Generate Rooms")]
    public void ButtonEventGenerateRooms()
    {
        Debug.Log($"{nameof(DungeonGenerator)}: GenerateDungeon invoked.");
        
        GenerateRooms();
    }
    
    [ContextMenu("Reset")]
    public void ButtonEventReset()
    {
        Debug.Log($"{nameof(DungeonGenerator)}: GenerateDungeon invoked.");
        
        Reset();
    }

    private void GenerateCompleteDungeon()
    {
        
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
        int randomX = Random.Range(0, GridManager.Instance.gridSize.x) + GridManager.Instance.gridOrigin.x;
        int randomY = Random.Range(0, GridManager.Instance.gridSize.y) + GridManager.Instance.gridOrigin.y;
        int randomZ = Random.Range(0, GridManager.Instance.gridSize.z) + GridManager.Instance.gridOrigin.z;
        
        int sizeX = Random.Range(minRoomSize.x, maxRoomSize.x);
        int sizeY = Random.Range(minRoomSize.y, maxRoomSize.y);
        int sizeZ = Random.Range(minRoomSize.z, maxRoomSize.z);
        
        Rooms.Add(new Room {Position = new Vector3Int(randomX, randomY, randomZ), Size = new Vector3Int(sizeX, sizeY, sizeZ)});

        Instantiate(new GameObject(), new Vector3(randomX, randomY, randomZ), Quaternion.identity);
    }

    private bool CollidesOrTouchesExistingRoom(Room candidate)
    {
        const int gap = 1;
        foreach (var room in Rooms)
        {
            if (OverlapsWithGap(room, candidate, gap))
                return true;
        }

        return false;

    }
    
    private static bool OverlapsWithGap(Room a, Room b, int gapCells)
    {
        var gap = new Vector3Int(gapCells, gapCells, gapCells);

        Vector3Int aMin = a.Position - gap;
        Vector3Int aMax = a.Position + a.Size + gap;

        Vector3Int bMin = b.Position;
        Vector3Int bMax = b.Position + b.Size;

        bool overlapX = aMin.x < bMax.x && aMax.x > bMin.x;
        bool overlapY = aMin.y < bMax.y && aMax.y > bMin.y;
        bool overlapZ = aMin.z < bMax.z && aMax.z > bMin.z;

        return overlapX && overlapY && overlapZ;
    }


    private void Reset()
    {
        
    }
}