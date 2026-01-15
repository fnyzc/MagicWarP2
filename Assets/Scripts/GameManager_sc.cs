using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager_sc : MonoBehaviour
{
    public static GameManager_sc Instance;

    [Header("References")]
    public Health_sc playerHealth;
    public Health_sc enemyHealth;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public GameObject winTextObject;
    public GameObject loseTextObject;

    private bool gameOver = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (playerHealth != null)
            playerHealth.OnDeath += OnPlayerDeath;

        if (enemyHealth != null)
            enemyHealth.OnDeath += OnEnemyDeath;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnDeath -= OnPlayerDeath;

        if (enemyHealth != null)
            enemyHealth.OnDeath -= OnEnemyDeath;
    }

    void OnPlayerDeath()
    {
        if (gameOver) return;
        gameOver = true;

        ShowGameOver(win: false);
    }

    void OnEnemyDeath()
    {
        if (gameOver) return;
        gameOver = true;

        ShowGameOver(win: true);
    }

    void ShowGameOver(bool win)
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (winTextObject != null)
            winTextObject.SetActive(win);

        if (loseTextObject != null)
            loseTextObject.SetActive(!win);

        Time.timeScale = 0f;
    }

    public void OnClickMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void OnClickRestart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }
}
