/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Red Light Green Light
 * Creation Date: 1/31/2023 3:23:23 PM
 * 
 * Description: TODO
*********************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayLightAnimation : MonoBehaviour
{
    #region Fields
    private Animation animationClip;
    #endregion

    #region Functions
    // Start is called before the first frame update
    private void Awake()
    {
        animationClip = GetComponent<Animation>();
    }

    private void OnEnable()
    {
        animationClip.Play();
    }
    #endregion
}
