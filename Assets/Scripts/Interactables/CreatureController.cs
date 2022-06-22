using System.Collections.Generic;
using UnityEngine;

public abstract class CreatureController : InteractableController
{
    [SerializeField]
    protected int CurrentHealth;
    [SerializeField]
    protected int MaxHealth;
    [SerializeField]
    protected int BaseDamage;

    public abstract void Interact(List<GameObject> targetList);

    public virtual int TakeDamage(int amount)
    {
        CurrentHealth -= amount;

        if (CurrentHealth <= 0)
        {
            DestroySelf();
            return pointValue;
        }

        return 0;
    }

    public virtual void GainHealth(int amount)
    {
        CurrentHealth += amount;
        
        if  (CurrentHealth > MaxHealth)
        {
            CurrentHealth = MaxHealth;
        }
    }

    protected int Attack(GameObject target)
    {
        return target.GetComponent<CreatureController>().TakeDamage(BaseDamage);
    }
    protected int PickUp(GameObject target)
    {
        return target.GetComponent<ItemController>().Use(gameObject);
    }
}
