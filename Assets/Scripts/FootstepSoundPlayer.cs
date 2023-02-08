/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Red Light Green Light
 * Creation Date: 2/7/2023 3:18:59 PM
 * 
 * Description: TODO
*********************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static InspectorValues;

public class FootstepSoundPlayer : MonoBehaviour
{
    #region Fields
    /// <summary>
    /// The AudioSource for game state events.
    /// </summary>
    private AudioSource audioSource;

    [Header("Sound")]
    [Tooltip("The footstep sounds")]
    [SerializeField]
    private AudioClip[] footstepSounds;

    [Range(0.0f, 1.0f)]
    [Tooltip("The volume of the footsteps")]
    [SerializeField]
    private float footstepVolume = 1.0f;
    #endregion

    #region Functions
    private void Awake()
    {
        audioSource = transform.root.GetComponent<AudioSource>();
    }

    /// <summary>
    /// Plays a sound for a specified event (Has null checks built in).
    /// </summary>
    /// <param name="soundClip">The sound clip to be played.</param>
    /// <param name="soundVolume">The volume of the sound to be played.</param>
    public void PlaySound()
    {
        if (audioSource == null || footstepSounds == null || footstepSounds.Length == 0) return;

        audioSource.PlayOneShot(footstepSounds[Random.Range(0, footstepSounds.Length)], footstepVolume);
    }
    #endregion
}
