using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum State
{
    Menu,
    Game,
    Pause
}
public class GameHandler : MonoBehaviour
{
    public HotkeyConfig hotkeys;
    State state;

    [Header("display")]
    public GameObject HUD;
    public GameObject Menu;
    public GameObject Pause;

    [Header("glider Init")]
    public GliderController glider;
    public StartPad launchPad;
    void Start()
    {
        // god who knows
        // possibly do entry credits starting animation
        HUD.SetActive(false);
        Menu.SetActive(true);
        Pause.SetActive(false);
        state = State.Menu;
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.Menu)
        {
            glider.SetNothing(true);
            if (!Menu.activeSelf)
            {
                Menu.SetActive(true);
                HUD.SetActive(false);
                Pause.SetActive(false);
            }
            if (Input.GetKeyDown(KeyCode.Space))
            { 
                StartGame();
            }
            if (Input.GetKeyDown(hotkeys.exitGame))
            {
                Quit();
            }
            if (glider.activateMenuPlease == true)
            {
                glider.activateMenuPlease = false;
            }
        }
        else if(state == State.Game)
        {
            if (Menu.activeSelf)
            {
                HUD.SetActive(true);
                Menu.SetActive(false);
                Pause.SetActive(false);
            }
            if (Input.GetKeyDown(hotkeys.pauseGame))
            {
                state = State.Pause;
                PauseGame();
            }
            if(glider.activateMenuPlease == true)
            {
                glider.activateMenuPlease = false;
                state = State.Menu;
            }
        }
        else if(state == State.Pause)
        {
            HUD.SetActive(false);
            Menu.SetActive(false);
            Pause.SetActive(true);
        }
        // start in the "menu" which is the normal scene looking down?
        // If in menu then wait for space to start escape to exit etc
        // If in game listen for esc pause
        // other stuffs?
    }
    public void ActivateMenu()
    {
        Time.timeScale = 1;
        state = State.Menu;
        glider.Respawn();
    }
    void StartGame()
    {
        HUD.SetActive(true);
        Menu.SetActive(false);
        Pause.SetActive(false);
        state = State.Menu;
        state = State.Game;
        launchPad.LaunchPlayer();
        glider.SetNothing(false);
    }
    public void Quit()
    {
        // possibly saving
        Application.Quit();
    }
    public void PauseGame()
    {
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        Menu.SetActive(true);
        HUD.SetActive(false);
        Pause.SetActive(false);
        Time.timeScale = 1;
    }
}
