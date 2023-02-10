/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Red Light Green Light
 * Creation Date: 2/8/2023 11:41:54 AM
 * 
 * Description: Plays music when the game ends.
*********************************/
using UnityEngine;

public class WinMusicHandler : MonoBehaviour
{
    #region Fields
    /// <summary>
    /// The instance of the handler in the scene.
    /// </summary>
    private static WinMusicHandler instance;
    
    /// <summary>
    /// The audiosource for the music.
    /// </summary>
    private static AudioSource audioSource;

    /// <summary>
    /// The music that can be randomly selected.
    /// </summary>
    [field:SerializeField] public AudioClip[] MusicClips { get; private set; }
    #endregion

    #region Functions
    /// <summary>
    /// Initializes the components and references.
    /// </summary>
    private void Awake()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Plays the winning music.
    /// </summary>
    public static void StartWinMusic()
    {
        if (instance == null || audioSource == null || instance.MusicClips.Length == 0) return;

        audioSource.clip = instance.MusicClips[Random.Range(0, instance.MusicClips.Length)];
        audioSource.Play();
    }
    #endregion
}
