using UnityEngine;

public abstract class ItemController : InteractableController
{
    public abstract int Use(GameObject creatureUsing);
}
