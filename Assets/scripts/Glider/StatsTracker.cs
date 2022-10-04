using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/*
How to track a new stat:
1. Add property to the StatsTracker.Stats struct
2. In the structs constructor, add the default value for the property
3. Add the property to the ToString method
4. Validate the property as needed in the Validate method
5. In the UpdateStats method of StatsTracker, update the statistic as needed
6. In the UpdateBestStats method of StatsTracker, update the best and lifetime stat using the format of the other stats
7. Make sure the Debug screen isnt messed up
8. Add it to highscore page
 */

[RequireComponent(typeof(GameHandler))]
public class StatsTracker : MonoBehaviour
{   
    [Tooltip("The number of frames between each stat update. Increasing this number decreases accuracy.")]
    public int updateSpeed = 0;
    public bool freeze = false;

    public TMP_Text bestText;
    public TMP_Text lifetimeText;

    GliderController glider;
    GameHandler game;

    Stats stats = new Stats();
    Stats bestStats = new Stats(); // TODO get from save
    Stats lifetimeStats = new Stats(); // TODO get from save

    private void Start()
    {
        game = GetComponent<GameHandler>();
        glider = game.glider;
    }

    private void LateUpdate()
    {
        if (Time.frameCount % (updateSpeed + 1) == 0 && !glider.dead && !freeze) UpdateStats();
    }

    private void OnValidate()
    {
        updateSpeed = Mathf.Clamp(updateSpeed, 0, int.MaxValue);
        stats.Validate();
    }

    public void UpdateStats()
    {
        stats.timeAlive = Time.time - glider.aliveSince;
        stats.thrustTime += glider.boosting ? Time.smoothDeltaTime : 0;
        stats.underwaterTime += glider.transform.position.y < 50 ? Time.smoothDeltaTime : 0;
        stats.topSpeed = Mathf.Max(stats.topSpeed, glider.rb.velocity.magnitude);
        stats.distanceTraveled += Mathf.Abs((new Vector2(glider.rb.velocity.x, glider.rb.velocity.z)).magnitude * Time.deltaTime);
        stats.highestAltitude = Mathf.Max(stats.highestAltitude, glider.transform.position.y);
        stats.score = glider.currentScore;
        stats.tricksCompleted = 0;

        game.HUD.DisplayStats(stats);
    }

    public void UpdateBestStats()
    {
        stats.Validate();

        bestStats.timeAlive = Mathf.Max(bestStats.timeAlive, stats.timeAlive);
        bestStats.thrustTime = Mathf.Max(bestStats.thrustTime, stats.thrustTime);
        bestStats.underwaterTime = Mathf.Max(bestStats.underwaterTime, stats.underwaterTime);
        bestStats.topSpeed = Mathf.Max(bestStats.topSpeed, stats.topSpeed);
        bestStats.distanceTraveled = Mathf.Max(bestStats.distanceTraveled, stats.distanceTraveled);
        bestStats.highestAltitude = Mathf.Max(bestStats.highestAltitude, stats.highestAltitude);
        bestStats.score = Mathf.Max(bestStats.score, stats.score);
        bestStats.tricksCompleted = Mathf.Max(bestStats.tricksCompleted, stats.tricksCompleted);

        bestStats.Validate();

        lifetimeStats.timeAlive += stats.timeAlive;
        lifetimeStats.thrustTime += stats.thrustTime;
        lifetimeStats.underwaterTime += stats.underwaterTime;
        lifetimeStats.topSpeed += stats.topSpeed;
        lifetimeStats.distanceTraveled += stats.distanceTraveled;
        lifetimeStats.highestAltitude += stats.highestAltitude;
        lifetimeStats.score += stats.score;
        lifetimeStats.tricksCompleted += stats.tricksCompleted;

        lifetimeStats.Validate();

        bestText.text = bestStats.DisplayNumbers();
        lifetimeText.text = lifetimeStats.DisplayNumbers();
    }

    public void ResetStats()
    {
        stats = new Stats();
    }
}

[System.Serializable]
public struct Stats
{
    public float timeAlive;
    public float thrustTime;
    public float underwaterTime;
    public float topSpeed;
    public float distanceTraveled;
    public float highestAltitude;
    public int score;
    public int tricksCompleted;

    public Stats(int def = 0)
    {
        timeAlive = def;
        thrustTime = def;
        underwaterTime = def;
        topSpeed = def;
        distanceTraveled = def;
        highestAltitude = def;
        score = def;
        tricksCompleted = def;
    }

    public override string ToString()
    {
        Validate();
        return
$@"{timeAlive} : ta 
{thrustTime} : tt 
{underwaterTime} : ut 
{topSpeed} : ts 
{distanceTraveled} : dt 
{highestAltitude} : ha 
{tricksCompleted} : tc ";
    }

    public string DisplayNumbers()
    {
        Validate();
        return
$@"{timeAlive}
{thrustTime}
{underwaterTime}
{topSpeed}
{distanceTraveled}
{highestAltitude}
{score}
{tricksCompleted}";
    }

    public void Validate()
    {
        timeAlive = Mathf.Round(timeAlive * 100) / 100;
        thrustTime = Mathf.Round(thrustTime * 100) / 100;
        underwaterTime = Mathf.Round(underwaterTime * 100) / 100;
        topSpeed = Mathf.Round(topSpeed);
        distanceTraveled = Mathf.Round(distanceTraveled);
        highestAltitude = Mathf.Round(highestAltitude);
    }
}