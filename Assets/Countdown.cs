/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Red Light Green Light
 * Creation Date: 2/20/2023 8:49:34 AM
 * 
 * Description: Handles the funcitonality of a countdown.
*********************************/
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Countdown : MonoBehaviour
{
    #region Fields
    /// <summary>
    /// The AudioSource for game state events.
    /// </summary>
    private AudioSource audioSource;

    /// <summary>
    /// The instance of the countdown in the scene.
    /// </summary>
    private static Countdown instance;

    [Header("Sound")]
    #region Countdown Sound
    [Tooltip("The sound made with each change of the countdown")]
    [SerializeField]
    private AudioClip countDownSound;

    [Range(0.0f, 1.0f)]
    [Tooltip("The volume of the count down sound")]
    [SerializeField]
    private float countDownSoundVolume = 1.0f;
    #endregion
    #endregion

    #region Functions
    /// <summary>
    /// Initializes components.
    /// </summary>
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        instance = this;
    }

    /// <summary>
    /// Counts down before starting the game again.
    /// </summary>
    /// <returns></returns>
    public static IEnumerator CountdownLoop()
    {
        CountdownUIHandler.UpdateCountdown(3);
        var t = 3;

        #region Countdown Update
        while (t > 0)
        {
            instance.PlaySound();

            yield return new WaitForSeconds(1);

            t -= 1;
            CountdownUIHandler.UpdateCountdown(t);
        }
        #endregion
    }

    #region Sound
    /// <summary>
    /// Plays a sound for a specified event (Has null checks built in).
    /// </summary>
    /// <param name="soundClip">The sound clip to be played.</param>
    /// <param name="soundVolume">The volume of the sound to be played.</param>
    private void PlaySound()
    {
        if (audioSource == null || countDownSound == null) return;

        audioSource.PlayOneShot(countDownSound, countDownSoundVolume);
    }
    #endregion
    #endregion
}
