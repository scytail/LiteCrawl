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

    public virtual void TakeDamage(int amount)
    {
        CurrentHealth -= amount;

        if (CurrentHealth <= 0)
        {
            DestroySelf();
        }
    }

    public virtual void GainHealth(int amount)
    {
        CurrentHealth += amount;
        
        if  (CurrentHealth > MaxHealth)
        {
            CurrentHealth = MaxHealth;
        }
    }

    protected void Attack(GameObject target)
    {
        target.GetComponent<CreatureController>().TakeDamage(BaseDamage);
    }
    protected void PickUp(GameObject target)
    {
        target.GetComponent<ItemController>().Use(gameObject);
    }
}
