using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public enum State
{
    Intro,
    Menu,
    Game,
    Pause
}

[RequireComponent(typeof(StatsTracker))]
public class GameHandler : MonoBehaviour
{
    public State state;

    public SettingsEditor settings;

    [Header("Menu Items")]
    public HUDController HUD;
    public GameObject Menu;
    public GameObject Pause;
    public GameObject Intro;

    [Header("Glider")]
    public GliderController glider;

    StatsTracker tracker;
    bool stateProcessed = false;

    void Start()
    {
        tracker = GetComponent<StatsTracker>();

        glider.input.actions["Quit"].performed += _ => Quit();
        glider.input.actions["Hud"].performed += _ => { HUD.gameObject.SetActive(!HUD.isWanted); HUD.isWanted = !HUD.isWanted; };
        glider.input.actions["Pause"].performed += _ => SetState(State.Pause);
        glider.input.actions["Resume"].performed += _ => SetState(State.Game);

        ResetLevel();

        SetState(State.Intro);
    }

    void Update()
    {
        if (glider.dead)
        {
            tracker.UpdateStats();
            tracker.UpdateBestStats();
            tracker.freeze = true;
            SetState(State.Menu);
        } 
        if (!stateProcessed)
        {
            if (state == State.Intro)
            {
                Intro.SetActive(true);
            } else if (state == State.Menu)
            {
                HUD.gameObject.SetActive(false);
                Pause.SetActive(false);
                Menu.SetActive(true);

                Time.timeScale = 1;

                ResetLevel();

                glider.Respawn();

                glider.input.SwitchCurrentActionMap("Menu");
                glider.EnableAutoFly();
            } else if (state == State.Game)
            {
                tracker.ResetStats();
                tracker.freeze = false;

                glider.input.SwitchCurrentActionMap("Game");

                HUD.gameObject.SetActive(HUD.isWanted);
                Menu.SetActive(false);
                Pause.SetActive(false);

                Time.timeScale = 1;

                glider.DisableAutoFly();

                if (glider.aliveSince == -1) { 
                    glider.aliveSince = Time.time;
                }
                
            } else if (state == State.Pause)
            {
                glider.input.SwitchCurrentActionMap("Paused");
                Pause.SetActive(true);
                HUD.gameObject.SetActive(false);
                Time.timeScale = 0;
            }
            stateProcessed = true;
        }
    }

    public void SetState(State state)
    {
        this.state = state;
        stateProcessed = false;
    }

    public void Begin() { SetState(State.Game);  }

    public void Quit()
    {
        // possibly saving
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Application.Quit();
    }

    public void ResetLevel()
    {
        if (settings.settings.randomSeed)
        {
            settings.SetSeed(System.DateTime.Now.ToString());
            settings.Updater();
        }
    }
}
