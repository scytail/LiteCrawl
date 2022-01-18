using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    [SerializeField]
    private GameObject Player;
    [SerializeField]
    private GameObject Selector;
    [SerializeField]
    private RoomDataScriptableObject RoomData;
    [SerializeField]
    private Vector2Int LevelDimensions;
    
    private List<List<GameObject>> RoomGrid;
    private int SelectedTargetIndex = 0;
    private Vector2Int CurrentLocation;
    private GameObject CurrentRoom { get
        {
            return RoomGrid[CurrentLocation.x][CurrentLocation.y];
        } 
    }

    #region Unity Events
    public void Awake()
    {
        GenerateRooms();
    }
    public void Left(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            Debug.Log("Left");
            ResetSelector();
        }
    }
    public void Right(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            Debug.Log("Right");
            ResetSelector();
        }
    }
    public void Up(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            Debug.Log("Up");
            ResetSelector();
        }
    }
    public void Down(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            Debug.Log("Down");
            ResetSelector();
        }
    }
    public void ChangeTargets(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            List<GameObject> roomInteractableList = CurrentRoom.GetComponent<RoomController>().InteractableList;
            if (roomInteractableList.Count > 0)
            {
                SelectedTargetIndex = (SelectedTargetIndex + 1) % roomInteractableList.Count;
                MoveSelector();
            }
        }
    }
    public void Select(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            List<GameObject> roomInteractableList = CurrentRoom.GetComponent<RoomController>().InteractableList;
            Player.GetComponent<PlayerController>().Interact(new List<GameObject> { roomInteractableList[SelectedTargetIndex]});
            DoEnemyTurns();
        }
    }
    #endregion

    #region Game Control
    public void Kill(GameObject target)
    {
        List<GameObject> roomInteractableList = CurrentRoom.GetComponent<RoomController>().InteractableList;
        roomInteractableList.Remove(target);
        Destroy(target);
        SelectedTargetIndex = 0;
        MoveSelector();
    }

    private void MoveSelector()
    {
        List<GameObject> roomInteractableList = CurrentRoom.GetComponent<RoomController>().InteractableList;
        if (roomInteractableList.Count > 0)
        {
            Transform targetTransform = roomInteractableList[SelectedTargetIndex].transform;
            Selector.transform.position = new Vector2(targetTransform.position.x, targetTransform.position.y + 2);
        }
        else
        {
            Destroy(Selector);
        }
    }

    private void ResetSelector()
    {
        SelectedTargetIndex = 0;
        MoveSelector();
    }

    private void GenerateRooms()
    {
        RoomGrid = new List<List<GameObject>>();
        for (int x = 0; x < LevelDimensions.x; x++)
        {
            RoomGrid.Add(new List<GameObject>());
            for (int y = 0; y < LevelDimensions.y; y++)
            {
                int roomToggle = Random.Range(0, 2);
                if (roomToggle == 0)
                {
                    RoomGrid[x].Add(Instantiate(RoomData.FoodRoomPrefab));
                }
                else
                {
                    RoomGrid[x].Add(Instantiate(RoomData.BasicRoomPrefab));
                }
                RoomGrid[x][y].transform.position = new Vector2(x, y);
            }
        }
    }

    private void DoEnemyTurns()
    {
        List<GameObject> roomInteractableList = CurrentRoom.GetComponent<RoomController>().InteractableList;
        foreach (GameObject interactable in roomInteractableList)
        {
            if (interactable.tag == "Enemy")
            {
                List<GameObject> enemyTargets = interactable.GetComponent<EnemyController>().DetermineTargets(Player, roomInteractableList);
                interactable.GetComponent<CreatureController>().Interact(enemyTargets);
            }
        }
    }
    #endregion
}
