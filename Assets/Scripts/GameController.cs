using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    [SerializeField]
    private GameObject Camera;
    [SerializeField]
    private GameObject Player;
    [SerializeField]
    private GameObject Selector;
    
    private int SelectedTargetIndex = 0;
    private LevelController _levelController;
    private LevelController LevelController
    {
        get
        {
            if (!_levelController)
            {
                _levelController = gameObject.GetComponent<LevelController>();
            }
            return _levelController;
        }
    }
    

    #region Unity Events
    public void Awake()
    {
        ResetLevel();
    }
    public void Left(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            if (LevelController.MoveToNewRoom(MoveDirection.Left))
            {
                FocusCamera();
            }
        }
    }
    public void Right(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            if (LevelController.MoveToNewRoom(MoveDirection.Right))
            {
                FocusCamera();
            }
        }
    }
    public void Up(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            if (LevelController.MoveToNewRoom(MoveDirection.Up))
            {
                FocusCamera();
            }
        }
    }
    public void Down(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            if (LevelController.MoveToNewRoom(MoveDirection.Down))
            {
                FocusCamera();
            }
        }
    }
    public void ChangeTargets(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            List<GameObject> roomInteractableList = LevelController.CurrentRoom.GetComponent<RoomController>().InteractableList;
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
            List<GameObject> roomInteractableList = LevelController.CurrentRoom.GetComponent<RoomController>().InteractableList;
            if (roomInteractableList.Count > 0)
            {
                Player.GetComponent<PlayerController>().Interact(new List<GameObject> { roomInteractableList[SelectedTargetIndex]});
                DoEnemyTurns();
            }
        }
    }
    #endregion

    #region Game Control
    public void Kill(GameObject target)
    {
        List<GameObject> roomInteractableList = LevelController.CurrentRoom.GetComponent<RoomController>().InteractableList;
        roomInteractableList.Remove(target);
        Destroy(target);
        SelectedTargetIndex = 0;
        MoveSelector();
    }

    public void ResetLevel()
    {
        gameObject.GetComponent<LevelController>().ClearRooms();
        gameObject.GetComponent<LevelController>().GenerateRooms();
        FocusCamera();
    }

    private void MoveSelector()
    {
        List<GameObject> roomInteractableList = LevelController.CurrentRoom.GetComponent<RoomController>().InteractableList;
        if (roomInteractableList.Count > 0)
        {
            if (!Selector.GetComponent<Renderer>().enabled)
            {
                Selector.GetComponent<Renderer>().enabled = true;
            }
            Transform targetTransform = roomInteractableList[SelectedTargetIndex].transform;
            Selector.transform.position = new Vector2(targetTransform.position.x, targetTransform.position.y + 1);
        }
        else
        {
            Selector.GetComponent<Renderer>().enabled = false;
        }
    }

    private void ResetSelector()
    {
        SelectedTargetIndex = 0;
        MoveSelector();
    }

    private void FocusCamera()
    {
        // Move the camera
        Camera.transform.position = new Vector3(LevelController.CurrentRoom.transform.position.x,
                                                LevelController.CurrentRoom.transform.position.y,
                                                Camera.transform.position.z);
        // Reveal the room on the minimap
        LevelController.CurrentRoom.GetComponent<RoomController>().SetMinimapVisibility(true);
        // Set the selector
        ResetSelector();
    }

    private void DoEnemyTurns()
    {
        List<GameObject> roomInteractableList = LevelController.CurrentRoom.GetComponent<RoomController>().InteractableList;
        foreach (GameObject interactable in roomInteractableList)
        {
            if (interactable.CompareTag("Enemy"))
            {
                List<GameObject> enemyTargets = interactable.GetComponent<EnemyController>().DetermineTargets(Player, roomInteractableList);
                interactable.GetComponent<CreatureController>().Interact(enemyTargets);
            }
        }
    }
    #endregion
}
