using System.Collections.Generic;
using UnityEngine;

public class RoomController : MonoBehaviour
{
    [SerializeField]
    private RoomElementDataScriptableObject RoomElementData;
    [SerializeField]
    private bool ShouldContainLadder;
    [SerializeField]
    private Vector2Int NumberOfEnemies;
    [SerializeField]
    private Vector2Int NumberOfCupcakes;
    public GameObject NorthDoor;
    public GameObject SouthDoor;
    public GameObject WestDoor;
    public GameObject EastDoor;

    [System.NonSerialized]
    public List<GameObject> InteractableList;

    public void Awake()
    {
        InteractableList = new List<GameObject>();

        int numberOfEnemies = Random.Range(NumberOfEnemies.x, NumberOfEnemies.y + 1);
        GameObject spawnedInteractable;

        if (ShouldContainLadder)
        {
            spawnedInteractable = Instantiate(RoomElementData.LadderPrefab);
            spawnedInteractable.transform.parent = gameObject.transform;
            spawnedInteractable.transform.localPosition = new Vector2(0, 1);
            InteractableList.Add(spawnedInteractable);
        }

        for (int enemyCounter = 0; enemyCounter < numberOfEnemies; enemyCounter++)
        {
            spawnedInteractable = Instantiate(RoomElementData.EnemyPrefab);
            spawnedInteractable.transform.parent = gameObject.transform;
            spawnedInteractable.transform.localPosition = new Vector2(-3 + enemyCounter * 1.5f, 2);
            InteractableList.Add(spawnedInteractable);
        }

        int numberOfCupcakes = Random.Range(NumberOfCupcakes.x, NumberOfCupcakes.y + 1);
        for (int cupcakeCounter = 0; cupcakeCounter < numberOfCupcakes; cupcakeCounter++)
        {
            spawnedInteractable = Instantiate(RoomElementData.CupcakePrefab);
            spawnedInteractable.transform.parent = gameObject.transform;
            spawnedInteractable.transform.localPosition = new Vector2(3, -1 + cupcakeCounter * 1.5f);
            InteractableList.Add(spawnedInteractable);
        }
    }
}