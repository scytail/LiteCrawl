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