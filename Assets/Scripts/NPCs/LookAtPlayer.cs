/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Red Light Green Light
 * Creation Date: 2/8/2023 11:04:00 AM
 * 
 * Description: The object will rotate to always
 * look at the player.
*********************************/
using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    #region Fields
    /// <summary>
    /// The transform of the player.
    /// </summary>
    private Transform player;
    #endregion

    #region Functions
    /// <summary>
    /// Gets the transform of the player.
    /// </summary>
    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        RotateLookAtPlayer();
    }

    /*
    /// <summary>
    /// Rotates this objects in the z axis to always look at the player.
    /// </summary>
    private void FixedUpdate()
    {
        RotateLookAtPlayer();
    }*/

    private void RotateLookAtPlayer()
    {
        transform.LookAt(player.position);
        transform.rotation = Quaternion.Euler(new Vector3(0, transform.rotation.eulerAngles.y, 0));
    }
    #endregion
}
