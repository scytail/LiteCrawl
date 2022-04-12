using UnityEngine;

[CreateAssetMenu(fileName = "RoomData", menuName = "ScriptableObjects/RoomDataScriptableObject", order = 1)]
public class RoomDataScriptableObject : ScriptableObject
{
    // REMINDER: if you add a new room type here, make sure you update the Room Type Enum and add it to the level generator
    public GameObject BasicRoomPrefab;
    public GameObject FoodRoomPrefab;
    public GameObject EmptyRoomPrefab;
    public GameObject DescentRoomPrefab;
}
