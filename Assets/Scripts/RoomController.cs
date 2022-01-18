using System.Collections.Generic;
using UnityEngine;

public class RoomController : MonoBehaviour
{
    [SerializeField]
    private RoomDataScriptableObject RoomData;
    [System.NonSerialized]
    public List<GameObject> InteractableList;

    public void Awake()
    {
        InteractableList = new List<GameObject>();

        GameObject spawnedInteractable = Instantiate(RoomData.EnemyPrefab);
        spawnedInteractable.transform.position = new Vector2(-3, 2);
        InteractableList.Add(spawnedInteractable);

        spawnedInteractable = Instantiate(RoomData.EnemyPrefab);
        spawnedInteractable.transform.position = new Vector2(3, 2);
        InteractableList.Add(spawnedInteractable);

        spawnedInteractable = Instantiate(RoomData.CupcakePrefab);
        spawnedInteractable.transform.position = new Vector2(4, 1);
        InteractableList.Add(spawnedInteractable);
    }
}