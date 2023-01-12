/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Red Light Green Light
 * Creation Date: 12/29/2022 1:54:52 PM
 * 
 * Description: Handles the displayed timer for the game.
*********************************/
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameTimerUIHandler : MonoBehaviour
{
    #region Fields
    /// <summary>
    /// All of the points displays.
    /// </summary>
    private static TextMeshProUGUI timerText;
    #endregion

    #region Functions
    /// <summary>
    /// Initializes the timer UI display.
    /// </summary>
    private void Awake()
    {
        timerText = GetComponent<TextMeshProUGUI>();
        UpdateTimer(0);
    }

    /// <summary>
    /// Updates timer UI.
    /// </summary>
    /// <param name="currentCount">The current timer to be displayed.</param>
    public static void UpdateTimer(int currentTimer)
    {
        // Ensures the text isn't null
        if (timerText == null) return;

        // Updates the countdown UI 
        timerText.text = "Time Left: " + currentTimer.ToString();
        timerText.gameObject.SetActive(currentTimer != 0);
    }
    #endregion
}
