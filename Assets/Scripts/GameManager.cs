using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public GameObject gameOverPanel;
    public float gameSpeed = 10f;

    private float time;
    private bool isGameOver = false;

    void Update()
    {
        if (!isGameOver)
        {
            time += Time.deltaTime;
            timerText.text = time.ToString("F1");
        }
    }

    public void GameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;

        gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}