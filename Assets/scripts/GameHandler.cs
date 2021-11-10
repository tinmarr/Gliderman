using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum State
{
    Menu,
    Game,
    Pause
}
public class GameHandler : MonoBehaviour
{
    State state;

    [Header("Menu Items")]
    public GameObject HUD;
    public GameObject Menu;
    public GameObject Pause;

    [Header("Glider")]
    public GliderController glider;

    [Header("Terrain Generation")]
    public TerrainGenerator terrain; 
    public HeightMapSettings[] biomes;
    public SettingsConfig settings;

    void Start()
    {
        glider.input.actions["Quit"].performed += _ => Quit();
        glider.input.actions["Pause"].performed += _ => ToggleRunState();

        ResetLevel();

        HUD.SetActive(false);
        Menu.SetActive(true);
        Pause.SetActive(false);

        state = State.Menu;

        glider.input.SwitchCurrentActionMap("Menu");
        glider.EnableAutoFly();
    }

    void Update()
    {
        if (glider.dead)
        {
            ActivateMenu();
        }
    }

    public void ToggleHud()
    {
        HUD.SetActive(!HUD.activeSelf);
    }

    public void Begin()
    {
        state = State.Game;
        glider.input.SwitchCurrentActionMap("Game");
        
        HUD.SetActive(true);
        Menu.SetActive(false);

        glider.DisableAutoFly();
        StartCoroutine(glider.AddToScore());
    }

    public void ActivateMenu()
    {
        HUD.SetActive(false);
        Pause.SetActive(false);
        Menu.SetActive(true);

        Time.timeScale = 1;
        state = State.Menu;

        ResetLevel();

        glider.Respawn();

        glider.input.SwitchCurrentActionMap("Menu");
        glider.EnableAutoFly();
    }

    public void Quit()
    {
        // possibly saving
        if (state == State.Menu)
        {
            Application.Quit();
        }
    }

    public void ToggleRunState()
    {
        if (state == State.Game)
        {
            state = State.Pause;
            Pause.SetActive(true);
            HUD.SetActive(false);
            Time.timeScale = 0;
        } else if (state == State.Pause)
        {
            Pause.SetActive(false);
            HUD.SetActive(true);
            Time.timeScale = 1;
            state = State.Game;
        }
    }

    public void ResetLevel()
    {
        int seedVal = Random.Range(-10000, 10000);
        settings.seed = seedVal;
        HeightMapSettings nextBiome = biomes.Length == 0 ? terrain.heightMapSettings : biomes[Random.Range(0, biomes.Length - 1)];
        terrain.heightMapSettings = nextBiome;
        terrain.ClearAllTerrain();
    }
}
