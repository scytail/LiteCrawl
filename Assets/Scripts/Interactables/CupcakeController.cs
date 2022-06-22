using UnityEngine;

public class CupcakeController : ItemController
{
    [SerializeField]
    private int HealthGainAmount;

    public override int Use(GameObject creatureUsing)
    {
        creatureUsing.GetComponent<CreatureController>().GainHealth(HealthGainAmount);
        DestroySelf();
        return pointValue;
    }
}
