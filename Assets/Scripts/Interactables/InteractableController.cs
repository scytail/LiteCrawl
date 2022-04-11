using UnityEngine;

public abstract class InteractableController : MonoBehaviour
{
    [SerializeField]
    private int pointValue;

    private GameObject _gameController;
    protected GameObject GameManager { 
        get
        {
            if (!_gameController)
            {
                _gameController = GameObject.FindGameObjectWithTag("GameController");
            }

            return _gameController;
        } 
    }

    protected void DestroySelf()
    {
        GameManager.GetComponent<GameController>().Kill(gameObject);
    }
}
