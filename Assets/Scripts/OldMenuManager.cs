using UnityEngine;
using UnityEngine.SceneManagement;

public class OldMenuManager : MonoBehaviour
{
    // Load Level 1
    public void StartGame()
    {
        SceneManager.LoadScene("Level 1");
    }

    // Quit game
    public void QuitGame()
    {
        Application.Quit();
    }
}
