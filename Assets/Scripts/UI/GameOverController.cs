using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverController : MonoBehaviour
{
    [SerializeField]
    private GameObject ScoreText;

    public void Awake()
    {
        int score = PlayerPrefs.GetInt("score");

        ScoreText.GetComponent<TextMeshProUGUI>().text = $"Score: {score}";
    }

    public void OpenMainMenu()
    {
        SceneManager.LoadScene(0);
    }
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif

        Application.Quit();
    }
}
