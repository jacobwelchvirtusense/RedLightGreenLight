/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Red Light Green Light
 * Creation Date: 12/28/2022 9:15:02 AM
 * 
 * Description: Displays the countdown for the game to start.
*********************************/
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountdownUIHandler : MonoBehaviour
{
    #region Fields
    /// <summary>
    /// All of the points displays.
    /// </summary>
    private static TextMeshProUGUI countdownText;

    private static Image countdownImage;
    #endregion

    #region Functions
    /// <summary>
    /// Initializes the countdown UI display.
    /// </summary>
    private void Awake()
    {
        countdownText = GetComponent<TextMeshProUGUI>();
        countdownImage = GetComponentInChildren<Image>();

        UpdateCountdown(0);
    }

    /// <summary>
    /// Updates countdown UI.
    /// </summary>
    /// <param name="currentCount">The current countdown to be displayed.</param>
    public static void UpdateCountdown(int currentCount)
    {
        // Ensures the text isn't null
        if (countdownText == null) return;

        // Updates the countdown UI 
        countdownText.text = currentCount.ToString();
        countdownText.gameObject.SetActive(currentCount != 0);
    }

    public static void ChangeTransparency()
    {
        var color = Color.white;
        color.a = 0.5f;

        countdownImage.color = color;
    }
    #endregion
}
