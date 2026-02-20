using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class sceneManager : MonoBehaviour
{
    public GameObject tutorialScreen;

    public void MenuScene()
    {
        SceneManager.LoadScene("Menu");
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Game");
        Time.timeScale = 1f;
    }

    public void OpenTutorial()
    {
        tutorialScreen.SetActive(true);
    }

    public void CloseTutorial()
    {
        tutorialScreen.SetActive(false);
    }
}