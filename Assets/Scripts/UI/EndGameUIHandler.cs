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
    private static TextMeshProUGUI endGameMessage;
    #endregion

    #region Functions
    /// <summary>
    /// Initializes the return to start text.
    /// </summary>
    private void Awake()
    {
        endGameMessage = GetComponent<TextMeshProUGUI>();
        endGameMessage.enabled = true;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Enables the return to start text.
    /// </summary>
    /// <param name="shouldEnable">Holds true if the text should be enabled.</param>
    public static void EnableText(bool shouldEnable)
    {
        if (endGameMessage == null) return;

        endGameMessage.gameObject.SetActive(shouldEnable);
    }
    #endregion
}
