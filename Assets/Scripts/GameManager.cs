using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton Game Manager that handles game over and other global game states
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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

    /// <summary>Called when the raft dies - reload scene</summary>
    public void TriggerGameOver()
    {
        if (isGameOver) return; // Prevent multiple calls
        
        isGameOver = true;
        ShowGameOver();
    }

    void ShowGameOver()
    {
        // Pause the game
        Time.timeScale = 0f;
        
        // Show game over UI
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
        else
        {
            Debug.LogWarning("GameManager: gameOverUI is not assigned. Reloading scene immediately.");
        }

        // Reload after delay
        Invoke(nameof(RestartGame), gameOverDelay);
    }

    /// <summary>Restart the current scene</summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        isGameOver = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>Load main menu (make sure you have a scene named "MainMenu")</summary>
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        isGameOver = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    /// <summary>Quit the game</summary>
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public bool IsGameOver() => isGameOver;
}