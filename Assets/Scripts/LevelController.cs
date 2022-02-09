using System.Collections.Generic;
using UnityEngine;

public enum RoomType
{
    BasicRoom,
    FoodRoom,
    EmptyRoom,
    StartRoom,
    DescentRoom
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
    [SerializeField]
    [Range(0,100)]
    private int openDoorChance;

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

    private class RoomGenData
    {
        public List<List<RoomType>> RoomTypeGrid;
        public Dictionary<RoomType, Vector2Int> SpecialRooms;

        public RoomGenData()
        {
            RoomTypeGrid = new List<List<RoomType>>();
            SpecialRooms = new Dictionary<RoomType, Vector2Int>();
        }
    }

    public void GenerateRooms()
    {
        if (LevelDimensions.x < 2 || LevelDimensions.y < 2)
        {
            throw new System.ArgumentException("Level Dimensions must be at least 2x2", "LevelDimensions");
        }
        RoomGenData roomGenData = CalculateLevelRoomTypes();
        LevelDoorData levelDoorData = CalculateRoomConnections(roomGenData);
        PlaceRooms(roomGenData.RoomTypeGrid, levelDoorData);
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

    private RoomGenData CalculateLevelRoomTypes()
    {
        RoomGenData roomGenData = new RoomGenData();

        // Calculate start room
        Vector2Int startRoomCoordinates = new Vector2Int(Random.Range(0, LevelDimensions.x), Random.Range(0, LevelDimensions.y));
        roomGenData.SpecialRooms.Add(RoomType.StartRoom, startRoomCoordinates);

        // Calculate end room (And make sure it's not the same as the start room)
        Vector2Int descentRoomCoordinates = new Vector2Int(Random.Range(0, LevelDimensions.x), Random.Range(0, LevelDimensions.y));
        while (descentRoomCoordinates.x == startRoomCoordinates.x && descentRoomCoordinates.y == startRoomCoordinates.y)
        {
            descentRoomCoordinates = new Vector2Int(Random.Range(0, LevelDimensions.x), Random.Range(0, LevelDimensions.y));
        }
        roomGenData.SpecialRooms.Add(RoomType.DescentRoom, descentRoomCoordinates);

        // Generate the room data
        for (int x = 0; x < LevelDimensions.x; x++)
        {
            roomGenData.RoomTypeGrid.Add(new List<RoomType>());
            for (int y = 0; y < LevelDimensions.y; y++)
            {
                // Mark the room if it's a special room
                if (x == startRoomCoordinates.x && y == startRoomCoordinates.y)
                {
                    roomGenData.RoomTypeGrid[x].Add(RoomType.StartRoom);
                } 
                else if (x == descentRoomCoordinates.x && y == descentRoomCoordinates.y)
                {
                    roomGenData.RoomTypeGrid[x].Add(RoomType.DescentRoom);
                }
                else
                {
                    // PoC: randomly generate all other non-special rooms
                    switch (Random.Range(0, 3))
                    {
                        case 1:
                            roomGenData.RoomTypeGrid[x].Add(RoomType.FoodRoom);
                            break;
                        case 2:
                            roomGenData.RoomTypeGrid[x].Add(RoomType.EmptyRoom);
                            break;
                        default:
                            roomGenData.RoomTypeGrid[x].Add(RoomType.BasicRoom);
                            break;
                    }
                }
            }
        }

        return roomGenData;
    }

    private LevelDoorData CalculateRoomConnections(RoomGenData roomGenData)
    {
        LevelDoorData levelDoorData = new LevelDoorData(LevelDimensions.x, LevelDimensions.y);

        // Calculate required doors
        List<Vector2Int> pathToExit = CalculatePathFromStartRoomToDescentRoom(roomGenData);

        // Iterate over all the rooms and determine what doors are to be opened for that room
        for (int rowIndex = 0; rowIndex < LevelDimensions.x; rowIndex++)
        {
            for (int columnIndex = 0; columnIndex < LevelDimensions.y; columnIndex++)
            {
                // Load the current data (as other rooms may have set the doors as open already)
                RoomDoorData currentDoorData = levelDoorData.GetRoomDoorData(rowIndex, columnIndex);
                bool allowNorth = currentDoorData.NorthEnabled;
                bool allowSouth = currentDoorData.SouthEnabled;
                bool allowWest = currentDoorData.WestEnabled;
                bool allowEast = currentDoorData.EastEnabled;

                // Enable the doors tied to the exit path
                if (pathToExit.Contains(new Vector2Int(rowIndex, columnIndex)))
                {
                    if (pathToExit.Contains(new Vector2Int(rowIndex - 1, columnIndex)))
                    {
                        allowNorth = true;
                    }
                    if (pathToExit.Contains(new Vector2Int(rowIndex + 1, columnIndex)))
                    {
                        allowSouth = true;
                    }
                    if (pathToExit.Contains(new Vector2Int(rowIndex, columnIndex - 1)))
                    {
                        allowWest = true;
                    }
                    if (pathToExit.Contains(new Vector2Int(rowIndex, columnIndex + 1)))
                    {
                        allowEast = true;
                    }
                }

                // randomly open all still-disabled doors based on their open chance EXCEPT if room is descent room
                // Note that this only sets north and west doors... because the east and south doors will be set by their adjacent rooms.
                // If we did all four rooms, we'd effectively be giving each door 2x the chance of opening... simulating a roll with "Advantage."
                if (new Vector2Int(rowIndex, columnIndex) != roomGenData.SpecialRooms[RoomType.DescentRoom])
                {
                    if (!allowNorth && new Vector2Int(rowIndex - 1, columnIndex) != roomGenData.SpecialRooms[RoomType.DescentRoom])
                    {
                        allowNorth = Random.Range(0, 100) < openDoorChance;
                    }
                    if (!allowWest && new Vector2Int(rowIndex, columnIndex - 1) != roomGenData.SpecialRooms[RoomType.DescentRoom])
                    {
                        allowWest = Random.Range(0, 100) < openDoorChance;
                    }
                }

                // Edges should never have doors, and thus should override any open doors previously set
                if (rowIndex == 0)
                {
                    allowNorth = false;
                } 
                else if (rowIndex == LevelDimensions.x - 1)
                {
                    allowSouth = false;
                } 
                if (columnIndex == 0)
                {
                    allowWest = false;
                }
                else if (columnIndex == LevelDimensions.y - 1)
                {
                    allowEast = false;
                }

                // Update room (and adjacent rooms)
                levelDoorData.UpdateRoom(rowIndex, columnIndex, new RoomDoorData(allowNorth, allowSouth, allowWest, allowEast));
            }
        }

        return levelDoorData;
    }

    private List<Vector2Int> CalculatePathFromStartRoomToDescentRoom(RoomGenData roomGenData)
    {
        // find a path from start to finish by going either vertical first or horizontal first
        // (basically making an "L" of sorts)
        List<Vector2Int> pathToExit = new List<Vector2Int>();
        pathToExit.Add(roomGenData.SpecialRooms[RoomType.StartRoom]);
        bool goVerticalBeforeHorizontal = Random.Range(0, 2) == 1;
        int columnIndex = roomGenData.SpecialRooms[RoomType.StartRoom].y;
        int rowIndex = roomGenData.SpecialRooms[RoomType.StartRoom].x;
        int indexIncrement;
        if (goVerticalBeforeHorizontal)
        {
            // rows
            if (rowIndex > roomGenData.SpecialRooms[RoomType.DescentRoom].x)
            {
                indexIncrement = -1;
            }
            else
            {
                indexIncrement = 1;
            }
            while (rowIndex != roomGenData.SpecialRooms[RoomType.DescentRoom].x)
            {
                rowIndex += indexIncrement;
                pathToExit.Add(new Vector2Int(rowIndex, columnIndex));
            }
            // columns
            if (columnIndex > roomGenData.SpecialRooms[RoomType.DescentRoom].y)
            {
                indexIncrement = -1;
            }
            else
            {
                indexIncrement = 1;
            }
            while (columnIndex != roomGenData.SpecialRooms[RoomType.DescentRoom].y)
            {
                columnIndex += indexIncrement;
                pathToExit.Add(new Vector2Int(rowIndex, columnIndex));
            }
        }
        else
        {
            // columns
            if (columnIndex > roomGenData.SpecialRooms[RoomType.DescentRoom].y)
            {
                indexIncrement = -1;
            }
            else
            {
                indexIncrement = 1;
            }
            while (columnIndex != roomGenData.SpecialRooms[RoomType.DescentRoom].y)
            {
                columnIndex += indexIncrement;
                pathToExit.Add(new Vector2Int(rowIndex, columnIndex));
            }
            // rows
            if (rowIndex > roomGenData.SpecialRooms[RoomType.DescentRoom].x)
            {
                indexIncrement = -1;
            }
            else
            {
                indexIncrement = 1;
            }
            while (rowIndex != roomGenData.SpecialRooms[RoomType.DescentRoom].x)
            {
                rowIndex += indexIncrement;
                pathToExit.Add(new Vector2Int(rowIndex, columnIndex));
            }
        }

        return pathToExit;
    }

    private void PlaceRooms(List<List<RoomType>> levelMap, LevelDoorData levelDoorData)
    {
        RoomGrid = new List<List<GameObject>>();
        for (int rowIndex = 0; rowIndex < LevelDimensions.x; rowIndex++)
        {
            RoomGrid.Add(new List<GameObject>());
            for (int columnIndex = 0; columnIndex < LevelDimensions.y; columnIndex++)
            {
                switch (levelMap[rowIndex][columnIndex])
                {
                    case RoomType.BasicRoom:
                        PlaceRoom(rowIndex, columnIndex, RoomData.BasicRoomPrefab, levelDoorData.GetRoomDoorData(rowIndex, columnIndex));
                        break;
                    case RoomType.FoodRoom:
                        PlaceRoom(rowIndex, columnIndex, RoomData.FoodRoomPrefab, levelDoorData.GetRoomDoorData(rowIndex, columnIndex));
                        break;
                    case RoomType.EmptyRoom:
                        PlaceRoom(rowIndex, columnIndex, RoomData.EmptyRoomPrefab, levelDoorData.GetRoomDoorData(rowIndex, columnIndex));
                        break;
                    case RoomType.StartRoom:
                        PlaceRoom(rowIndex, columnIndex, RoomData.EmptyRoomPrefab, levelDoorData.GetRoomDoorData(rowIndex, columnIndex));
                        CurrentLocation = new Vector2Int(rowIndex, columnIndex);
                        break;
                    case RoomType.DescentRoom:
                        PlaceRoom(rowIndex, columnIndex, RoomData.DescentRoomPrefab, levelDoorData.GetRoomDoorData(rowIndex, columnIndex));
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
