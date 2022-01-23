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
        LevelDoorData levelDoorData = CalculateRoomConnections(levelMap);
        PlaceRooms(levelMap, levelDoorData);
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

    private LevelDoorData CalculateRoomConnections(List<List<RoomType>> levelMap)
    {
        LevelDoorData adjacencyList = new LevelDoorData(LevelDimensions.x, LevelDimensions.y);

        for (int row = 0; row < LevelDimensions.x; row++)
        {
            for (int column = 0; column < LevelDimensions.y; column++)
            {
                // edges should never have doors
                bool allowNorth = row != 0;
                bool allowSouth = row != levelMap.Count - 1;
                bool allowWest = column != 0;
                bool allowEast = column != levelMap[row].Count - 1;

                // Update room (and adjacent rooms)
                adjacencyList.UpdateRoom(row, column, new RoomDoorData(allowNorth, allowSouth, allowWest, allowEast));
            }
        }

        return adjacencyList;
    }

    private void PlaceRooms(List<List<RoomType>> levelMap, LevelDoorData levelDoorData)
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
                        PlaceRoom(x, y, RoomData.BasicRoomPrefab, levelDoorData.GetRoomDoorData(x, y));
                        break;
                    case RoomType.FoodRoom:
                        PlaceRoom(x, y, RoomData.FoodRoomPrefab, levelDoorData.GetRoomDoorData(x, y));
                        break;
                    case RoomType.EmptyRoom:
                        PlaceRoom(x, y, RoomData.EmptyRoomPrefab, levelDoorData.GetRoomDoorData(x, y));
                        break;
                }
            }
        }
    }

    private void PlaceRoom(int row, int column, GameObject roomPrefab, RoomDoorData doors)
    {
        RoomGrid[row].Add(Instantiate(roomPrefab));
        RoomGrid[row][column].transform.position = new Vector2(column * 10, row * -10);
        RoomGrid[row][column].gameObject.GetComponent<RoomController>().NorthDoor.GetComponent<Renderer>().enabled = doors.NorthEnabled;
        RoomGrid[row][column].gameObject.GetComponent<RoomController>().SouthDoor.GetComponent<Renderer>().enabled = doors.SouthEnabled;
        RoomGrid[row][column].gameObject.GetComponent<RoomController>().WestDoor.GetComponent<Renderer>().enabled = doors.WestEnabled;
        RoomGrid[row][column].gameObject.GetComponent<RoomController>().EastDoor.GetComponent<Renderer>().enabled = doors.EastEnabled;
    }
}
