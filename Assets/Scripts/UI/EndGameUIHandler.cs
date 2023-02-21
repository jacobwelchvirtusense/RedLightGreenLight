/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Red Light Green Light
 * Creation Date: 1/5/2023 2:22:16 PM
 * 
 * Description: Displays a message at the end of the game.
*********************************/
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndGameUIHandler : MonoBehaviour
{
    #region Fields
    /// <summary>
    /// The return to start text.
    /// </summary>
    private static GameObject endGameMessage;

    [SerializeField] private EndGameDataDisplay metersDisplayed;
    [SerializeField] private EndGameDataDisplay timeSpent;
    [SerializeField] private EndGameDataDisplay averageSpeed;
    [SerializeField] private EndGameDataDisplay redLightsFailed;
    [SerializeField] private EndGameDataDisplay metersLost;

    private static EndGameDataDisplay MetersDisplayed;
    private static EndGameDataDisplay TimeSpent;
    private static EndGameDataDisplay AverageSpeed;
    private static EndGameDataDisplay RedLightsFailed;
    private static EndGameDataDisplay MetersLost;
    #endregion

    #region Functions
    /// <summary>
    /// Initializes the return to start text.
    /// </summary>
    private void Awake()
    {
        endGameMessage = gameObject;

        MetersDisplayed = metersDisplayed;
        TimeSpent = timeSpent;
        AverageSpeed = averageSpeed;
        RedLightsFailed = redLightsFailed;
        MetersLost = metersLost;

        gameObject.SetActive(false);

        GameController.ResetGameEvent.AddListener(ResetEndGameUI);
    }

    private void ResetEndGameUI()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Enables the return to start text.
    /// </summary>
    /// <param name="shouldEnable">Holds true if the text should be enabled.</param>
    public static void EnableText(bool shouldEnable)
    {
        if (endGameMessage == null) return;

        endGameMessage.SetActive(shouldEnable);
    }

    /// <summary>
    /// Updates the data being displayed in the end screen.
    /// </summary>
    /// <param name="metersTraveled">The meters that the user traveled</param>
    /// <param name="timeSpent">The time the user spent in the game</param>
    /// <param name="averageSpeed">The average speed of the user</param>
    /// <param name="redLightsFailed">The amount of red lights that the user has failed</param>
    /// <param name="metersLost">The amount of meters lost from failing red lights.</param>
    public static void UpdateEndGameData(int metersTraveled, int timeSpent, float averageSpeed, int redLightsFailed, int metersLost)
    {
        MetersDisplayed.UpdateText(metersTraveled.ToString() + "m");
        TimeSpent.UpdateText(GameTimerUIHandler.TimeToString(timeSpent));
        AverageSpeed.UpdateText(averageSpeed.ToString() + "m/s");
        RedLightsFailed.UpdateText(redLightsFailed.ToString());
        MetersLost.UpdateText(metersLost.ToString() + "m");
    }
    #endregion
}
