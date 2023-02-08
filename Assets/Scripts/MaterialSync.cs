/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Red Light Green Light
 * Creation Date: 2/3/2023 4:14:54 PM
 * 
 * Description: TODO
*********************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialSync : MonoBehaviour
{
    #region Fields

    #endregion

    #region Functions
    // Start is called before the first frame update
    private void Awake()
    {
        var renderers = GetComponentsInChildren<MeshRenderer>();
        var material = GetComponent<MeshRenderer>().material;

        foreach(Renderer renderer in renderers)
        {
            renderer.material = material;
        }
    }
    #endregion
}
