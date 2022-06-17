using UnityEngine;

[CreateAssetMenu(fileName = "RoomElementData", menuName = "ScriptableObjects/RoomElementDataScriptableObject", order = 1)]
public class RoomElementDataScriptableObject : ScriptableObject
{
    public GameObject EnemyPrefab;
    public GameObject CupcakePrefab;
    public GameObject LadderPrefab;
}
