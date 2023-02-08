/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Red Light Green Light
 * Creation Date: 2/7/2023 1:28:47 PM
 * 
 * Description: Ensures the correct screen setting is selected.
*********************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceFullScreen : MonoBehaviour
{
    #region Functions
    // Start is called before the first frame update
    private void Awake()
    {
#if UNITY_EDITOR
        return;
#endif
        if(Screen.fullScreenMode != FullScreenMode.ExclusiveFullScreen)
        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
    }
    #endregion
}
