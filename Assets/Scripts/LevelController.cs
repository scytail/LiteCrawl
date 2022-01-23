using System.Collections.Generic;
using UnityEngine;

public enum RoomType
{
    BasicRoom,
    FoodRoom,
    EmptyRoom
}

public enum MoveDirection
{
    Up,
    Down,
    Left,
    Right
}

class DoorsEnabled
{
    public bool North;
    public bool South;
    public bool West;
    public bool East;

    public DoorsEnabled()
    {
        North = true;
        South = true;
        West = true;
        East = true;
    }
    public DoorsEnabled(bool north, bool south, bool west, bool east)
    {
        North = north;
        South = south;
        West = west;
        East = east;
    }
}

public class LevelController : MonoBehaviour
{   
    [SerializeField]
    private RoomDataScriptableObject RoomData;
    [SerializeField]
    private Vector2Int LevelDimensions;

    [System.NonSerialized]
    public List<List<GameObject>> RoomGrid;
    [System.NonSerialized]
    public Vector2Int CurrentLocation;
    public GameObject CurrentRoom
    {
        get
        {
            return RoomGrid[CurrentLocation.x][CurrentLocation.y];
        }
    }

    public void Awake()
    {
        GenerateRooms();
    }

    public bool MoveToNewRoom(MoveDirection direction)
    {
        bool moveWasPerformed = ValidateMove(direction);
        
        if (moveWasPerformed)
        {
            switch (direction)
            {
                case MoveDirection.Up:
                    CurrentLocation.x -= 1;
                    break;
                case MoveDirection.Down:
                    CurrentLocation.x += 1;
                    break;
                case MoveDirection.Left:
                    CurrentLocation.y -= 1;
                    break;
                case MoveDirection.Right:
                    CurrentLocation.y += 1;
                    break;
            }
        }

        return moveWasPerformed;
    }
    
    private bool ValidateMove(MoveDirection direction)
    {
        switch (direction)
        {
            case MoveDirection.Up:
                return CurrentRoom.GetComponent<RoomController>().NorthDoor.GetComponent<Renderer>().enabled;
            case MoveDirection.Down:
                return CurrentRoom.GetComponent<RoomController>().SouthDoor.GetComponent<Renderer>().enabled;
            case MoveDirection.Left:
                return CurrentRoom.GetComponent<RoomController>().WestDoor.GetComponent<Renderer>().enabled;
            case MoveDirection.Right:
                return CurrentRoom.GetComponent<RoomController>().EastDoor.GetComponent<Renderer>().enabled;
            default:
                return false;
        }
    }

    private void GenerateRooms()
    {
        List<List<RoomType>> levelMap = CalculateLevelRoomTypes();
        List<DoorsEnabled> doors = CalculateRoomConnections(levelMap);
        PlaceRooms(levelMap, doors);
    }

    private List<List<RoomType>> CalculateLevelRoomTypes()
    {
        List<List<RoomType>> roomTypeGrid = new List<List<RoomType>>();

        // TODO:
        // find "start" and "end" (need two new room types: an empty room for the start and a "descent" room for the end)
        // find path from start to end
        // RNG the rest (don't forget that no room counts as an option here)

        // PoC:
        for (int x = 0; x < LevelDimensions.x; x++)
        {
            roomTypeGrid.Add(new List<RoomType>());
            for (int y = 0; y < LevelDimensions.y; y++)
            {
                switch (Random.Range(0, 3))
                {
                    case 1:
                        roomTypeGrid[x].Add(RoomType.FoodRoom);
                        break;
                    case 2:
                        roomTypeGrid[x].Add(RoomType.EmptyRoom);
                        break;
                    default:
                        roomTypeGrid[x].Add(RoomType.BasicRoom);
                        break;
                }
            }
        }

        return roomTypeGrid;
    }

