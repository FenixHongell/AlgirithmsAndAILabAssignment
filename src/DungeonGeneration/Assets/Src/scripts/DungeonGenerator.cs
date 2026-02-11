using System;
using System.Collections.Generic;
using Graphs;
using Src.Configs;
using UnityEngine;
using Random = System.Random;

namespace Src.scripts
{
    public class DungeonGenerator : MonoBehaviour
    {
        enum CellType : byte
        {
            None = 0,
            Room = 1,
            Hallway = 2
        }

        private class Room
        {
            public RectInt Bounds;

            public Room(Vector2Int location, Vector2Int size)
            {
                Bounds = new RectInt(location, size);
            }

            public static bool Intersect(Room a, Room b)
            {
                return !((a.Bounds.position.x >= (b.Bounds.position.x + b.Bounds.size.x)) ||
                         ((a.Bounds.position.x + a.Bounds.size.x) <= b.Bounds.position.x)
                         || (a.Bounds.position.y >= (b.Bounds.position.y + b.Bounds.size.y)) ||
                         ((a.Bounds.position.y + a.Bounds.size.y) <= b.Bounds.position.y));
            }
        }

        public DungeonGeneratorConfig config;

        private System.Random _random;
        private Grid<CellType> _grid;
        private List<Room> _rooms;
        private Delaunay _delaunay;
        HashSet<PrimsAlgorithm.Edge> _tree;

        private void Start()
        {
            Generate();
        }

        void Generate()
        {
            _random = new Random(config.seed);
            _grid = new Grid<CellType>(config.size, Vector2Int.zero);
            _rooms = new List<Room>();

            SpawnRooms();
            Triangulate();
            CreateHallways();
            FindHallways();
        }

        void SpawnRooms()
        {
            while (_rooms.Count < config.roomCount)
            {
                Vector2Int location = new Vector2Int(
                    _random.Next(0, config.size.x),
                    _random.Next(0, config.size.y));
                Vector2Int roomSize = new Vector2Int(
                    _random.Next(1, config.roomMaxSize.x + 1),
                    _random.Next(1, config.roomMaxSize.y + 1));

                bool add = true;
                Room newRoom = new Room(location, roomSize);
                Room buffer = new Room(location + new Vector2Int(-1, -1), roomSize + new Vector2Int(2, 2));

                foreach (Room room in _rooms)
                {
                    if (Room.Intersect(room, buffer))
                    {
                        add = false;
                        break;
                    }
                }

                if (newRoom.Bounds.xMin < 0 || newRoom.Bounds.xMax >= config.size.x || newRoom.Bounds.yMin < 0 ||
                    newRoom.Bounds.yMax >= config.size.y) add = false;

                if (add)
                {
                    _rooms.Add(newRoom);
                    SpawnRoom(newRoom.Bounds.position, newRoom.Bounds.size, _rooms.Count - 1);

                    foreach (Vector2Int p in newRoom.Bounds.allPositionsWithin)
                    {
                        _grid[p] = CellType.Room;
                    }
                }
            }
        }

        void CreateHallways()
        {
            Debug.Log("Creating hallways...");
            List<PrimsAlgorithm.Edge> edges = new List<PrimsAlgorithm.Edge>();
            
            if (_delaunay.Edges.Count == 0)
            {
                Debug.LogError("No edges");
                return;
            }

            foreach (Delaunay.Edge edge in _delaunay.Edges)
            {
               
                edges.Add(new PrimsAlgorithm.Edge(edge.A, edge.B));
            }

            List<PrimsAlgorithm.Edge> mst = PrimsAlgorithm.GetMST(edges, edges[0].A);

            _tree = new HashSet<PrimsAlgorithm.Edge>(mst);
            HashSet<PrimsAlgorithm.Edge> remaining = new HashSet<PrimsAlgorithm.Edge>(edges);
            remaining.ExceptWith(_tree);

            foreach (PrimsAlgorithm.Edge edge in remaining)
            {
                if (_random.NextDouble() < config.luckyNumber)
                {
                    _tree.Add(edge);
                }
            }

            foreach (var edge in _tree)
            {
                var vertexA = edge.A as Vertex<Room>;
                var vertexB = edge.B as Vertex<Room>;
                Debug.DrawLine(new Vector3(vertexA.Position.x, 0, vertexA.Position.y),
                    new Vector3(vertexB.Position.x, 0, vertexB.Position.y),
                    Color.green,
                    100f);
            }
        }

