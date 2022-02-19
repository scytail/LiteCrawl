using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public void OnDestroy()
    {
        SceneManager.LoadScene(2);
    }
}
