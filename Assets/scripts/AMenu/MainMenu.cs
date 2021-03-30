using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MenuState
{
    Main,
    Settings,
    Credits,
    Zen
}
public class MainMenu : MonoBehaviour
{
    public GliderController controller;
    public Text scoreText;
    public Text highScore;

    public void NewGame()
    {
        // get the seed from config
     }
    public void Quit()
    {
        Application.Quit();
    }

    [Header("display")]
    public GameObject main;
    public GameObject settings;
    public GameObject credits;
    public GameObject zen;
    public MenuState state;
    void Start()
    {
        settings.SetActive(false);
        main.SetActive(true);
        credits.SetActive(false);
        zen.SetActive(false);
        state = MenuState.Main;
    }
    void Update()
    {
        scoreText.text = controller.lastScore + "";
        highScore.text = controller.highScore + "";
    }
    public void Main()
    {
        settings.SetActive(false);
        main.SetActive(true);
        CanvasGroup temp = main.GetComponent<CanvasGroup>();
        if (temp != null) temp.alpha = 1;
        credits.SetActive(false);
        zen.SetActive(false);
    }
    public void Settings()
    {
        settings.SetActive(true);
        main.SetActive(false);
        credits.SetActive(false);
        zen.SetActive(false);
    }
    public void Credits()
    {
        settings.SetActive(false);
        main.SetActive(false);
        credits.SetActive(true);
        zen.SetActive(false);
    }
    public void Zen()
    {
        settings.SetActive(false);
        main.SetActive(false);
        credits.SetActive(false);
        zen.SetActive(true);
    }
}
