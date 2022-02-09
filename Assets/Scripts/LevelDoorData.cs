using System.Collections.Generic;
using UnityEngine;

public class RoomDoorData
{
    public bool NorthEnabled;
    public bool SouthEnabled;
    public bool WestEnabled;
    public bool EastEnabled;

    public RoomDoorData()
    {
        NorthEnabled = false;
        SouthEnabled = false;
        WestEnabled = false;
        EastEnabled = false;
    }
    public RoomDoorData(bool north, bool south, bool west, bool east)
    {
        NorthEnabled = north;
        SouthEnabled = south;
        WestEnabled = west;
        EastEnabled = east;
    }
}

public class LevelDoorData
{
    private List<List<RoomDoorData>> Rooms;
    private int Rows;
    private int Columns;

    public LevelDoorData(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;

        Rooms = new List<List<RoomDoorData>>();
        for (int rowIndex = 0; rowIndex < rows; rowIndex++)
        {
            Rooms.Add(new List<RoomDoorData>());
            for (int columnIndex = 0; columnIndex < columns; columnIndex++)
            {
                Rooms[rowIndex].Add(new RoomDoorData());
            }
        }
    }

    public void UpdateRoom(int row, int column, RoomDoorData doorData)
    {
        Rooms[row][column] = doorData;

        UpdateAdjacentRooms(row, column, doorData);
    }

    public RoomDoorData GetRoomDoorData(int row, int column)
    {
        return Rooms[row][column];
    }

    private void UpdateAdjacentRooms(int row, int column, RoomDoorData doorData)
    {
        // Update adjacent north room
        int relatedRoomRow = row - 1;
        int relatedRoomColumn = column;
        if (relatedRoomRow >= 0)
        {
            RoomDoorData relatedRoomDoors = Rooms[relatedRoomRow][relatedRoomColumn];
            Rooms[relatedRoomRow][relatedRoomColumn] = new RoomDoorData(relatedRoomDoors.NorthEnabled, doorData.NorthEnabled, relatedRoomDoors.WestEnabled, relatedRoomDoors.EastEnabled);
        }

        // Update adjacent south room
        relatedRoomRow = row + 1;
        relatedRoomColumn = column;
        if (relatedRoomRow < Rooms.Count)
        {
            RoomDoorData relatedRoomDoors = Rooms[relatedRoomRow][relatedRoomColumn];
            Rooms[relatedRoomRow][relatedRoomColumn] = new RoomDoorData(doorData.SouthEnabled, relatedRoomDoors.SouthEnabled, relatedRoomDoors.WestEnabled, relatedRoomDoors.EastEnabled);
        }

        // Update adjacent west room
        relatedRoomRow = row;
        relatedRoomColumn = column - 1;
        if (relatedRoomColumn >= 0)
        {
            RoomDoorData relatedRoomDoors = Rooms[relatedRoomRow][relatedRoomColumn];
            Rooms[relatedRoomRow][relatedRoomColumn] = new RoomDoorData(relatedRoomDoors.NorthEnabled, relatedRoomDoors.SouthEnabled, relatedRoomDoors.WestEnabled, doorData.WestEnabled);
        }

        // Update adjacent east room
        relatedRoomRow = row;
        relatedRoomColumn = column + 1;
        if (relatedRoomColumn < Rooms.Count)
        {
            RoomDoorData relatedRoomDoors = Rooms[relatedRoomRow][relatedRoomColumn];
            Rooms[relatedRoomRow][relatedRoomColumn] = new RoomDoorData(relatedRoomDoors.NorthEnabled, relatedRoomDoors.SouthEnabled, doorData.EastEnabled, relatedRoomDoors.EastEnabled);
        }
    }

    private int GetRoomIdFromCoordinates(int row, int column)
    {
        return row * Rows + column;
    }
}