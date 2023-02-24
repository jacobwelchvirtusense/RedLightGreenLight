/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Red Light Green Light
 * Creation Date: 2/20/2023 8:27:06 AM
 * 
 * Description: TODO
*********************************/
using System.Collections;
using static InspectorValues;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TutorialManager : MonoBehaviour
{
    #region Fields
    [Header("Extra Settings")]
    [Range(0, 50)]
    [Tooltip("The delay after the movement tutorial")]
    [SerializeField] private int movementTutorialDistance = 50;

    [Header("Tutorial Timings")]
    [Range(0.0f, 10.0f)]
    [Tooltip("The delay after a light change")]
    [SerializeField] private float lightDelays = 1.5f;

    [Range(0.0f, 10.0f)]
    [Tooltip("The delay after the movement tutorial")]
    [SerializeField] private float delayAfterMovementTutorial = 1.5f;

    [Range(0.0f, 10.0f)]
    [Tooltip("The delay after the red light tutorial")]
    [SerializeField] private float delayAfterRedLightTutorial = 1.5f;

    [Range(0.0f, 10.0f)]
    [Tooltip("The delay after the red light tutorial")]
    [SerializeField] private float delayAfterGreenLightTutorial = 1.5f;

    [Range(0.0f, 10.0f)]
    [Tooltip("The delay after the move faster tutorial")]
    [SerializeField] private float delayAfterMoveFaster = 1.5f;

    [Range(0.0f, 10.0f)]
    [Tooltip("The delay after the last dialogue")]
    [SerializeField] private float delayAtEnd = 1.5f;

    #region Subtitles
    [Header("Subtitles")]
    [TextArea]
    [Tooltip("The subtitle text for the movement tutorial")]
    [SerializeField] private string movementSubtitle = "";

    [TextArea]
    [Tooltip("The subtitle text for the red light tutorial")]
    [SerializeField] private string redLightSubtitle = "";

    [TextArea]
    [Tooltip("The subtitle text for the green light tutorial")]
    [SerializeField] private string greenLightSubtitle = "";

    [TextArea]
    [Tooltip("The subtitle text for the move faster tutorial")]
    [SerializeField] private string moveFasterSubtitle = "";

    [TextArea]
    [Tooltip("The subtitle text for the timing tutorial")]
    [SerializeField] private string timingSubtitle = "";
    #endregion

    #region Sound
    /// <summary>
    /// The AudioSource for game state events.
    /// </summary>
    private AudioSource audioSource;

    [Header("Sound")]
    #region Movement Dialogue
    [Tooltip("The dialogue for the movement tutorial")]
    [SerializeField]
    private AudioClip movementTutorialDialogue;

    [Range(0.0f, 1.0f)]
    [Tooltip("The volume of the movement tutorial")]
    [SerializeField]
    private float movementTutorialDialogueVolume = 1.0f;

    [Space(SPACE_BETWEEN_EDITOR_ELEMENTS)]
    #endregion

    #region Red Light Dialogue
    [Tooltip("The dialogue for the red light tutorial")]
    [SerializeField]
    private AudioClip redLightDialogue;

    [Range(0.0f, 1.0f)]
    [Tooltip("The volume of the red light turoial")]
    [SerializeField]
    private float redLightDialogueVolume = 1.0f;

    [Space(SPACE_BETWEEN_EDITOR_ELEMENTS)]
    #endregion

    #region Green Light Dialogue
    [Tooltip("The dialogue for the green light tutorial")]
    [SerializeField]
    private AudioClip greenLightDialogue;

    [Range(0.0f, 1.0f)]
    [Tooltip("The volume of the green light turoial")]
    [SerializeField]
    private float greenLightDialogueVolume = 1.0f;

    [Space(SPACE_BETWEEN_EDITOR_ELEMENTS)]
    #endregion

    #region Move Faster Dialogue
    [Tooltip("The dialogue for the move faster tutorial")]
    [SerializeField]
    private AudioClip moveFasterDialogue;

    [Range(0.0f, 1.0f)]
    [Tooltip("The volume of the move faster turoial")]
    [SerializeField]
    private float moveFasterDialogueVolume = 1.0f;

    [Space(SPACE_BETWEEN_EDITOR_ELEMENTS)]
    #endregion

    #region Timing Dialogue
    [Tooltip("The dialogue for the timing tutorial")]
    [SerializeField]
    private AudioClip timingDialogue;

    [Range(0.0f, 1.0f)]
    [Tooltip("The volume of the timing turoial")]
    [SerializeField]
    private float timingDialogueVolume = 1.0f;

    [Space(SPACE_BETWEEN_EDITOR_ELEMENTS)]
    #endregion
    #endregion

    public static bool IsPlaying = false;

    private static TutorialManager tutorialManagerSceneInstance;
    #endregion

    #region Functions
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        tutorialManagerSceneInstance = this;
        GameController.ResetGameEvent.AddListener(ResetTutorial);
    }

    private void ResetTutorial()
    {
        TutorialSubtitleHandler.SetSubtitle("");
    }

    public void StartTutorial()
    {
        StartCoroutine(TutorialLoop());
    }

    public static void StopTutorial()
    {
        tutorialManagerSceneInstance.StopAllCoroutines();
    }

    private IEnumerator TutorialLoop()
    {
        IsPlaying = true;

        yield return Countdown.CountdownLoop();

        yield return MovementTutorial();

        yield return RedLightTutorial();

        yield return GreenLightTutorial();

        yield return MoveFasterTutorial();

        yield return TimingTutorial();

        GameController.gameController.PlayAgain();

        IsPlaying = false;
    }

    private IEnumerator MovementTutorial()
    {
        TutorialVideoHandler.SetVideo("Movement");

        yield return TutorialBase(GameController.LightState.GREEN, movementSubtitle, movementTutorialDialogue, movementTutorialDialogueVolume);

        while (PointUIHandler.PointsToMeters() < movementTutorialDistance)
        {
            yield return null;
        }

        TutorialVideoHandler.SetVideo("None");
    }

    private IEnumerator RedLightTutorial()
    {
        StartCoroutine(ForceRedLightFail());

        yield return TutorialBase(GameController.LightState.RED, redLightSubtitle, redLightDialogue, redLightDialogueVolume);

        yield return new WaitForSeconds(delayAfterRedLightTutorial);
    }

    private IEnumerator ForceRedLightFail()
    {
        yield return new WaitForSeconds(0.9f);

        PlayerMovement.ForceRedLightFail();
    }

    private IEnumerator GreenLightTutorial()
    {
        PlayerMovement.AllowMovementAgain();

        yield return TutorialBase(GameController.LightState.GREEN, greenLightSubtitle, greenLightDialogue, greenLightDialogueVolume);

        yield return new WaitForSeconds(delayAfterGreenLightTutorial);
    }

    private IEnumerator MoveFasterTutorial()
    {
        yield return DialoguePlayer(moveFasterSubtitle, moveFasterDialogue, moveFasterDialogueVolume);

        yield return new WaitForSeconds(delayAfterMoveFaster);
    }

    private IEnumerator TimingTutorial()
    {
        yield return DialoguePlayer(timingSubtitle, timingDialogue, timingDialogueVolume);

        yield return new WaitForSeconds(delayAtEnd);
    }

    #region Helper Functions
    private IEnumerator TutorialBase(GameController.LightState lightState, string subtitle, AudioClip dialogue, float dialogueVolume)
    {
        // Sets a green light state and delays for the effect to be off screen
        GameController.LightStateSetter(lightState);
        yield return new WaitForSeconds(lightDelays);

        yield return DialoguePlayer(subtitle, dialogue, dialogueVolume);
    }

    private IEnumerator DialoguePlayer(string subtitle, AudioClip dialogue, float dialogueVolume)
    {
        TutorialSubtitleHandler.SetSubtitle(subtitle);
        PlaySound(dialogue, dialogueVolume);

        if (dialogue != null) yield return new WaitForSeconds(dialogue.length);
    }
    #endregion

    #region Sound
    /// <summary>
    /// Plays a sound for a specified event (Has null checks built in).
    /// </summary>
    /// <param name="soundClip">The sound clip to be played.</param>
    /// <param name="soundVolume">The volume of the sound to be played.</param>
    private void PlaySound(AudioClip soundClip, float soundVolume)
    {
        if (audioSource == null || soundClip == null) return;

        audioSource.PlayOneShot(soundClip, soundVolume);
    }
    #endregion
    #endregion
}
