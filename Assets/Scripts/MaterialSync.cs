/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Red Light Green Light
 * Creation Date: 2/3/2023 4:14:54 PM
 * 
 * Description: Syncs all child objects to use the same
 * material. This is for LODs which can have a different
 * material than their parent object.
*********************************/
using UnityEngine;

public class MaterialSync : MonoBehaviour
{
    #region Functions
    /// <summary>
    /// Sets the material of all child objects to be the same as the parent.
    /// </summary>
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
