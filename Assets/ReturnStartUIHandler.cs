/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Red Light Green Light
 * Creation Date: 1/4/2023 11:18:17 AM
 * 
 * Description: Handles the UI text for telling the player to return to start.
*********************************/
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ReturnStartUIHandler : MonoBehaviour
{
    #region Fields
    /// <summary>
    /// The return to start text.
    /// </summary>
    private static TextMeshProUGUI returnToStartText;
    #endregion

    #region Functions
    /// <summary>
    /// Initializes the return to start text.
    /// </summary>
    private void Awake()
    {
        returnToStartText = GetComponent<TextMeshProUGUI>();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Enables the return to start text.
    /// </summary>
    /// <param name="shouldEnable">Holds true if the text should be enabled.</param>
    public static void EnableText(bool shouldEnable)
    {
        if(returnToStartText == null) return;

        returnToStartText.gameObject.SetActive(shouldEnable);
    }
    #endregion
}
