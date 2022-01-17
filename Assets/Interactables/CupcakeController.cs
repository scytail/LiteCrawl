using UnityEngine;

public class CupcakeController : ItemController
{
    [SerializeField]
    private int HealthGainAmount;

    public override void Use(GameObject creatureUsing)
    {
        creatureUsing.GetComponent<CreatureController>().GainHealth(HealthGainAmount);
        DestroySelf();
    }
}
