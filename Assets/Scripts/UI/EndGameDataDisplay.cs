/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Apple Basket
 * Creation Date: 1/30/2023 9:44:58 AM
 * 
 * Description: TODO
*********************************/
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndGameDataDisplay : MonoBehaviour
{
    #region Fields
    [Tooltip("The number of dots to displace the score by")]
    [SerializeField] private int numberOfDots = 10;

    private const int numberOfDotsToChar = 2;

    private TextMeshProUGUI textMeshProUGUI;

    private string startingText = "";
    #endregion

    #region Functions
    private void Awake()
    {
        textMeshProUGUI = GetComponent<TextMeshProUGUI>();
        startingText = textMeshProUGUI.text;
    }

    public void UpdateText(string displayText)
    {
        if (textMeshProUGUI == null) return;

        var numberOfCharacters = displayText.Length;
        numberOfDots -= numberOfDotsToChar * numberOfCharacters;
        string dots = "";

        for(int i = 0; i < numberOfDots; i++)
        {
            dots += ".";
        }

        textMeshProUGUI.text = startingText + dots + displayText;
    }
    #endregion
}
