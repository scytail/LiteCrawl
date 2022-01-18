using System.Collections.Generic;
using UnityEngine;

public class EnemyController : CreatureController
{
    public List<GameObject> DetermineTargets(GameObject player, List<GameObject> otherInteractables)
    {
        return new List<GameObject> { player };
    }

    public override void Interact(List<GameObject> targetList)
    {
        foreach (GameObject target in targetList)
        {
            switch (target.tag)
            {
                case "Player":
                    Attack(target);
                    break;
                case "Pickup":
                    PickUp(target);
                    break;
            }
        }
    }
}
