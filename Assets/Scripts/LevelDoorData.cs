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
        NorthEnabled = true;
        SouthEnabled = true;
        WestEnabled = true;
        EastEnabled = true;
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
    private List<RoomDoorData> Rooms;
    private int Rows;
    private int Columns;

    public LevelDoorData(int rows, int columns)
    {
        Rooms = new List<RoomDoorData>(rows*columns);
        Rows = rows;
        Columns = columns;

        for (int i = 0; i < rows*columns; i++)
        {
            Rooms.Add(new RoomDoorData());
        }
    }

    public void UpdateRoom(int row, int column, RoomDoorData doorData)
    {
        int roomId = GetRoomIdFromCoordinates(row, column);
        Rooms[roomId] = doorData;

        UpdateAdjacentRooms(roomId, doorData);
    }

    public RoomDoorData GetRoomDoorData(int row, int column)
    {
        return Rooms[GetRoomIdFromCoordinates(row, column)];
    }

    private void UpdateAdjacentRooms(int roomId, RoomDoorData doorData)
    {
        // Update adjacent north room
        int relatedRoomId = roomId - Rows;
        if (relatedRoomId >= 0)
        {
            RoomDoorData relatedRoomDoors = Rooms[relatedRoomId];
            Rooms[relatedRoomId] = new RoomDoorData(relatedRoomDoors.NorthEnabled, doorData.NorthEnabled, relatedRoomDoors.WestEnabled, relatedRoomDoors.EastEnabled);
        }

        // Update adjacent south room
        relatedRoomId = roomId + Rows;
        if (relatedRoomId < Rooms.Count)
        {
            RoomDoorData relatedRoomDoors = Rooms[relatedRoomId];
            Rooms[relatedRoomId] = new RoomDoorData(doorData.SouthEnabled, relatedRoomDoors.SouthEnabled, relatedRoomDoors.WestEnabled, relatedRoomDoors.EastEnabled);
        }

        // Update adjacent west room
        relatedRoomId = roomId - 1;
        if (relatedRoomId >= 0)
        {
            RoomDoorData relatedRoomDoors = Rooms[relatedRoomId];
            Rooms[relatedRoomId] = new RoomDoorData(relatedRoomDoors.NorthEnabled, relatedRoomDoors.SouthEnabled, relatedRoomDoors.WestEnabled, doorData.WestEnabled);
        }

        // Update adjacent east room
        relatedRoomId = roomId + 1;
        if (relatedRoomId < Rooms.Count)
        {
            RoomDoorData relatedRoomDoors = Rooms[relatedRoomId];
            Rooms[relatedRoomId] = new RoomDoorData(relatedRoomDoors.NorthEnabled, relatedRoomDoors.SouthEnabled, doorData.EastEnabled, relatedRoomDoors.EastEnabled);
        }
    }

    private int GetRoomIdFromCoordinates(int row, int column)
    {
        return row * Rows + column;
    }
}