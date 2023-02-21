/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Apple Basket
 * Creation Date: 1/30/2023 9:44:58 AM
 * 
 * Description: Handles the end game displays for data.
*********************************/
using TMPro;
using UnityEngine;

public class EndGameDataDisplay : MonoBehaviour
{
    #region Fields
    private TextMeshProUGUI textMeshProUGUI;
    #endregion

    #region Functions
    /// <summary>
    /// Initializes components.
    /// </summary>
    private void Awake()
    {
        textMeshProUGUI = GetComponent<TextMeshProUGUI>();
    }

    /// <summary>
    /// Displays the score for this end game data.
    /// </summary>
    /// <param name="displayText">The end game data text to be displayed.</param>
    public void UpdateText(string displayText)
    {
        if (textMeshProUGUI == null) return;

        textMeshProUGUI.text = displayText;
    }
    #endregion
}
