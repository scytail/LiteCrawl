using System.Collections.Generic;
using UnityEngine;

public class PlayerController : CreatureController
{
    public override void Interact(List<GameObject> targetList)
    {
        foreach (GameObject target in targetList)
        {
            switch (target.tag)
            {
                case "Enemy":
                    Attack(target);
                    break;
                case "Pickup":
                    PickUp(target);
                    break;
            }
        }
    }
}
