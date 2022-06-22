using System.Collections;

public class RoomDoorData
{
    public bool NorthEnabled;
    public bool SouthEnabled;
    public bool WestEnabled;
    public bool EastEnabled;

    public RoomDoorData()
    { }
    public RoomDoorData(bool north, bool south, bool west, bool east)
    {
        NorthEnabled = north;
        SouthEnabled = south;
        WestEnabled = west;
        EastEnabled = east;
    }

    public override string ToString()
    {
        // Take the four booleans and build an array,
        // then turn that array into it's corresponding byte number and return it as a string
        // Credit to https://stackoverflow.com/questions/40900788/how-do-i-convert-a-boolean-list-to-int
        BitArray bitField = new BitArray(new bool[] { NorthEnabled, SouthEnabled, WestEnabled, EastEnabled });
        byte[] bytes = new byte[1];
        bitField.CopyTo(bytes, 0);
        return bytes[0].ToString();
    }

    /// <summary>
    /// converts the string representation of a RoomDoorData class to its RoomDoorData equivalent. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="s">A string containing a RoomDoorData to convert.</param>
    /// <param name="result">When this method returns, contains the `RoomDoorData` value equivalent of the string contained in `s`, if the conversion succeeded, or `null` if the conversion failed. The conversion fails if the `s` parameter is `null` or `string.Empty`, is not of the correct format, or represents a number less than 0 or greater than 15. This parameter is passed uninitialized; any value originally supplied in result will be overwritten.</param>
    /// <returns>`true` if `s` was converted successfully; otherwise, `false`.</returns>
    public static bool TryParse(string s, out RoomDoorData result)
    {
        if (string.IsNullOrEmpty(s) ||
            !int.TryParse(s, out int encodedBitDataInteger) ||
            encodedBitDataInteger < 0 || 
            encodedBitDataInteger > 15)
        {
            result = null;
            return false;
        }

        BitArray bitField = new BitArray(new int[] { encodedBitDataInteger });

        result = new RoomDoorData(bitField.Get(0), bitField.Get(1), bitField.Get(2), bitField.Get(3));
        return true;
    }
}