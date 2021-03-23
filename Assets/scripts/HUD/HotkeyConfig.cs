using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Hokey Config", menuName = "Hokey Config")]
public class HotkeyConfig : ScriptableObject
{
    [SerializeField] string duplicates = "";
    [Header("Keys")]
    public KeyCode pitchDown = KeyCode.W;
    public KeyCode pitchUp = KeyCode.S;
    public KeyCode rollRight = KeyCode.D;
    public KeyCode rollLeft = KeyCode.A;
    public KeyCode useNitro = KeyCode.Space;
    public KeyCode noobModeToggle = KeyCode.P;
    public KeyCode launchFromPlatform = KeyCode.L;
    public KeyCode pauseGame = KeyCode.Escape;
    public KeyCode toggleHUD = KeyCode.F1;
    public KeyCode takeScreenshot = KeyCode.F2;
    public KeyCode debugMode = KeyCode.F3;
    public KeyCode respawn = KeyCode.Return;

    private void OnValidate()
    {
        duplicates = "";
        KeyCode[] keys =
        {
            pitchDown,
            pitchUp,
            rollRight,
            rollLeft,
            useNitro,
            noobModeToggle,
            launchFromPlatform,
            pauseGame,
            toggleHUD,
            takeScreenshot,
            debugMode,
        };
        if (Array.FindAll<KeyCode>(keys, key => key == pitchDown).Length > 1)
        {
            duplicates += "Pitch Down, ";
        }
        if (Array.FindAll<KeyCode>(keys, key => key == pitchUp).Length > 1)
        {
            duplicates += "Pitch Up, ";
        }
        if (Array.FindAll<KeyCode>(keys, key => key == rollRight).Length > 1)
        {
            duplicates += "Roll Right, ";
        }
        if (Array.FindAll<KeyCode>(keys, key => key == rollLeft).Length > 1)
        {
            duplicates += "Roll Left, ";
        }
        if (Array.FindAll<KeyCode>(keys, key => key == useNitro).Length > 1)
        {
            duplicates += "Use Nitro, ";
        }
        if (Array.FindAll<KeyCode>(keys, key => key == noobModeToggle).Length > 1)
        {
            duplicates += "Noob Mode Toggle, ";
        }
        if (Array.FindAll<KeyCode>(keys, key => key == launchFromPlatform).Length > 1)
        {
            duplicates += "Launch From Platform, ";
        }
        if (Array.FindAll<KeyCode>(keys, key => key == pauseGame).Length > 1)
        {
            duplicates += "Pause Game, ";
        }
        if (Array.FindAll<KeyCode>(keys, key => key == toggleHUD).Length > 1)
        {
            duplicates += "Toggle HUD, ";
        }
        if (Array.FindAll<KeyCode>(keys, key => key == takeScreenshot).Length > 1)
        {
            duplicates += "Take Screenshot, ";
        }
        if (Array.FindAll<KeyCode>(keys, key => key == debugMode).Length > 1)
        {
            duplicates += "Debug Mode, ";
        }
        if (Array.FindAll<KeyCode>(keys, key => key == respawn).Length > 1)
        {
            duplicates += "Respawn, ";
        }

        duplicates = duplicates != "" ? duplicates.Remove(duplicates.Length - 2, 1) : "";
    }
}
