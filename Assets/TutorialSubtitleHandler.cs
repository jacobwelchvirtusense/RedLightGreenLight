/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Red Light Green Light
 * Creation Date: 2/20/2023 9:37:47 AM
 * 
 * Description: TODO
*********************************/
using System.Collections;
using TMPro;
using UnityEngine;

public class TutorialSubtitleHandler : MonoBehaviour
{
    #region Fields
    private static TutorialSubtitleHandler instance;

    private TextMeshProUGUI subtitleText;
    private float timeBetweenCharacters = 0.015f;
    #endregion

    #region Functions
    private void Awake()
    {
        instance = this;
        subtitleText = GetComponentInChildren<TextMeshProUGUI>();
        gameObject.SetActive(false);
    }

    public static void SetSubtitle(string subtitle)
    {
        if (instance == null) return;

        var subtileValid = subtitle != "";
        instance.gameObject.SetActive(subtileValid);
        if(subtileValid) instance.StartCoroutine(instance.SubtitleRoutine(subtitle));
    }

    private IEnumerator SubtitleRoutine(string subtitle)
    {
        if (subtitleText == null) yield break;

        subtitleText.text = "";

        foreach (char c in subtitle)
        {
            subtitleText.text += c;
            yield return new WaitForSeconds(timeBetweenCharacters);
        }
    }
    #endregion
}
