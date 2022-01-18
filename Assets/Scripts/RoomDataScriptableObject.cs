using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomData", menuName = "ScriptableObjects/RoomDataScriptableObject", order = 1)]
public class RoomDataScriptableObject : ScriptableObject
{
    public GameObject EnemyPrefab;
    public GameObject CupcakePrefab;
}
