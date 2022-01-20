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
        MoveSelector();
    }
    public void Left(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            if (LevelController.MoveToNewRoom(MoveDirection.Left))
            {
                Camera.transform.position = new Vector3(LevelController.CurrentRoom.transform.position.x,
                                                        LevelController.CurrentRoom.transform.position.y,
                                                        Camera.transform.position.z);
                ResetSelector();
            }
        }
    }
    public void Right(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            if (LevelController.MoveToNewRoom(MoveDirection.Right))
            {
                Camera.transform.position = new Vector3(LevelController.CurrentRoom.transform.position.x,
                                                        LevelController.CurrentRoom.transform.position.y, 
                                                        Camera.transform.position.z);
                ResetSelector();
            }
        }
    }
    public void Up(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            if (LevelController.MoveToNewRoom(MoveDirection.Up))
            {
                Camera.transform.position = new Vector3(LevelController.CurrentRoom.transform.position.x,
                                                        LevelController.CurrentRoom.transform.position.y,
                                                        Camera.transform.position.z);
                ResetSelector();
            }
        }
    }
    public void Down(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            if (LevelController.MoveToNewRoom(MoveDirection.Down))
            {
                Camera.transform.position = new Vector3(LevelController.CurrentRoom.transform.position.x,
                                                        LevelController.CurrentRoom.transform.position.y,
                                                        Camera.transform.position.z);
                ResetSelector();
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
            Player.GetComponent<PlayerController>().Interact(new List<GameObject> { roomInteractableList[SelectedTargetIndex]});
            DoEnemyTurns();
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

    private void MoveSelector()
    {
        List<GameObject> roomInteractableList = LevelController.CurrentRoom.GetComponent<RoomController>().InteractableList;
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

    private void DoEnemyTurns()
    {
        List<GameObject> roomInteractableList = LevelController.CurrentRoom.GetComponent<RoomController>().InteractableList;
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
