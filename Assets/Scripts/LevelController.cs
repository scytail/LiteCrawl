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
    private string LevelSeedString;
    [SerializeField]
    private Vector2Int LevelDimensions;
    [SerializeField]
    [Range(0,100)]
    private int openDoorChance;
    public GameObject CurrentRoom
    {
        get
        {
            return RoomGrid[CurrentLocation.x][CurrentLocation.y];
        }
    }

    private List<List<GameObject>> RoomGrid;
    [System.NonSerialized]
    public Vector2Int CurrentLocation;
    
    private GameObject _gameController;
    protected GameObject GameManager
    {
        get
        {
            if (!_gameController)
            {
                _gameController = GameObject.FindGameObjectWithTag("GameController");
            }

            return _gameController;
        }
    }
    private Seed LevelSeed;

    #region Room Generation

    public void GenerateRooms()
    {
        if (LevelSeedString is not null && LevelSeedString != string.Empty)
        {
            // We've provided the generator with a seed, so we will ignore all other generator data so that we can force the generator down a specific path
            LevelSeed = new Seed(LevelSeedString);
            GenerateRoomsFromSeed();
            return;
        }

        // No seed was provided, so we'll build one ourselves (for diagnostics and such)
        LevelSeed = new Seed();

        // Log the dimensions for the seed generation
        LevelSeed.LevelDimensions = LevelDimensions;

        // Validate level dimensions
        if (LevelDimensions.x < 2 || LevelDimensions.y < 2)
        {
            throw new System.ArgumentException("Level Dimensions must be at least 2x2", "LevelDimensions");
        }

        gameObject.GetComponent<GameController>().DebugLog($"Room generation has begun. Maximum rows: [{LevelDimensions.x}] Maximum columns: [{LevelDimensions.y}]");

        // New up a blank room grid (we'll need this so that our recursive function can keep track of rooms already generated)
        RoomGrid = BuildEmptyRoomGrid(LevelDimensions.x, LevelDimensions.y);

        // Set up the points of interest
        Vector2Int startRoomCoordinates;
        Dictionary<Vector2Int, RoomType> pointsOfInterest;
        
        // Pick start room (this is the tree root)
        startRoomCoordinates = GeneratePOICoordinates(new List<Vector2Int> { });
        
        // pick any points of interest that we MUST path to (like the descent room)
        pointsOfInterest = new Dictionary<Vector2Int, RoomType>();
        pointsOfInterest.Add(startRoomCoordinates, RoomType.StartRoom);
        pointsOfInterest.Add(GeneratePOICoordinates(new List<Vector2Int> { startRoomCoordinates }), RoomType.DescentRoom);

        // Set the seed data for the POIs
        LevelSeed.PointsOfInterest = pointsOfInterest;

        // Recursively build the dungeon like a depth-first graph, with the root being the start room.
        GenerateRoom(startRoomCoordinates, pointsOfInterest, false);

        gameObject.GetComponent<GameController>().DebugLog($"Room generation has been completed. Seed: {LevelSeed}");
    }

    public void ClearRooms()
    {
        if (!(RoomGrid is null))
        {
            foreach (List<GameObject> RoomRow in RoomGrid)
            {
                foreach (GameObject Room in RoomRow)
                {
                    Destroy(Room);
                }
            }
            RoomGrid = null;
        }
    }

    private void GenerateRoomsFromSeed()
    {
        gameObject.GetComponent<GameController>().DebugLog($"Building pre-generated world from seed.");

        // Override the unity-set data
        LevelDimensions = LevelSeed.LevelDimensions;

        // Validate level dimensions
        if (LevelDimensions.x < 2 || LevelDimensions.y < 2)
        {
            throw new System.ArgumentException("Level Dimensions must be at least 2x2", "LevelDimensions");
        }

        gameObject.GetComponent<GameController>().DebugLog($"Room generation has begun. Maximum rows: [{LevelDimensions.x}] Maximum columns: [{LevelDimensions.y}]");

        // New up a blank room grid (we'll need this so that our recursive function can keep track of rooms already generated)
        RoomGrid = BuildEmptyRoomGrid(LevelDimensions.x, LevelDimensions.y);

        // Set up the points of interest
        Vector2Int startRoomCoordinates = new();
        bool startRoomCoordsFound = false;
        Dictionary<Vector2Int, RoomType> pointsOfInterest;
        
        // Get the points of interest from the seed
        pointsOfInterest = LevelSeed.PointsOfInterest;

        // Find the start room coordinates since we explicitly need those
        foreach (KeyValuePair<Vector2Int, RoomType> room in pointsOfInterest)
        {
            if (room.Value == RoomType.StartRoom)
            {
                startRoomCoordinates = room.Key;
                startRoomCoordsFound = true;
                break;
            }
        }

        if (!startRoomCoordsFound)
        {
            throw new System.ArgumentNullException("Level controller couldn't find the start room from the provided seed.");
        }

        // Recursively build the dungeon like a depth-first graph, with the root being the start room.
        GenerateRoom(startRoomCoordinates, pointsOfInterest, true);
    }

    private List<List<GameObject>> BuildEmptyRoomGrid(int numRows, int numColumns)
    {
        List<List<GameObject>> roomGrid = new List<List<GameObject>>();

        for (int rowIndex = 0; rowIndex < numRows; rowIndex++)
        {
            roomGrid.Add(new List<GameObject>());
            for (int columnIndex = 0; columnIndex < numColumns; columnIndex++)
            {
                roomGrid[rowIndex].Add(null);
            }
        }

        return roomGrid;
    }

    private Vector2Int GeneratePOICoordinates(List<Vector2Int> existingPointsOfInterest)
    {
        // Calculate the new point of interest (And make sure it's not the same as a previous point of interest)
        Vector2Int pointOfInterest = new Vector2Int(Random.Range(0, LevelDimensions.x), Random.Range(0, LevelDimensions.y));
        while (existingPointsOfInterest.Contains(pointOfInterest))
        {
            pointOfInterest = new Vector2Int(Random.Range(0, LevelDimensions.x), Random.Range(0, LevelDimensions.y));
        }
        return pointOfInterest;
    }

    private void GenerateRoom(Vector2Int roomCoordinates, Dictionary<Vector2Int, RoomType> pointsOfInterest, bool useSeed)
    {
        RoomType roomType = GenerateRoomType(roomCoordinates, pointsOfInterest, useSeed);

        RoomDoorData roomDoorData = GenerateDoorData(roomCoordinates, pointsOfInterest, useSeed, out List<Vector2Int> childRoomCoordsList);

        // log the data if we aren't using a seed
        if (!useSeed)
        {
            LevelSeed.RoomTypeList.Add(roomCoordinates, roomType);
            LevelSeed.RoomDoorDataList.Add(roomCoordinates, roomDoorData);
        }
        
        // Generate the physical room and place it onto the level
        BuildRoom(roomCoordinates.x, roomCoordinates.y, roomType, roomDoorData);

        // For each child room, recursively call this method until we no longer have children to visit (either no more doors OR the child room has already been "visited")
        foreach (Vector2Int childRoomCoordinates in childRoomCoordsList)
        {
            if (RoomGrid[childRoomCoordinates.x][childRoomCoordinates.y] is null)
            {
                GenerateRoom(childRoomCoordinates, pointsOfInterest, useSeed);
            }
        }
    }

    private RoomType GenerateRoomType(Vector2Int roomCoordinates, Dictionary<Vector2Int, RoomType> pointsOfInterest, bool useSeed)
    {
        RoomType roomType;
        if (!pointsOfInterest.TryGetValue(roomCoordinates, out roomType))
        {
            // roomType was not in the points of interest, randomly pick the room type
            if (useSeed)
            {
                roomType = LevelSeed.RoomTypeList[roomCoordinates];
            }
            else
            {
                switch (Random.Range(0, 3))
                {
                    case 1:
                        roomType = RoomType.FoodRoom;
                        break;
                    case 2:
                        roomType = RoomType.EmptyRoom;
                        break;
                    default:
                        roomType = RoomType.BasicRoom;
                        break;
                }
            }
        }

        return roomType;
    }

    private RoomDoorData GenerateDoorData(Vector2Int roomCoordinates, Dictionary<Vector2Int, RoomType> pointsOfInterest, bool useSeed, out List<Vector2Int> childRoomCoordsList)
    {
        childRoomCoordsList = new List<Vector2Int>();
        
        // Set doors as what the adjacent room's matching door is set to, or closed if the room doesn't exist
        bool allowNorth = false;
        bool allowSouth = false;
        bool allowWest = false;
        bool allowEast = false;
        if (IsRoomBuilt(roomCoordinates.x - 1, roomCoordinates.y, out _))
        {
            allowNorth = RoomGrid[roomCoordinates.x - 1][roomCoordinates.y].GetComponent<RoomController>().SouthDoor.GetComponent<Renderer>().enabled;
        }
        if (IsRoomBuilt(roomCoordinates.x + 1, roomCoordinates.y, out _))
        {
            allowSouth = RoomGrid[roomCoordinates.x + 1][roomCoordinates.y].GetComponent<RoomController>().NorthDoor.GetComponent<Renderer>().enabled;
        }
        if (IsRoomBuilt(roomCoordinates.x, roomCoordinates.y - 1, out _))
        {
            allowWest = RoomGrid[roomCoordinates.x][roomCoordinates.y - 1].GetComponent<RoomController>().EastDoor.GetComponent<Renderer>().enabled;
        }
        if (IsRoomBuilt(roomCoordinates.x, roomCoordinates.y + 1, out _))
        {
            allowEast = RoomGrid[roomCoordinates.x][roomCoordinates.y + 1].GetComponent<RoomController>().WestDoor.GetComponent<Renderer>().enabled;
        }

        // Force open doors that are potentially on the path to any points of interest (assuming we haven't already found a path to the PoI)
        // NOTE: This isn't forcing open doors on the other side for rooms already made. May need to remedy that.
        // NOTE: This process forces the recursive traversal system to follow the child paths first (by adding them to the child list first)
        bool goVertical;
        if (useSeed)
        {
            goVertical = LevelSeed.poiPrioritizeVerticalFirstList[roomCoordinates];
        }
        else
        {
            goVertical = Random.Range(0, 2) == 1;
            LevelSeed.poiPrioritizeVerticalFirstList.Add(roomCoordinates, goVertical);
        }
        Vector2Int childRoomCoords;
        foreach (Vector2Int pointOfInterest in pointsOfInterest.Keys)
        {
            if (RoomGrid[pointOfInterest.x][pointOfInterest.y] is null && pointOfInterest != roomCoordinates)
            {
                if (goVertical)
                {
                    if (pointOfInterest.x < roomCoordinates.x)
                    {
                        allowNorth = true;
                        childRoomCoords = new Vector2Int(roomCoordinates.x - 1, roomCoordinates.y);
                        if (!childRoomCoordsList.Contains(childRoomCoords))
                        {
                            childRoomCoordsList.Add(childRoomCoords);
                        }
                    }
                    else if (pointOfInterest.x > roomCoordinates.x)
                    {
                        allowSouth = true;
                        childRoomCoords = new Vector2Int(roomCoordinates.x + 1, roomCoordinates.y);
                        if (!childRoomCoordsList.Contains(childRoomCoords))
                        {
                            childRoomCoordsList.Add(childRoomCoords);
                        }
                    }
                    else
                    {
                        //  already aligned vertically; go vertical
                        if (pointOfInterest.y < roomCoordinates.y)
                        {
                            allowWest = true;
                            childRoomCoords = new Vector2Int(roomCoordinates.x, roomCoordinates.y - 1);
                            if (!childRoomCoordsList.Contains(childRoomCoords))
                            {
                                childRoomCoordsList.Add(childRoomCoords);
                            }
                        }
                        else if (pointOfInterest.y > roomCoordinates.y)
                        {
                            allowEast = true;
                            childRoomCoords = new Vector2Int(roomCoordinates.x, roomCoordinates.y + 1);
                            if (!childRoomCoordsList.Contains(childRoomCoords))
                            {
                                childRoomCoordsList.Add(childRoomCoords);
                            }
                        }
                    }
                }
                else  // go horizontal
                {
                    if (pointOfInterest.y < roomCoordinates.y)
                    {
                        allowWest = true;
                        childRoomCoords = new Vector2Int(roomCoordinates.x, roomCoordinates.y - 1);
                        if (!childRoomCoordsList.Contains(childRoomCoords))
                        {
                            childRoomCoordsList.Add(childRoomCoords);
                        }
                    }
                    else if (pointOfInterest.y > roomCoordinates.y)
                    {
                        allowEast = true;
                        childRoomCoords = new Vector2Int(roomCoordinates.x, roomCoordinates.y + 1);
                        if (!childRoomCoordsList.Contains(childRoomCoords))
                        {
                            childRoomCoordsList.Add(childRoomCoords);
                        }
                    }
                    else
                    {
                        // already aligned horizontally; go vertical
                        if (pointOfInterest.x < roomCoordinates.x)
                        {
                            allowNorth = true;
                            childRoomCoords = new Vector2Int(roomCoordinates.x - 1, roomCoordinates.y);
                            if (!childRoomCoordsList.Contains(childRoomCoords))
                            {
                                childRoomCoordsList.Add(childRoomCoords);
                            }
                        }
                        else if (pointOfInterest.x > roomCoordinates.x)
                        {
                            allowSouth = true;
                            childRoomCoords = new Vector2Int(roomCoordinates.x + 1, roomCoordinates.y);
                            if (!childRoomCoordsList.Contains(childRoomCoords))
                            {
                                childRoomCoordsList.Add(childRoomCoords);
                            }
                        }
                    }
                }
            }
        }

        // RNG still-closed doors of rooms not generated (if the adjacent room is generated and the door is still closed, it's already been decided that the door will be closed)
        if (!allowNorth && !IsRoomBuilt(roomCoordinates.x - 1, roomCoordinates.y, out bool outOfBounds) && !outOfBounds)
        {
            if (useSeed)
            {
                allowNorth = LevelSeed.RoomDoorDataList[roomCoordinates].NorthEnabled;
            }
            else
            {
                allowNorth = Random.Range(0, 100) < openDoorChance;
            }
        }
        if (!allowSouth && !IsRoomBuilt(roomCoordinates.x + 1, roomCoordinates.y, out outOfBounds) && !outOfBounds)
        {
            if (useSeed)
            {
                allowSouth = LevelSeed.RoomDoorDataList[roomCoordinates].SouthEnabled;
            }
            else
            {
                allowSouth = Random.Range(0, 100) < openDoorChance;
            }
        }
        if (!allowWest && !IsRoomBuilt(roomCoordinates.x, roomCoordinates.y - 1, out outOfBounds) && !outOfBounds)
        {
            if (useSeed)
            {
                allowWest = LevelSeed.RoomDoorDataList[roomCoordinates].WestEnabled;
            }
            else
            {
                allowWest = Random.Range(0, 100) < openDoorChance;
            }
        }
        if (!allowEast && !IsRoomBuilt(roomCoordinates.x, roomCoordinates.y + 1, out outOfBounds) && !outOfBounds)
        {
            if (useSeed)
            {
                allowEast = LevelSeed.RoomDoorDataList[roomCoordinates].EastEnabled;
            }
            else
            {
                allowEast = Random.Range(0, 100) < openDoorChance;
            }
        }

        // Add other child rooms based on the doors we've opened
        childRoomCoords = new Vector2Int(roomCoordinates.x - 1, roomCoordinates.y);
        if (allowNorth && !childRoomCoordsList.Contains(childRoomCoords))
        {
            childRoomCoordsList.Add(childRoomCoords);
        }
        childRoomCoords = new Vector2Int(roomCoordinates.x + 1, roomCoordinates.y);
        if (allowSouth && !childRoomCoordsList.Contains(childRoomCoords))
        {
            childRoomCoordsList.Add(childRoomCoords);
        }
        childRoomCoords = new Vector2Int(roomCoordinates.x, roomCoordinates.y - 1);
        if (allowWest && !childRoomCoordsList.Contains(childRoomCoords))
        {
            childRoomCoordsList.Add(childRoomCoords);
        }
        childRoomCoords = new Vector2Int(roomCoordinates.x, roomCoordinates.y + 1);
        if (allowEast && !childRoomCoordsList.Contains(childRoomCoords))
        {
            childRoomCoordsList.Add(childRoomCoords);
        }

        RoomDoorData doorData = new RoomDoorData(allowNorth, allowSouth, allowWest, allowEast);

        return doorData;
    }

    private void BuildRoom(int rowIndex, int columnIndex, RoomType roomType, RoomDoorData doors)
    {
        GameObject roomPrefab;

        // Find the prefab to place
        switch (roomType)
        {
            case RoomType.BasicRoom:
                roomPrefab = RoomData.BasicRoomPrefab;
                break;
            case RoomType.FoodRoom:
                roomPrefab = RoomData.FoodRoomPrefab;
                break;
            case RoomType.EmptyRoom:
                roomPrefab = RoomData.EmptyRoomPrefab;
                break;
            case RoomType.StartRoom:
                roomPrefab = RoomData.EmptyRoomPrefab;
                CurrentLocation = new Vector2Int(rowIndex, columnIndex);
                break;
            case RoomType.DescentRoom:
                roomPrefab = RoomData.DescentRoomPrefab;
                break;
            default:
                roomPrefab = RoomData.EmptyRoomPrefab;
                break;
        }

        // Generate the room and place it
        RoomGrid[rowIndex][columnIndex] = Instantiate(roomPrefab);
        RoomGrid[rowIndex][columnIndex].transform.position = new Vector2(columnIndex * 10, rowIndex * -10);
        RoomController roomController = RoomGrid[rowIndex][columnIndex].gameObject.GetComponent<RoomController>();
        foreach (Renderer renderer in roomController.NorthDoor.GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = doors.NorthEnabled;
        }
        foreach (Renderer renderer in roomController.SouthDoor.GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = doors.SouthEnabled;
        }
        foreach (Renderer renderer in roomController.WestDoor.GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = doors.WestEnabled;
        }
        foreach (Renderer renderer in roomController.EastDoor.GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = doors.EastEnabled;
        }

        // Hide it on the minimap by default
        roomController.SetMinimapVisibility(false);
    }

    private bool IsRoomBuilt(int rowIndex, int columnIndex, out bool isOutOfBounds)
    {
        // We're outside the bounds of the level, therefore we haven't visited the room (technically)
        if (rowIndex < 0 || rowIndex >= LevelDimensions.x || columnIndex < 0 || columnIndex >= LevelDimensions.y)
        {
            isOutOfBounds = true;
            return false;
        }

        // If the room isn't null, we've visited it
        isOutOfBounds = false;
        return !(RoomGrid[rowIndex][columnIndex] is null);
    }
    
    #endregion

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
}
