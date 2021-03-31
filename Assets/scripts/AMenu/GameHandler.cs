using Cinemachine;
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
    public CinemachineVirtualCamera topDownCamera;

    [Header("display")]
    public GameObject HUD;
    public GameObject Menu;
    public GameObject Pause;
    public GameObject Reef;
    public Fader fadeSystem;

    [Header("glider Init")]
    public GliderController glider;
    public StartPad launchPad;

    [Header("sounds")]
    public SoundManager soundManager;
    string gameMusic = "inGame2";
    string closeMusic = "inGame1";

    void Start()
    {
        // god who knows
        // possibly do entry credits starting animation
        HUD.SetActive(false);
        Menu.SetActive(false);
        Pause.SetActive(false);
        Reef.SetActive(true);
        StartCoroutine(Starting());
    }
    IEnumerator Starting()
    {
        float time = 0;
        while (time < 3)
        {
            yield return new WaitForSeconds(0.3f);
            time += .3f;
        }
        Reef.SetActive(false);
        Menu.SetActive(true);
        fadeSystem.turnOff();
        fadeSystem.Fade();
        state = State.Menu;
        soundManager.Play("startMusic");
    }

    void Update()
    {
        if (state == State.Menu)
        {
            topDownCamera.Priority = 3;
            glider.SetNothing(true);
            if (Input.GetKeyDown(KeyCode.Space))
            {
                glider.activateMenuPlease = false;
                fadeSystem.StopFadeIn();
                topDownCamera.Priority = 1;
                state = State.Game;
                StartCoroutine(StartGame());
            }
            if (Input.GetKeyDown(hotkeys.exitGame))
            {
                Quit();
            }
            if (glider.activateMenuPlease == true)
            {
                ActivateMenu();
                glider.activateMenuPlease = false;
            }
        }
        else if(state == State.Game)
        {
            if (Input.GetKeyDown(hotkeys.pauseGame))
            {
                state = State.Pause;
                PauseGame();
            }
            if(glider.activateMenuPlease == true)
            {
                glider.activateMenuPlease = false;
                state = State.Menu;
                HUD.SetActive(false);
                ActivateMenu();
            }
            if (Input.GetKeyDown(hotkeys.toggleHUD))
            {
                HUD.SetActive(!HUD.activeSelf);
            }
        }
        else if(state == State.Pause)
        {
            if (Input.GetKeyDown(hotkeys.pauseGame))
            {
                ResumeGame();
                state = State.Game;
            }
        }
        // start in the "menu" which is the normal scene looking down?
        // If in menu then wait for space to start escape to exit etc
        // If in game listen for esc pause
        // other stuffs?
    }
    public void ActivateMenu()
    {
        soundManager.FadeIn("startMusic", 1);
        soundManager.FadeOut(gameMusic, 2);
        soundManager.FadeOut(closeMusic, 1);
        HUD.SetActive(false);
        Pause.SetActive(false);
        Menu.SetActive(true);
        fadeSystem.turnOff();
        fadeSystem.Fade();
        Time.timeScale = 1;
        state = State.Menu;
        glider.Respawn();
    }
    IEnumerator StartGame()
    {
        soundManager.FadeOut("startMusic", 1);
        yield return new WaitForSeconds(1f);
        soundManager.Play(gameMusic, 0.5f);
        HUD.SetActive(true);
        Menu.SetActive(false);
        state = State.Game;
        yield return new WaitForSeconds(1f);
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
        Pause.SetActive(true);
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        Pause.SetActive(false);
        HUD.SetActive(true);
        Time.timeScale = 1;
        state = State.Game;
    }
}
