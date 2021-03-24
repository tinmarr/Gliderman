using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Hokey Config", menuName = "Hokey Config")]
public class HotkeyConfig : ScriptableObject
{
    [SerializeField] string duplicates = "";
    [Header("Keys")]
    public KeyCode useNitro = KeyCode.Space;
    public KeyCode noobModeToggle = KeyCode.P;
    public KeyCode launchFromPlatform = KeyCode.L;
    public KeyCode pauseGame = KeyCode.E;
    public KeyCode toggleHUD = KeyCode.F1;
    public KeyCode takeScreenshot = KeyCode.F2;
    public KeyCode debugMode = KeyCode.F3;
    public KeyCode respawn = KeyCode.Return;
    public KeyCode brakes = KeyCode.LeftShift;
    public KeyCode exitGame = KeyCode.Escape;
    public KeyCode accept = KeyCode.Return;

    private void OnValidate()
    {
        duplicates = "";
        KeyCode[] keys =
        {
            useNitro,
            noobModeToggle,
            launchFromPlatform,
            pauseGame,
            toggleHUD,
            takeScreenshot,
            debugMode,
            respawn,
            brakes,
        };

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
        if (Array.FindAll<KeyCode>(keys, key => key == brakes).Length > 1)
        {
            duplicates += "Brakes, ";
        }

        duplicates = duplicates != "" ? duplicates.Remove(duplicates.Length - 2, 1) : "";
    }
}
