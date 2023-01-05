/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Red Light Green Light
 * Creation Date: 12/27/2022 11:38:35 AM
 * 
 * Description: Handles Updating the displayed points for the player.
*********************************/
using System.Collections.Generic;
using TMPro;
using UnityEngine;

using static GameSettings;

public class PointUIHandler : MonoBehaviour
{
    #region Fields
    /// <summary>
    /// All of the points displays.
    /// </summary>
    private static List<TextMeshProUGUI> PointsText = new List<TextMeshProUGUI>();

    /// <summary>
    /// The current amount of points the player has.
    /// </summary>
    private static int currentPoints = 0;
    #endregion

    #region Functions
    /// <summary>
    /// Initializes the point UI display.
    /// </summary>
    private void Awake()
    {
        PointsText.Add(GetComponent<TextMeshProUGUI>());
        UpdatePoints(0);
    }

    private void Start()
    {
        gameObject.SetActive(CurrentGameMode != GameMode.RACE);
    }

    /// <summary>
    /// Updates the points and all of the UI displays for it.
    /// </summary>
    /// <param name="increment">The amount of points the player gained or lost.</param>
    public static void UpdatePoints(int increment)
    {
        // Increments the current points the player has
        currentPoints += increment;

        // Ensures the list and its elements are not null
        if (PointsText == null) return;
        PointsText.Remove(null);

        // Updates all point 
        foreach(TextMeshProUGUI text in PointsText) text.text = currentPoints.ToString();
    }
    #endregion
}
