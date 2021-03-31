using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MenuState
{
    Main,
    Settings,
    Credits,
    Zen,
    Help,
    Seed
}
public class MainMenu : MonoBehaviour
{
    public GliderController controller;
    public Text scoreText;
    public Text highScore;
    public Text seedText;
    public int seedVal = 0;
    public SettingsConfig config;
    public TerrainGenerator generator;

    public void takeSeed()
    {
        string check = seedText.text;
        check.Substring(0, Mathf.Min(check.Length, 10));
        int temp = 0;
        bool isNumeric = int.TryParse(check, out temp);
        if (isNumeric) seedVal = temp;
        else seedVal = check.GetHashCode();
        config.seed = seedVal;
        generator.ClearAllTerrain();
    }
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
    public GameObject help;
    public GameObject seed;
    public GameObject fadeIn;
    public GameObject keyboard;
    void Start()
    {
        settings.SetActive(false);
        main.SetActive(true);
        credits.SetActive(false);
        zen.SetActive(false);
        help.SetActive(false);
        seed.SetActive(false);
        keyboard.SetActive(false);
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
        CanvasGroup temp = fadeIn.GetComponent<CanvasGroup>();
        if (temp != null) temp.alpha = 1;
        credits.SetActive(false);
        zen.SetActive(false);
        help.SetActive(false);
        seed.SetActive(false);
    }
    public void Keyboard()
    {
        help.SetActive(false);
        keyboard.SetActive(true);
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
    public void Help()
    {
        main.SetActive(false);
        keyboard.SetActive(false);
        help.SetActive(true);
    }
    public void Seed()
    {
        main.SetActive(false);
        seed.SetActive(true);
    }
}
