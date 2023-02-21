/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Red Light Green Light
 * Creation Date: 2/6/2023 11:46:31 AM
 * 
 * Description: TODO
*********************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverTextHandler : MonoBehaviour
{
    #region Fields
    private static GameOverTextHandler instance;
    #endregion

    #region Functions
    private void Awake()
    {
        instance = this;
        gameObject.SetActive(false);

        GameController.ResetGameEvent.AddListener(ResetGameOverText);
    }

    private void ResetGameOverText()
    {
        gameObject.SetActive(false);
    }

    public static void ShowText()
    {
        instance.gameObject.SetActive(true);
    }
    #endregion
}
