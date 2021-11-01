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
    public CinemachineVirtualCamera topDownCamera;

    [Header("Menu Items")]
    public GameObject HUD;
    public GameObject Menu;
    public GameObject Pause;

    [Header("glider Init")]
    public GliderController glider;
    public StartPad launchPad;

    [Header("Terrain Generation")]
    public TerrainGenerator terrain; 
    public HeightMapSettings[] biomes;
    public SettingsConfig settings;

    [Header("Other")]
    public GameObject startTerrain;

    void Start()
    {
        glider.input.actions["Continue"].performed += _ => Begin();
        glider.input.actions["Quit"].performed += _ => Quit();
        glider.input.actions["Pause"].performed += _ => ToggleRunState();

        HUD.SetActive(false);
        Menu.SetActive(true);
        Pause.SetActive(false);
        state = State.Menu;
        glider.input.SwitchCurrentActionMap("Menu");
    }

    void Update()
    {
        if (glider.IsDead())
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
        topDownCamera.Priority = 1;
        state = State.Game;

        glider.input.SwitchCurrentActionMap("Game");

        HUD.SetActive(true);
        Menu.SetActive(false);
        state = State.Game;
        launchPad.LaunchPlayer();
        glider.SetNothing(false);
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
        startTerrain.SetActive(true);
        glider.input.SwitchCurrentActionMap("Menu");
        glider.SetNothing(true);
    }

    public void Quit()
    {
        // possibly saving
        Application.Quit();
    }

    public void ToggleRunState()
    {
        if (state == State.Game)
        {
            state = State.Pause;
            Pause.SetActive(true);
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
        HeightMapSettings nextBiome = biomes[Random.Range(0, biomes.Length - 1)];
        terrain.heightMapSettings = nextBiome;
        terrain.ClearAllTerrain();
        topDownCamera.Priority = 3;
    }
}
