/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Red Light Green Light
 * Creation Date: 2/21/2023 2:41:44 PM
 * 
 * Description: TODO
*********************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class TutorialVideoHandler : MonoBehaviour
{
    #region Fields
    private static TutorialVideoHandler instance;

    /// <summary>
    /// The player of all tutorial videos.
    /// </summary>
    private VideoPlayer videoPlayer;

    [Tooltip("If this video is sent then the last selected video will be kept")]
    [SerializeField] private VideoClip repeatLastVideoClip;
    #endregion

    #region Functions
    // Start is called before the first frame update
    private void Awake()
    {
        instance = this;

        videoPlayer = GetComponentInChildren<VideoPlayer>();

        GameController.ResetGameEvent.AddListener(ResetTutorialVideo);
        gameObject.SetActive(false);
    }

    private void ResetTutorialVideo()
    {
        SetVideo(null);
    }

    public static void SetVideo(VideoClip video)
    {
        if (instance.repeatLastVideoClip == video) return;

        else if (video == null)
        {
            //instance.movementTutorial.SetActive(false);
            instance.gameObject.SetActive(false);
        }
        else if (video != instance.videoPlayer.clip)
        {
            instance.videoPlayer.clip = video;
            instance.gameObject.SetActive(true);
        }
    }
    #endregion
}