    private List<DoorsEnabled> CalculateRoomConnections(List<List<RoomType>> levelMap)
    {
        List<DoorsEnabled> adjacencyList = new List<DoorsEnabled>(LevelDimensions.x * LevelDimensions.y);
        bool allowNorth, allowSouth, allowEast, allowWest;

        // Init the adjacency list
        for (int i = 0; i < LevelDimensions.x * LevelDimensions.y; i++)
        {
            adjacencyList.Add(new DoorsEnabled());
        }

        for (int row = 0; row < LevelDimensions.x; row++)
        {
            for (int column = 0; column < LevelDimensions.y; column++)
            {
                int roomId = GetRoomIdFromCoordinates(row, column);
                // edges should never have doors
                allowNorth = row != 0;
                allowSouth = row != levelMap.Count - 1;
                allowWest = column != 0;
                allowEast = column != levelMap[row].Count - 1;

                // Update room
                adjacencyList[roomId] = new DoorsEnabled(allowNorth, allowSouth, allowWest, allowEast);

                // Update adjacent north room
                int relatedRoomId = roomId - LevelDimensions.x;
                if (relatedRoomId >= 0)
                {
                    DoorsEnabled relatedRoomDoors = adjacencyList[relatedRoomId];
                    adjacencyList[relatedRoomId] = new DoorsEnabled(relatedRoomDoors.North, allowNorth, relatedRoomDoors.West, relatedRoomDoors.East);
                }

                // Update adjacent south room
                relatedRoomId = roomId + LevelDimensions.x;
                if (relatedRoomId < adjacencyList.Count)
                {
                    DoorsEnabled relatedRoomDoors = adjacencyList[relatedRoomId];
                    adjacencyList[relatedRoomId] = new DoorsEnabled(allowSouth, relatedRoomDoors.South, relatedRoomDoors.West, relatedRoomDoors.East);
                }

                // Update adjacent west room
                relatedRoomId = roomId - 1;
                if (relatedRoomId >= 0)
                {
                    DoorsEnabled relatedRoomDoors = adjacencyList[relatedRoomId];
                    adjacencyList[relatedRoomId] = new DoorsEnabled(relatedRoomDoors.North, relatedRoomDoors.South, relatedRoomDoors.West, allowWest);
                }

                // Update adjacent east room
                relatedRoomId = roomId + 1;
                if (relatedRoomId < adjacencyList.Count)
                {
                    DoorsEnabled relatedRoomDoors = adjacencyList[relatedRoomId];
                    adjacencyList[relatedRoomId] = new DoorsEnabled(relatedRoomDoors.North, relatedRoomDoors.South, allowEast, relatedRoomDoors.East);
                }
            }
        }

        return adjacencyList;
    }

    private void PlaceRooms(List<List<RoomType>> levelMap, List<DoorsEnabled> doors)
    {
        RoomGrid = new List<List<GameObject>>();
        for (int x = 0; x < LevelDimensions.x; x++)
        {
            RoomGrid.Add(new List<GameObject>());
            for (int y = 0; y < LevelDimensions.y; y++)
            {
                switch (levelMap[x][y])
                {
                    case RoomType.BasicRoom:
                        PlaceRoom(x, y, RoomData.BasicRoomPrefab, doors[GetRoomIdFromCoordinates(x, y)]);
                        break;
                    case RoomType.FoodRoom:
                        PlaceRoom(x, y, RoomData.FoodRoomPrefab, doors[GetRoomIdFromCoordinates(x, y)]);
                        break;
                    case RoomType.EmptyRoom:
                        PlaceRoom(x, y, RoomData.EmptyRoomPrefab, doors[GetRoomIdFromCoordinates(x, y)]);
                        break;
                }
            }
        }
    }

    private int GetRoomIdFromCoordinates(int row, int column)
    {
        return row * LevelDimensions.x + column;
    }

    private void PlaceRoom(int row, int column, GameObject roomPrefab, DoorsEnabled doors)
    {
        RoomGrid[row].Add(Instantiate(roomPrefab));
        RoomGrid[row][column].transform.position = new Vector2(column * 10, row * -10);
        RoomGrid[row][column].gameObject.GetComponent<RoomController>().NorthDoor.GetComponent<Renderer>().enabled = doors.North;
        RoomGrid[row][column].gameObject.GetComponent<RoomController>().SouthDoor.GetComponent<Renderer>().enabled = doors.South;
        RoomGrid[row][column].gameObject.GetComponent<RoomController>().WestDoor.GetComponent<Renderer>().enabled = doors.West;
        RoomGrid[row][column].gameObject.GetComponent<RoomController>().EastDoor.GetComponent<Renderer>().enabled = doors.East;
    }
}
