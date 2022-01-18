using UnityEngine;

[CreateAssetMenu(fileName = "RoomData", menuName = "ScriptableObjects/RoomDataScriptableObject", order = 1)]
public class RoomDataScriptableObject : ScriptableObject
{
    public GameObject BasicRoomPrefab;
    public GameObject FoodRoomPrefab;
}
