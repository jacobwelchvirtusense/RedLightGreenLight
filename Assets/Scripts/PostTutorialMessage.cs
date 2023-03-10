/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Apple Basket
 * Creation Date: 3/10/2023 12:03:59 PM
 * 
 * Description: TODO
*********************************/
using com.rfilkov.kinect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostTutorialMessage : MonoBehaviour
{
    #region Fields
    public static bool showMessage;

    [SerializeField] private GameObject settings;
    [SerializeField] private GameObject postTutorialMessage;
    #endregion

    #region Functions
    private void Start()
    {
        GameController.ResetGameEvent.AddListener(ResetPostMessage);
    }

    private void ResetPostMessage()
    {
        StartCoroutine(SlightPostMessageDelay());
    }

    private IEnumerator SlightPostMessageDelay()
    {
        yield return null;

        if (showMessage)
        {
            showMessage = false;
            FindObjectOfType<KinectManager>().shouldDisplaySensorData = false;
            postTutorialMessage.SetActive(true);
            settings.SetActive(false);
        }
    }

    public void ShowMessage(bool shouldShow)
    {
        postTutorialMessage.SetActive(shouldShow);
    }

    public void ShowSettings(bool shouldShow)
    {
        settings.SetActive(shouldShow);
    }
    #endregion
}
