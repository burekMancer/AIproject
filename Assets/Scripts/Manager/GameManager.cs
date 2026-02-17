using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public enum GameState
    {
        Gameplay,
        Paused,
        GameOver,
        Victory
    }

    public GameState prevState;
    public GameState currentState;

    //Screens
    public GameObject pauseScreen;
    public GameObject deathScreen;
    public GameObject victoryScreen;


    public bool isGameOver = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("Extra " + this + " deleted");
            Destroy(gameObject);
        }

        DisableScreen();
    }


    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case GameState.Gameplay:
                CheckForPause();

                break;
            case GameState.Paused:
                CheckForPause();
                break;
            case GameState.GameOver:
                if (!isGameOver)
                {
                    isGameOver = true;
                    Time.timeScale = 0f;
                }

                break;
            case GameState.Victory:
            {
                isGameOver = true;
                Time.timeScale = 0.5f;
            }


                break;
        }
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;
    }

    public void Pause()
    {
        if (currentState != GameState.Paused)
        {
            prevState = currentState;
            ChangeState(GameState.Paused);
            Time.timeScale = 0f;
            pauseScreen.SetActive(true);
        }
    }

    public void Resume()
    {
        if (currentState == GameState.Paused)
        {
            ChangeState(prevState);
            Time.timeScale = 1f;
            pauseScreen.SetActive(false);
        }
    }

    void CheckForPause()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Paused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    void DisableScreen()
    {
        pauseScreen.SetActive(false);
    }

    public void GameOver()
    {
        ChangeState(GameState.GameOver);
        ShowDeathScreen();
    }

    private void ShowDeathScreen()
    {
        deathScreen.SetActive(true);
        Cursor.visible = true;
    }

    public void Victory()
    {
        ChangeState(GameState.Victory);
        ShowVictoryScreen();
    }

    private void ShowVictoryScreen()
    {
        victoryScreen.SetActive(true);
        Cursor.visible = true;
    }
}