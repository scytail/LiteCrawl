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

    private List<List<GameObject>> RoomGrid;
    [System.NonSerialized]
    public Vector2Int CurrentLocation;
    public GameObject CurrentRoom
    {
        get
        {
            return RoomGrid[CurrentLocation.x][CurrentLocation.y];
        }
    }

    #region Room Generation
    
    public void GenerateRooms()
    {
        if (LevelDimensions.x < 2 || LevelDimensions.y < 2)
        {
            throw new System.ArgumentException("Level Dimensions must be at least 2x2", "LevelDimensions");
        }

        // New up a blank room grid (we'll need this so that our recursive function can keep track of rooms already generated)
        RoomGrid = new List<List<GameObject>>();

        for (int rowIndex = 0; rowIndex < LevelDimensions.x; rowIndex++)
        {
            RoomGrid.Add(new List<GameObject>());
            for (int columnIndex = 0; columnIndex < LevelDimensions.y; columnIndex++)
            {
                RoomGrid[rowIndex].Add(null);
            }
        }

        // Pick start room (this is the tree root)
        Vector2Int startRoomCoordinates = GeneratePOICoordinates(new List<Vector2Int> { });

        // pick any points of interest that we MUST path to (like the descent room)
        Dictionary<Vector2Int, RoomType> pointsOfInterest = new Dictionary<Vector2Int, RoomType>();
        pointsOfInterest.Add(startRoomCoordinates, RoomType.StartRoom);
        pointsOfInterest.Add(GeneratePOICoordinates(new List<Vector2Int> { startRoomCoordinates }), RoomType.DescentRoom);

        // Recursively build the dungeon like a depth-first graph, with the root being the start room.
        GenerateRoom(startRoomCoordinates, pointsOfInterest);
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

    private void GenerateRoom(Vector2Int roomCoordinates, Dictionary<Vector2Int, RoomType> pointsOfInterest)
    {
        RoomType roomType = GenerateRoomType(roomCoordinates, pointsOfInterest);

        RoomDoorData roomDoorData = GenerateDoorData(roomCoordinates, pointsOfInterest);

        // Find the child rooms based on the doors we've opened
        List<Vector2Int> childRooms = new List<Vector2Int>();
        if (roomDoorData.NorthEnabled)
        {
            childRooms.Add(new Vector2Int(roomCoordinates.x - 1, roomCoordinates.y));
        }
        if (roomDoorData.SouthEnabled)
        {
            childRooms.Add(new Vector2Int(roomCoordinates.x + 1, roomCoordinates.y));
        }
        if (roomDoorData.WestEnabled)
        {
            childRooms.Add(new Vector2Int(roomCoordinates.x, roomCoordinates.y - 1));
        }
        if (roomDoorData.EastEnabled)
        {
            childRooms.Add(new Vector2Int(roomCoordinates.x, roomCoordinates.y + 1));
        }
        
        // Generate the physical room and place it onto the level
        BuildRoom(roomCoordinates.x, roomCoordinates.y, roomType, roomDoorData);

        // For each child room, recursively call this method until we no longer have children to visit (either no more doors OR the child room has already been "visited")
        foreach (Vector2Int childRoomCoordinates in childRooms)
        {
            if (RoomGrid[childRoomCoordinates.x][childRoomCoordinates.y] is null)
            {
                GenerateRoom(childRoomCoordinates, pointsOfInterest);
            }
        }
    }

    private RoomType GenerateRoomType(Vector2Int roomCoordinates, Dictionary<Vector2Int, RoomType> pointsOfInterest)
    {
        // Pick room type
        RoomType roomType;
        if (!pointsOfInterest.TryGetValue(roomCoordinates, out roomType))
        {
            // roomType was not in the points of interest, randomly pick the room type
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

        return roomType;
    }

    private RoomDoorData GenerateDoorData(Vector2Int roomCoordinates, Dictionary<Vector2Int, RoomType> pointsOfInterest)
    {
        // Set doors as what the adjacent room's matching door is set to, or closed if the room doesn't exist
        bool allowNorth = false;
        if (IsRoomBuilt(roomCoordinates.x - 1, roomCoordinates.y, out _))
        {
            allowNorth = RoomGrid[roomCoordinates.x - 1][roomCoordinates.y].gameObject.GetComponent<RoomController>().SouthDoor.GetComponent<Renderer>().enabled;
        }
        bool allowSouth = false;
        if (IsRoomBuilt(roomCoordinates.x + 1, roomCoordinates.y, out _))
        {
            allowSouth = RoomGrid[roomCoordinates.x + 1][roomCoordinates.y].gameObject.GetComponent<RoomController>().NorthDoor.GetComponent<Renderer>().enabled;
        }
        bool allowWest = false;
        if (IsRoomBuilt(roomCoordinates.x, roomCoordinates.y - 1, out _))
        {
            allowWest = RoomGrid[roomCoordinates.x][roomCoordinates.y - 1].gameObject.GetComponent<RoomController>().EastDoor.GetComponent<Renderer>().enabled;
        }
        bool allowEast = false;
        if (IsRoomBuilt(roomCoordinates.x, roomCoordinates.y + 1, out _))
        {
            allowEast = RoomGrid[roomCoordinates.x][roomCoordinates.y + 1].gameObject.GetComponent<RoomController>().WestDoor.GetComponent<Renderer>().enabled;
        }

        // Force open doors that are on the path to any points of interest (assuming we haven't already found a path to the PoI)
        bool outOfBounds;
        bool goHorizontal = Random.Range(0, 2) == 1;  // false = verticcal, true = horizontal
        foreach (Vector2Int pointOfInterest in pointsOfInterest.Keys)
        {
            if (RoomGrid[pointOfInterest.x][pointOfInterest.y] is null && pointOfInterest != roomCoordinates)
            {
                if (goHorizontal)
                {
                    if (pointOfInterest.x < roomCoordinates.x)
                    {
                        allowNorth = true;
                    }
                    else if (pointOfInterest.x > roomCoordinates.x)
                    {
                        allowSouth = true;
                    }
                    else
                    {
                        //  already aligned vertically; go vertical
                        if (pointOfInterest.y < roomCoordinates.y)
                        {
                            allowWest = true;
                        }
                        else if (pointOfInterest.y > roomCoordinates.y)
                        {
                            allowEast = true;
                        }
                    }
                }
                else  // go vertical
                {
                    if (pointOfInterest.y < roomCoordinates.y)
                    {
                        allowWest = true;
                    }
                    else if (pointOfInterest.y > roomCoordinates.y)
                    {
                        allowEast = true;
                    }
                    else
                    {
                        // already aligned horizontally; go vertical
                        if (pointOfInterest.x < roomCoordinates.x)
                        {
                            allowNorth = true;
                        }
                        else if (pointOfInterest.x > roomCoordinates.x)
                        {
                            allowSouth = true;
                        }
                    }
                }
            }
        }

        // RNG still-closed doors of rooms not generated (if the adjacent room is generated and the door is still closed, it's already been decided that the door will be closed)
        if (!allowNorth && !IsRoomBuilt(roomCoordinates.x - 1, roomCoordinates.y, out outOfBounds) && !outOfBounds)
        {
            allowNorth = Random.Range(0, 100) < openDoorChance;
        }
        if (!allowSouth && !IsRoomBuilt(roomCoordinates.x + 1, roomCoordinates.y, out outOfBounds) && !outOfBounds)
        {
            allowSouth = Random.Range(0, 100) < openDoorChance;
        }
        if (!allowWest && !IsRoomBuilt(roomCoordinates.x, roomCoordinates.y - 1, out outOfBounds) && !outOfBounds)
        {
            allowWest = Random.Range(0, 100) < openDoorChance;
        }
        if (!allowEast && !IsRoomBuilt(roomCoordinates.x, roomCoordinates.y + 1, out outOfBounds) && !outOfBounds)
        {
            allowEast = Random.Range(0, 100) < openDoorChance;
        }

        return new RoomDoorData(allowNorth, allowSouth, allowWest, allowEast);
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

    private bool IsRoomBuilt(int rowIndex, int columnIndex, out bool outOfBounds)
    {
        // We're outside the bounds of the level, therefore we haven't visited the room (technically)
        if (rowIndex < 0 || rowIndex >= LevelDimensions.x || columnIndex < 0 || columnIndex >= LevelDimensions.y)
        {
            outOfBounds = true;
            return false;
        }

        // If the room isn't null, we've visited it
        outOfBounds = false;
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
