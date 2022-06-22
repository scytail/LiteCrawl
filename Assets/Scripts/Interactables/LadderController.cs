using UnityEngine;

public class LadderController : ItemController
{
    public override int Use(GameObject creatureUsing)
    {
        GameManager.GetComponent<GameController>().ResetLevel();
        return pointValue;
    }
}
