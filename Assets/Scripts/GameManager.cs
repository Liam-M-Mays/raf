using UnityEngine;
using UnityEngine.SceneManagement;

/// Singleton Game Manager that handles game over and other global game states
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    /*

    [Header("Game Over Settings")]
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private float gameOverDelay = 1f;
    
    private bool isGameOver = false;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Hide game over UI at start
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
    }

    /// Called when the raft dies
    */
    public void TriggerGameOver()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
    }
    /*

    void ShowGameOver()
    {
        // Pause the game
        Time.timeScale = 0f;
        
        // Show game over UI
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
    }

    /// Restart the current scene
    public void RestartGame()
    {
        Time.timeScale = 1f;
        isGameOver = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// Load main menu (make sure you have a scene named "MainMenu")
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        isGameOver = false;
        SceneManager.LoadScene("MainMenu");
    }

    /// Quit the game
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public bool IsGameOver() => isGameOver;
    */
}