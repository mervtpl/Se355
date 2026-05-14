using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        Playing,
        LevelComplete,
        GameOver
    }

    public static GameManager Instance { get; private set; }

    public UIManager uiManager;
    public LevelManager levelManager;
    public PlayerHealth playerHealth;

    public GameState CurrentState { get; private set; } = GameState.Playing;
    public int TotalEnemiesDefeated { get; private set; }
    public bool IsGameOver => CurrentState == GameState.GameOver;
    public bool AllowsGameplayActions => CurrentState == GameState.Playing;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Time.timeScale = 1f;
    }

    private void Start()
    {
        if (uiManager == null)
        {
            uiManager = FindFirstObjectByType<UIManager>();
        }

        if (levelManager == null)
        {
            levelManager = FindFirstObjectByType<LevelManager>();
        }

        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        RefreshUI();
    }

    private void Update()
    {
        if (!IsGameOver || Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartScene();
        }
    }

    public void RegisterUI(UIManager manager)
    {
        if (manager == null)
        {
            return;
        }

        uiManager = manager;
        RefreshUI();
    }

    public void RegisterLevelManager(LevelManager manager)
    {
        if (manager != null)
        {
            levelManager = manager;
        }
    }

    public void RegisterPlayer(PlayerHealth player)
    {
        if (player != null)
        {
            playerHealth = player;
        }

        RefreshUI();
    }

    public void OnPlayerHealthChanged(PlayerHealth player)
    {
        if (player != null)
        {
            playerHealth = player;
        }

        if (uiManager != null && playerHealth != null)
        {
            uiManager.UpdateHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }
    }

    public void OnLevelStarted(int level, int enemiesRemaining)
    {
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;

        if (uiManager == null)
        {
            uiManager = FindFirstObjectByType<UIManager>();
        }

        uiManager?.UpdateLevel(level);
        uiManager?.UpdateEnemiesRemaining(enemiesRemaining);
        uiManager?.ShowLevelComplete(false);
        uiManager?.ShowGameOver(false);
        uiManager?.SetRestartVisible(false);
    }

    public void OnEnemyCountChanged(int enemiesRemaining)
    {
        uiManager?.UpdateEnemiesRemaining(enemiesRemaining);
    }

    public void NotifyEnemyDefeated()
    {
        TotalEnemiesDefeated += 1;
    }

    public void OnLevelComplete()
    {
        if (IsGameOver)
        {
            return;
        }

        CurrentState = GameState.LevelComplete;
        uiManager?.ShowLevelComplete(true);
    }

    public void HandlePlayerDeath()
    {
        if (IsGameOver)
        {
            return;
        }

        CurrentState = GameState.GameOver;
        Time.timeScale = 0f;

        uiManager?.ShowGameOver(true);
        uiManager?.SetRestartVisible(true);
    }

    private void RefreshUI()
    {
        if (uiManager == null)
        {
            return;
        }

        if (playerHealth != null)
        {
            uiManager.UpdateHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }

        if (levelManager != null)
        {
            uiManager.UpdateLevel(levelManager.currentLevel);
            uiManager.UpdateEnemiesRemaining(levelManager.enemiesAlive);
        }

        uiManager.ShowLevelComplete(CurrentState == GameState.LevelComplete);
        uiManager.ShowGameOver(CurrentState == GameState.GameOver);
        uiManager.SetRestartVisible(CurrentState == GameState.GameOver);
    }

    private void RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
