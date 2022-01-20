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
        bool moveWasPerformed = false;

        switch (direction)
        {
            case MoveDirection.Down:
                if (CurrentLocation.y > 0)
                {
                    CurrentLocation.y -= 1;
                    moveWasPerformed = true;
                }
                break;
            case MoveDirection.Up:
                if (CurrentLocation.y < RoomGrid[CurrentLocation.x].Count - 1)
                {
                    CurrentLocation.y += 1;
                    moveWasPerformed = true;
                }
                break;
            case MoveDirection.Left:
                if (CurrentLocation.x > 0)
                {
                    CurrentLocation.x -= 1;
                    moveWasPerformed = true;
                }
                break;
            case MoveDirection.Right:
                if (CurrentLocation.x < RoomGrid.Count - 1)
                {
                    CurrentLocation.x += 1;
                    moveWasPerformed = true;
                }
                break;
        }

        return moveWasPerformed;
    }
    
    private void GenerateRooms()
    {
        List<List<RoomType>> levelMap = CalculateLevelRoomTypes();
        RoomGrid = new List<List<GameObject>>();
        for (int x = 0; x < LevelDimensions.x; x++)
        {
            RoomGrid.Add(new List<GameObject>());
            for (int y = 0; y < LevelDimensions.y; y++)
            {
                switch (levelMap[x][y])
                {
                    case RoomType.BasicRoom:
                        PlaceRoom(x, y, RoomData.BasicRoomPrefab);
                        break;
                    case RoomType.FoodRoom:
                        PlaceRoom(x, y, RoomData.FoodRoomPrefab);
                        break;
                    case RoomType.EmptyRoom:
                        PlaceRoom(x, y, RoomData.EmptyRoomPrefab);
                        break;
                }
            }
        }
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

    private void PlaceRoom(int row, int column, GameObject roomPrefab)
    {
        RoomGrid[row].Add(Instantiate(roomPrefab));
        RoomGrid[row][column].transform.position = new Vector2(row * 10, column * 10);
    }
}
