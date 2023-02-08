/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Red Light Green Light
 * Creation Date: 1/31/2023 1:33:54 PM
 * 
 * Description: The player will return to the specified start location.
 *              This basically optimizes the level so that
 *              occlusion culling is enabled on all stuff without
 *              rebaking at run time.
*********************************/
using UnityEngine;
using static GameController;

public class StartEndReturn : MonoBehaviour
{
    #region Fields
    private static Transform start;
    #endregion

    #region Functions
    /// <summary>
    /// Initializes the start location.
    /// </summary>
    private void Awake()
    {
        var col = GetComponent<Collider>();

        if (!col.enabled)
        {
            start = transform;
        }
    }

    /// <summary>
    /// Calls for the player to be sent back to the start location seamlessly.
    /// </summary>
    /// <param name="other">The player.</param>
    private void OnTriggerEnter(Collider other)
    {
        if(gameController.lightState != LightState.OFF)
        other.transform.position -= (transform.position - start.position);
    }
    #endregion
}
