using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    // Load Level 1
    public void StartGame()
    {
        SceneManager.LoadScene("Level 1");
    }

    // Load Level 2
    public void LoadLevel2()
    {
        SceneManager.LoadScene("Level 2");
    }

    // Load Level 3
    public void LoadLevel3()
    {
        SceneManager.LoadScene("Level 3");
    }

    // Show Controls Panel
    public GameObject controlsPanel;
    public void OpenControls()
    {
        controlsPanel.SetActive(true);
    }

    // Hide Controls Panel
    public void CloseControls()
    {
        controlsPanel.SetActive(false);
    }

    // Quit game
    public void QuitGame()
    {
        Application.Quit();
    }
}
