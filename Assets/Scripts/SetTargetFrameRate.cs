/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Apple Basket
 * Creation Date: 2/1/2023 8:22:57 AM
 * 
 * Description: TODO
*********************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetTargetFrameRate : MonoBehaviour
{
    #region Fields
    [Tooltip("The frame rate to limit the user by")]
    [SerializeField] private int targetFrameRate = 120;
    #endregion

    #region Functions
    /// <summary>
    /// Initializes the users frame rate.
    /// </summary>
    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFrameRate;
    }
    #endregion
}
