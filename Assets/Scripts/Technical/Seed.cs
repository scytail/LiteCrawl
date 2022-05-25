using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Seed
{
    public Vector2Int LevelDimensions;
    public Dictionary<Vector2Int, RoomType> RoomTypeList;
    public Dictionary<Vector2Int, RoomDoorData> RoomDoorDataList;
    public Dictionary<Vector2Int, RoomType> PointsOfInterest;

    public Seed()
    {
        init();
    }
    public Seed(string serializedString)
    {
        init();
        ParseSeed(serializedString);
    }
    
    public override string ToString()
    {
        // maxRows|maxCols;roomRowCoord,roomColCoord,roomType,roomDoorData|[moreRoomData];poiRow|poiColumn|poiRoomType;[more POIs];

        // build level dimensions
        string seedString = $"{LevelDimensions.x}|{LevelDimensions.y};";

        // build room data
        List<string> roomDataStrings = new();
        foreach (KeyValuePair<Vector2Int, RoomType> roomTypeData in RoomTypeList)
        {
            roomDataStrings.Add($"{roomTypeData.Key.x},{roomTypeData.Key.y},{roomTypeData.Value},{RoomDoorDataList[roomTypeData.Key]}");
        }
        seedString += $"{string.Join('|', roomDataStrings)};";

        // build POIs
        foreach (KeyValuePair<Vector2Int, RoomType> poi in PointsOfInterest)
        {
            seedString += $"{poi.Key.x}|{poi.Key.y}|{poi.Value};";
        }

        return seedString;
    }

    private void init()
    {
        RoomTypeList = new();
        RoomDoorDataList = new();
        PointsOfInterest = new();
    }

    private void ParseSeed(string serializedSeed)
    {
        // These magic constants need to be kept track of manually
        const int SEED_SUBSECTION_MIN_LENGTH = 3;
        const int DIMENSION_SUBSECTION_INDEX = 0;
        const int ROOM_HASH_SUBSECTTION_INDEX = 1;
        const int POI_SUBSECTION_START_INDEX = 2;

        // Divide the seed and validate it
        string[] splitSeed = serializedSeed.Split(';');
        if (splitSeed.Length < SEED_SUBSECTION_MIN_LENGTH)
        {
            throw new System.ArgumentException($"Seed does not have enough data points to process correctly. The number of datapoints found was [{splitSeed.Length}].");
        }

        LevelDimensions = ParseDimensions(splitSeed[DIMENSION_SUBSECTION_INDEX]);

        ParseRoomHash(splitSeed[ROOM_HASH_SUBSECTTION_INDEX]);

        PointsOfInterest = ParsePointsOfInterest(splitSeed.Skip(POI_SUBSECTION_START_INDEX).ToArray());
    }

    private Vector2Int ParseDimensions(string seedSubsection)
    {
        string[] splitDimensions = seedSubsection.Split('|');
        if (splitDimensions.Length < 2)
        {
            throw new System.ArgumentException($"Seed does not contain the correct data to parse dimensions. Data found for the dimensions was [{seedSubsection}].");
        }
        if (!int.TryParse(splitDimensions[0], out int maxRows) || !int.TryParse(splitDimensions[1], out int maxColumns))
        {
            throw new System.ArgumentNullException($"Seed must contain valid room dimensions. Row value discovered: [{splitDimensions[0]}] Column value discovered: [{splitDimensions[1]}]");
        }
        return new Vector2Int(maxRows, maxColumns);
    }

    private void ParseRoomHash(string seedSubsection)
    {
        if (seedSubsection == string.Empty)
        {
            throw new System.ArgumentNullException("Seed does not contain any room hash data.");
        }

        string[] splitRoomDataList = seedSubsection.Split('|');

        if (splitRoomDataList.Length < 4)  // Min dimensions are 2x2, so we can assume there should be at least 4 rooms here
        {
            throw new System.ArgumentNullException($"Seed does not contain enough room data. Data found was: [{seedSubsection}]");
        }

        foreach (string roomDataString in splitRoomDataList)
        {
            string[] splitRoomData = roomDataString.Split(',');

            if (splitRoomData.Length != 4)
            {
                throw new System.ArgumentException($"Room data does not contain the proper amount of information. Room data found: [{roomDataString}]");
            }

            if (!int.TryParse(splitRoomData[0], out int roomRowCoord) || 
                !int.TryParse(splitRoomData[1], out int roomColumnCoord) ||
                roomRowCoord < 0 ||
                roomRowCoord > LevelDimensions.x ||
                roomColumnCoord < 0 ||
                roomColumnCoord > LevelDimensions.y)
            {
                throw new System.ArgumentException($"Seed must contain valid room coordinates. Row value found: [{splitRoomData[0]}] Column value found: [{splitRoomData[1]}]");
            }
            if (!System.Enum.TryParse(splitRoomData[2], out RoomType roomType))
            {
                throw new System.ArgumentException($"Room type was not valid. Value found: [{splitRoomData[2]}]");
            }
            if (!RoomDoorData.TryParse(splitRoomData[3], out RoomDoorData doorData))
            {
                throw new System.ArgumentException($"Room door data was not valid. Value found: [{splitRoomData[3]}]");
            }

            Vector2Int roomCoords = new(roomRowCoord, roomColumnCoord);
            RoomTypeList.Add(roomCoords, roomType);
            RoomDoorDataList.Add(roomCoords, doorData);
        }


    }

    private Dictionary<Vector2Int, RoomType> ParsePointsOfInterest(string[] seedSubsections)
    {
        Dictionary<Vector2Int, RoomType> pointsOfInterest = new();

        for (int splitSeedIndex = 0; splitSeedIndex < seedSubsections.Length; splitSeedIndex++)
        {
            if (seedSubsections[splitSeedIndex] != string.Empty) 
            { 
                string[] splitPOI = seedSubsections[splitSeedIndex].Split('|');
                if (splitPOI.Length < 3)
                {
                    throw new System.ArgumentException($"Seed does not contain the correct data to parse a point of interest. Data found at index [{splitSeedIndex}] for the problem poi was [{seedSubsections[splitSeedIndex]}].");
                }
                if (!int.TryParse(splitPOI[0], out int poiRow) || !int.TryParse(splitPOI[1], out int poiColumn) || !System.Enum.TryParse(typeof(RoomType), splitPOI[2], out object roomType))
                {
                    throw new System.ArgumentNullException($"Seed must contain valid poi data. Data discovered: [{splitPOI}]");
                }
                pointsOfInterest.Add(new Vector2Int(poiRow, poiColumn), (RoomType)roomType);
            }
        }
        if (!pointsOfInterest.ContainsValue(RoomType.StartRoom))
        {
            throw new System.ArgumentNullException("Seed must contain valid start room dimensions.");
        }

        return pointsOfInterest;
    }
}
