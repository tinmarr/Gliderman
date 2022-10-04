using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuOnLoad : MonoBehaviour
{
    public GameObject[] menus;

    void Start()
    {
        foreach (GameObject menu in menus)
        {
            menu.SetActive(false);
        }
    }
}
