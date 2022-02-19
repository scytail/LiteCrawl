using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : CreatureController
{
    [SerializeField]
    private GameObject HealthUIText;

    public void Awake()
    {
        UpdateUI();
    }

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
        UpdateUI();
    }

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);
        UpdateUI();
    }
    public override void GainHealth(int amount)
    {
        base.GainHealth(amount);
        UpdateUI();
    }

    private void UpdateUI()
    {
        HealthUIText.GetComponent<TextMeshProUGUI>().text = $"Health: {CurrentHealth}/{MaxHealth}";
    }

    public void OnDestroy()
    {
        SceneManager.LoadScene(2);
    }
}
