using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> RoomInteractableList;
    [SerializeField]
    private GameObject Player;
    [SerializeField]
    private GameObject SelectorIndicator;

    private int SelectedTargetIndex = 0;

    #region Unity Events
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
        if (context.phase == InputActionPhase.Performed && RoomInteractableList.Count > 0)
        {
            SelectedTargetIndex = (SelectedTargetIndex + 1) % RoomInteractableList.Count;
            MoveSelector();
        }
    }
    public void Select(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            Player.GetComponent<PlayerController>().Interact(new List<GameObject> {RoomInteractableList[SelectedTargetIndex]});
            DoEnemyTurns();
        }
    }
    #endregion

    #region Game Control
    public void Kill(GameObject target)
    {
        RoomInteractableList.Remove(target);
        Destroy(target);
        SelectedTargetIndex = 0;
        MoveSelector();
    }

    private void MoveSelector()
    {
        if (RoomInteractableList.Count > 0)
        {
            Transform targetTransform = RoomInteractableList[SelectedTargetIndex].transform;
            SelectorIndicator.transform.position = new Vector2(targetTransform.position.x, targetTransform.position.y + 2);
        }
        else
        {
            Destroy(SelectorIndicator);
        }
    }

    private void ResetSelector()
    {
        SelectedTargetIndex = 0;
        MoveSelector();
    }

    private void DoEnemyTurns()
    {
        foreach(GameObject interactable in RoomInteractableList)
        {
            if (interactable.tag == "Enemy")
            {
                List<GameObject> enemyTargets = interactable.GetComponent<EnemyController>().DetermineTargets(Player, RoomInteractableList);
                interactable.GetComponent<CreatureController>().Interact(enemyTargets);
            }
        }
    }
    #endregion
}