        void FindHallways()
        {
            Debug.Log("Finding hallways...");
            Pathfinder pathfinder = new Pathfinder(config.size);

            foreach (PrimsAlgorithm.Edge edge in _tree)
            {
                Room startRoom = (edge.A as Vertex<Room>).Item;
                Room endRoom = (edge.B as Vertex<Room>).Item;

                Vector2Int startPos = new Vector2Int((int)startRoom.Bounds.center.x, (int)startRoom.Bounds.center.y);
                Vector2Int endPos = new Vector2Int((int)endRoom.Bounds.center.x, (int)endRoom.Bounds.center.y);

                var path = pathfinder.FindPath(startPos, endPos, (Pathfinder.Node a, Pathfinder.Node b) =>
                {
                    Pathfinder.PathCost cost = new Pathfinder.PathCost();

                    cost.Cost = Vector2Int.Distance(b.Position, endPos);

                    if (_grid[b.Position] == CellType.Room)
                    {
                        cost.Cost += 10;
                    }
                    else if (_grid[b.Position] == CellType.None)
                    {
                        cost.Cost += 5;
                    }
                    else if (_grid[b.Position] == CellType.Hallway)
                    {
                        cost.Cost += 1;
                    }

                    cost.Traversable = true;

                    return cost;
                });

                Debug.Log($"Path found: {path != null}");

                if (path != null)
                {
                    for (int i = 0; i < path.Count; i++)
                    {
                        var current = path[i];

                        if (_grid[current] == CellType.None)
                        {
                            _grid[current] = CellType.Hallway;
                        }
                    }

                    foreach (Vector2Int pos in path)
                    {
                        if (_grid[pos] == CellType.Hallway)
                        {
                            PlaceHallway(pos);
                        }
                    }
                }
            }
        }

        void InstantiateCube(Vector2Int location, Vector2Int size, Material material)
        {
            GameObject gameObject =
                Instantiate(config.prefab, new Vector3(location.x, 0, location.y), Quaternion.identity);
            gameObject.transform.localScale = new Vector3(size.x, 1, size.y);
            gameObject.GetComponent<MeshRenderer>().material = material;
        }
        
        void InstantiateCube(Vector2Int location, Vector2Int size, Material material, string name)
        {
            GameObject gameObject =
                Instantiate(config.prefab, new Vector3(location.x, 0, location.y), Quaternion.identity);
            gameObject.name = name;
            gameObject.transform.localScale = new Vector3(size.x, 1, size.y);
            gameObject.GetComponent<MeshRenderer>().material = material;
        }

        void SpawnRoom(Vector2Int location, Vector2Int roomSize, int index)
        {
            InstantiateCube(location, roomSize, config.roomMaterial, $"Room_{index}" );;
        }

        void Triangulate()
        {
            List<Vertex> vertices = new List<Vertex>();
            foreach (Room room in _rooms)
            {
                vertices.Add(new Vertex<Room>((Vector2)room.Bounds.position + ((Vector2)room.Bounds.size) / 2, room));
            }
            
            _delaunay = Delaunay.Create(vertices);

            foreach (var edge in _delaunay.Edges)
            {
                Debug.DrawLine(new Vector3(edge.A.Position.x, 0, edge.A.Position.y),
                    new Vector3(edge.B.Position.x, 0, edge.B.Position.y),
                    Color.red,
                    100f);
            }
        }

        void PlaceHallway(Vector2Int location)
        {
            Debug.Log($"Placing hallway at {location}");
            InstantiateCube(location, new Vector2Int(1, 1), config.pathMaterial);
        }
    }
}