using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GliderController controller;
    public Text scoreText;
    public Text highScore;

    void Update()
    {
        scoreText.text = controller.lastScore + "";
        highScore.text = controller.highScore + "";
    }
    public void NewGame()
    {
        // get the seed from config
     }
    public void Quit()
    {
        Application.Quit();
    }

}
