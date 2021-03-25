using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHandler : MonoBehaviour
{
    bool inMenu = true;
    public HotkeyConfig hotkeys;

    public GameObject HUD;
    public GameObject Menu;
    void Start()
    {
        // god who knows
        // possibly do entry credits starting animation
        HUD.SetActive(false);
        Menu.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (inMenu)
        {
            if (!Menu.activeSelf)
            {
                Menu.SetActive(true);
                HUD.SetActive(false);
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartGame();
            }
            if (Input.GetKeyDown(hotkeys.exitGame))
            {
                Quit();
            }
        }
        else
        {
            if (Menu.activeSelf)
            {
                HUD.SetActive(true);
                Menu.SetActive(false);
            }
        }
        // start in the "menu" which is the normal scene looking down?
        // If in menu then wait for space to start escape to exit etc
        // If in game listen for esc pause
        // other stuffs?
    }
    void StartGame()
    {
        inMenu = false;
    }
    public void Quit()
    {
        // possibly saving
        Application.Quit();
    }
}
