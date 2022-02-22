using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderController : ItemController
{
    public override void Use(GameObject creatureUsing)
    {
        GameManager.GetComponent<GameController>().ResetLevel();
    }
}
