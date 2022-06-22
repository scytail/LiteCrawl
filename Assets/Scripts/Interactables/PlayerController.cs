using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : CreatureController
{
    [SerializeField]
    private GameObject HealthUIText;

    private int Score;

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
                    Score += Attack(target);
                    break;
                case "Pickup":
                    Score += PickUp(target);
                    break;
            }
        }
        UpdateUI();
    }

    public override int TakeDamage(int amount)
    {
        int pointValue = base.TakeDamage(amount);
        UpdateUI();
        return pointValue;
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
        PlayerPrefs.SetInt("score", Score);
        SceneManager.LoadScene(2);
    }
}
