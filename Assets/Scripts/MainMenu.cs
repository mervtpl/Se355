using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public GameObject pauseMenu;
    public void PlayGame()
    {
        SceneManager.LoadSceneAsync(1);
    }
    public void Home()
    {
        SceneManager.LoadSceneAsync(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            pauseMenu.SetActive(!pauseMenu.activeSelf);
        }
    }
}
