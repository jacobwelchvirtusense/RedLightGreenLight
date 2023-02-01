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
    [SerializeField] private EndGameDataDisplay fastestSpeed;
    [SerializeField] private EndGameDataDisplay redLightsFailed;
    [SerializeField] private EndGameDataDisplay metersLost;

    private static EndGameDataDisplay MetersDisplayed;
    private static EndGameDataDisplay TimeSpent;
    private static EndGameDataDisplay FastestSpeed;
    private static EndGameDataDisplay RedLightsFailed;
    private static EndGameDataDisplay MetersLost;
    #endregion

    #region Functions
    /// <summary>
    /// Initializes the return to start text.
    /// </summary>
    private void Awake()
    {
        /*
        endGameMessage = GetComponent<TextMeshProUGUI>();
        endGameMessage.enabled = true;
        */

        endGameMessage = gameObject;

        MetersDisplayed = metersDisplayed;
        TimeSpent = timeSpent;
        FastestSpeed = fastestSpeed;
        RedLightsFailed = redLightsFailed;
        MetersLost = metersLost;

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

    public static void UpdateEndGameData(int metersTraveled, int timeSpent, float fastestSpeed, int redLightsFailed, int metersLost)
    {
        MetersDisplayed.UpdateText(metersTraveled.ToString() + "m");
        TimeSpent.UpdateText(GameTimerUIHandler.TimeToString(timeSpent));
        FastestSpeed.UpdateText(fastestSpeed.ToString() + "m/s");
        RedLightsFailed.UpdateText(redLightsFailed.ToString());
        MetersLost.UpdateText(metersLost.ToString() + "m");
    }
    #endregion
}
