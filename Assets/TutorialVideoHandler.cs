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

public class TutorialVideoHandler : MonoBehaviour
{
    #region Fields
    private static TutorialVideoHandler instance;

    [Tooltip("The game object that holds the movement tutorial video")]
    [SerializeField] private GameObject movementTutorial;
    #endregion

    #region Functions
    // Start is called before the first frame update
    private void Awake()
    {
        instance = this;

        GameController.ResetGameEvent.AddListener(ResetTutorialVideo);
        gameObject.SetActive(false);
    }

    private void ResetTutorialVideo()
    {
        SetVideo("None");
    }

    public static void SetVideo(string video)
    {
        switch (video)
        {
            case "Movement":
                instance.gameObject.SetActive(true);
                instance.movementTutorial.SetActive(true);
                break;
            case "None":
            default:
                instance.movementTutorial.SetActive(false);
                instance.gameObject.SetActive(false);
                break;
        }
    }
    #endregion
}
