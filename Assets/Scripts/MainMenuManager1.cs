using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Names")]
    public string mainMenuScene = "Main Menu";
    public string gameplayScene = "Gameplay";
   

    // START GAME
    public void StartGame()
    {
        SceneManager.LoadScene(gameplayScene);
    }

   
    // GO TO MAIN MENU
    public void GoToMainMenu()
    {
        SceneManager.LoadScene(mainMenuScene);
    }

    // QUIT GAME
    public void QuitGame()
{
        Debug.Log("QUIT BUTTON PRESSED");

       #if UNITY_EDITOR
           UnityEditor.EditorApplication.isPlaying = false;
       #else
           Application.Quit();
       #endif
}
}