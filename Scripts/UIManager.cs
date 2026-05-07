using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text healthText;
    public Text levelText;
    public Text enemiesRemainingText;
    public Text levelCompleteText;
    public Text gameOverText;
    public Text restartText;

    private void Start()
    {
        GameManager.Instance?.RegisterUI(this);
        ShowLevelComplete(false);
        ShowGameOver(false);
        SetRestartVisible(false);
    }

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = "Health: " + currentHealth + "/" + maxHealth;
        }
    }

    public void UpdateLevel(int level)
    {
        if (levelText != null)
        {
            levelText.text = "Level: " + level;
        }
    }

    public void UpdateEnemiesRemaining(int enemiesRemaining)
    {
        if (enemiesRemainingText != null)
        {
            enemiesRemainingText.text = "Enemies Remaining: " + enemiesRemaining;
        }
    }

    public void ShowLevelComplete(bool visible)
    {
        if (levelCompleteText != null)
        {
            levelCompleteText.enabled = visible;
        }
    }

    public void ShowGameOver(bool visible)
    {
        if (gameOverText != null)
        {
            gameOverText.enabled = visible;
        }
    }

    public void SetRestartVisible(bool visible)
    {
        if (restartText != null)
        {
            restartText.enabled = visible;
        }
    }
}
