using System.Collections.Generic;
using UnityEngine;

public class Seed
{
    public Vector2Int LevelDimensions;
    public string DoorHash;
    public Dictionary<Vector2Int, RoomType> PointsOfInterest;

    public Seed()
    {
        PointsOfInterest = new Dictionary<Vector2Int, RoomType>();
    }
    public Seed(string serializedString)
    {
        ParseSeed(serializedString);
    }
    
    public override string ToString()
    {
        // TODO
        // maxRows|maxCols;doorhash;poiRow|poiColumn|poiRoomType;
        return base.ToString();
    }

    private void ParseSeed(string serializedSeed)
    {
        // This magic constant needs to be kept track of manually
        const int SEED_SUBSECTION_MIN_LENGTH = 3;

        // Divide the seed and validate it
        string[] splitSeed = serializedSeed.Split(';');
        if (splitSeed.Length < SEED_SUBSECTION_MIN_LENGTH)
        {
            throw new System.ArgumentException($"Seed does not have enough data points to process correctly. The number of datapoints found was [{splitSeed.Length}].");
        }

        // Dimensions
        string[] splitDimensions = splitSeed[0].Split('|');
        if (splitDimensions.Length < 2)
        {
            throw new System.ArgumentException($"Seed does not contain the correct data to parse dimensions. Data found for the dimensions was [{splitSeed[0]}].");
        }
        if (!int.TryParse(splitDimensions[0], out int maxRows) || !int.TryParse(splitDimensions[1], out int maxColumns))
        {
            throw new System.ArgumentNullException($"Seed must contain valid room dimensions. Row value discovered: [{splitDimensions[0]}] Column value discovered: [{splitDimensions[1]}]");
        }
        LevelDimensions = new Vector2Int(maxRows, maxColumns);

        // Door hash
        DoorHash = splitSeed[1];

        // Points of Interest
        for (int splitSeedIndex = 2; splitSeedIndex < splitSeed.Length; splitSeedIndex++)
        {
            string[] splitPOI = splitSeed[splitSeedIndex].Split();
            if (splitPOI.Length < 3)
            {
                throw new System.ArgumentException($"Seed does not contain the correct data to parse a point of interest. Data found at index [{splitSeedIndex}] for the problem poi was [{splitSeed[splitSeedIndex]}].");
            }
            if (!int.TryParse(splitPOI[0], out int poiRow) || !int.TryParse(splitPOI[1], out int poiColumn) || !System.Enum.TryParse(typeof(RoomType), splitPOI[2], out object roomType))
            {
                throw new System.ArgumentNullException($"Seed must contain valid poi data. Data discovered: [{splitPOI}]");
            }
            PointsOfInterest.Add(new Vector2Int(poiRow, poiColumn), (RoomType)roomType);
        }
        if (!PointsOfInterest.ContainsValue(RoomType.StartRoom))
        {
            throw new System.ArgumentNullException("Seed must contain valid start room dimensions.");
        }
    }
}
