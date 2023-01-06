/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Red Light Green Light
 * Creation Date: 12/27/2022 11:38:19 AM
 * 
 * Description: Handles the game state and events that happen in it.
 *              Examples include light state, scoring, and data output.
*********************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static InspectorValues;
using static GameSettings;

[RequireComponent(typeof(AudioSource))]
public class GameController : MonoBehaviour
{
    #region Fields
    /// <summary>
    /// Reference to the Game Controller in the scene.
    /// </summary>
    public static GameController gameController { get; private set; }

    #region Timer
    [Header("Timer")]
    [Range(0, 300)]
    [Tooltip("The minimum value for the timer in seconds")]
    [SerializeField] private int minTimerAmount = 30;

    [Range(0, 300)]
    [Tooltip("The maximum value for the timer in seconds")]
    [SerializeField] private int maxTimerAmount = 120;

    [Range(0.0f, 1.0f)]
    [Tooltip("The current lerp between the min and max timer amounts")]
    [SerializeField] private float currentTimer = 0.5f;
    #endregion

    #region Red Light Green Light
    [Tooltip("For debugging movement (true disables red lights from happening)")]
    [SerializeField] private bool disableRed = false;

    [Space(SPACE_BETWEEN_EDITOR_ELEMENTS)]

    /// <summary>
    /// The number of red lights the user has failed.
    /// </summary>
    private int failedRedLights = 0;

    /// <summary>
    /// Holds true if the game will detect movement during a red light.
    /// </summary>
    [HideInInspector] public bool canDetectPenaltyMovement = false;

    /// <summary>
    /// Reference to the current game loop.
    /// </summary>
    private Coroutine redGreenLoopReference;

    #region Lights
    #region Light State
    /// <summary>
    /// States for the light, yellow means inactive.
    /// </summary>
    public enum LightState { RED, GREEN, OFF };

    /// <summary>
    /// The current light state for the light.
    /// </summary>
    [HideInInspector] public LightState lightState = LightState.OFF;
    #endregion

    /// <summary>
    /// Event that is called whenever the light state is changed.
    /// </summary>
    [HideInInspector] public UnityEvent<LightState> lightChangeEvent;
    #endregion

    #region Time Before Event
    [Header("Timings")]
    #region Random Number Generation Method
    [Tooltip("The method for generating random intervals between light changes")]
    [SerializeField] private RandomNumberMethod randomNumberMethod = RandomNumberMethod.LINEAR;

    /// <summary>
    /// The method for generating random intervals between light changes.
    /// </summary>
    private enum RandomNumberMethod { LINEAR, NORMAL };
    #endregion

    #region Time Before Start
    [Range(0, 10)]
    [Tooltip("The count down time before starting")]
    [SerializeField] private int timeBeforeStart = 3;

    [Space(SPACE_BETWEEN_EDITOR_ELEMENTS)]
    #endregion

    #region Red Light Movement Timings
    [Range(0.0f, 5.0f)]
    [Tooltip("The time before checking for user movement during a red light (easy is index 0 - 2 for hard)")]
    [SerializeField] private float[] timeBeforeMovementDetection = new float[] { 0.4f, 0.3f, 0.25f };

    [Range(0, 10)]
    [Tooltip("The time to wait before going to a green light after failing a red light")]
    [SerializeField] private float penaltyDuration = 1.0f;

    [Space(SPACE_BETWEEN_EDITOR_ELEMENTS)]
    #endregion

    #region Time Before Light Change
    #region Time Before Red
    [Range(0.0f, 20.0f)]
    [Tooltip("The minimum time from a green light to a red light")]
    [SerializeField] private float minTimeBeforeRed = 2.0f;

    [Range(0.0f, 20.0f)]
    [Tooltip("The maximum time from a green light to red light")]
    [SerializeField] private float maxTimeBeforeRed = 7.0f;

    [Space(SPACE_BETWEEN_EDITOR_ELEMENTS)]
    #endregion

    #region Time Before Green
    [Range(0.0f, 20.0f)]
    [Tooltip("The minimum time from a red light to a greed light")]
    [SerializeField] private float minTimeBeforeGreen = 1.0f;

    [Range(0.0f, 20.0f)]
    [Tooltip("The maximum time from a red light to a greed light")]
    [SerializeField] private float maxTimeBeforeGreen = 3.0f;
    #endregion
    #endregion
    #endregion

    #region Red Light Camera Shake
    [Header("Penalty Camera Shake")]
    [Range(0.0f, 10.0f)]
    [Tooltip("The amplitude of camera shake when failing a red light")]
    [SerializeField] private float cameraShakeAmplitude = 1.5f;

    [Range(0.0f, 10.0f)]
    [Tooltip("The frequency of camera shake when failing a red light")]
    [SerializeField] private float cameraShakeFrequency = 1.5f;

    [Range(0.0f, 10.0f)]
    [Tooltip("The duration of camera shake when failing a red light")]
    [SerializeField] private float cameraShakeDuration = 1.25f;
    #endregion

    #region Sound
    /// <summary>
    /// The AudioSource for game state events.
    /// </summary>
    private AudioSource audioSource;

    [Header("Sound")]
    #region Countdown Sound
    [Tooltip("The sound made with each change of the countdown")]
    [SerializeField]
    private AudioClip countDownSound;

    [Range(0.0f, 1.0f)]
    [Tooltip("The volume of the count down sound")]
    [SerializeField]
    private float countDownSoundVolume = 1.0f;

    [Space(SPACE_BETWEEN_EDITOR_ELEMENTS)]
    #endregion

    #region Red Light
    [Tooltip("The sound made when going to a red light")]
    [SerializeField]
    private AudioClip redLightSound;

    [Range(0.0f, 1.0f)]
    [Tooltip("The volume of the sound when going to a red light")]
    [SerializeField]
    private float redLightSoundVolume = 1.0f;

    [Space(SPACE_BETWEEN_EDITOR_ELEMENTS)]
    #endregion

    #region Green Light
    [Tooltip("The sound made when going to a green light")]
    [SerializeField]
    private AudioClip greenLightSound;

    [Range(0.0f, 1.0f)]
    [Tooltip("The volume of the sound when going to a green light")]
    [SerializeField]
    private float greenLightSoundVolume = 1.0f;

    [Space(SPACE_BETWEEN_EDITOR_ELEMENTS)]
    #endregion

    #region Fail Sound
    [Tooltip("The sound made when the player moves at an incorrect time")]
    [SerializeField]
    private AudioClip failSound;

    [Range(0.0f, 1.0f)]
    [Tooltip("The volume of the sound when the player moves at an incorrect time")]
    [SerializeField]
    private float failSoundVolume = 1.0f;

    [Space(SPACE_BETWEEN_EDITOR_ELEMENTS)]
    #endregion
    #endregion
    #endregion

    #region Points
    [Header("Points")]
    [Range(0, 1000)]
    [Tooltip("The amount of points to earn per tick of movement")]
    [SerializeField] private int pointsPerMovement = 10;

    [Range(0, 10000)]
    [Tooltip("The amount of points to lose when failing a red light")]
    [SerializeField] private int penaltyPoints = 1000;
    #endregion
    #endregion

    #region Functions
    #region Initialization
    /// <summary>
    /// Initializes components and starts the game.
    /// </summary>
    private void Awake()
    {
        InitializeComponents();
        GameTimerUIHandler.UpdateTimer(0);

        StartCoroutine(CountdownLoop());
    }

    /// <summary>
    /// Initializes any components needed.
    /// </summary>
    private void InitializeComponents()
    {
        gameController = this;
        audioSource = GetComponent<AudioSource>();
    }
    #endregion

    #region Countdown
    /// <summary>
    /// Counts down before starting the game again.
    /// </summary>
    /// <returns></returns>
    private IEnumerator CountdownLoop()
    {
        // Sets the initial state
        GoToOffLight();
        int t = timeBeforeStart;
        CountdownUIHandler.UpdateCountdown(0);

        yield return new WaitForSeconds(1);

        CountdownUIHandler.UpdateCountdown(timeBeforeStart);

        #region Countdown Update
        while (t > 0)
        {
            PlaySound(countDownSound, countDownSoundVolume);

            yield return new WaitForSeconds(1);

            t -= 1;
            CountdownUIHandler.UpdateCountdown(t);
        }
        #endregion

        StartRedGreenLoop(true);

        // Sets timer if this game mode has it
        if (CurrentGameMode.Equals(GameMode.STATIONARY)) StartCoroutine(GameTimer());
    }
    #endregion

    #region Timer
    private IEnumerator GameTimer()
    {
        int t = Mathf.RoundToInt(Mathf.Lerp(minTimerAmount, maxTimerAmount, currentTimer));
        GameTimerUIHandler.UpdateTimer(t);  // Initializes the countdown

        #region Countdown Update
        while (t > 0)
        {
            yield return new WaitForSeconds(1);

            t -= 1;
            GameTimerUIHandler.UpdateTimer(t);
        }
        #endregion

        StartCoroutine(EndGame());
    }
    #endregion

    #region State Handling
    #region Red
    /// <summary>
    /// Handles the event of failing a red light.
    /// </summary>
    /// <returns></returns>
    public IEnumerator FailedRedRoutine()
    {
        failedRedLights++;
        StartRedGreenLoop(false);
        PlaySound(failSound, failSoundVolume);
        StartCoroutine(MainCameraHandler.ApplyCameraShake(cameraShakeAmplitude, cameraShakeFrequency, cameraShakeDuration));
        UpdatePoints(-penaltyPoints);
        yield return new WaitForSeconds(penaltyDuration);

        switch (CurrentGameMode)
        {
            case GameMode.RACE:
                ReturnStartUIHandler.EnableText(true);
                break;
            case GameMode.STATIONARY:
            default:
                StartRedGreenLoop(true);
                break;
        }
    }

    public void RestartGameRace()
    {
        ReturnStartUIHandler.EnableText(false);
        StartCoroutine(CountdownLoop());
    }
    #endregion

    #region Green
    /// <summary>
    /// Updates the player's current point total.
    /// </summary>
    public void UpdatePoints(int updateAmount = 0)
    {
        if (updateAmount == 0) updateAmount = pointsPerMovement;

        PointUIHandler.UpdatePoints(updateAmount);
    }
    #endregion
    #endregion

    #region Change Light
    #region Red Green Loop
    /// <summary>
    /// Starts or stops the red light green loop.
    /// </summary>
    /// <param name="shouldStart">Holds true if the loop should be started.</param>
    private void StartRedGreenLoop(bool shouldStart)
    {
        if(redGreenLoopReference == null)
        {
            if (shouldStart)
            {
                redGreenLoopReference = StartCoroutine(RedGreenLoop());
            }
        }
        else if (!shouldStart)
        {
            StopCoroutine(redGreenLoopReference);
            redGreenLoopReference = null;
        }
    }

    /// <summary>
    /// Constantly loops between red and green lights in psuedo random intervals.
    /// </summary>
    /// <returns></returns>
    private IEnumerator RedGreenLoop()
    {
        while (true)
        {
            GoToGreenLight();
            yield return new WaitForSeconds(GenerateRandomValue(minTimeBeforeRed, maxTimeBeforeRed));
            
            if(!disableRed)
            GoToRedLight();
            yield return new WaitForSeconds(GenerateRandomValue(minTimeBeforeGreen, maxTimeBeforeGreen));
        }
    }

    /// <summary>
    /// Chooses the current random generation method and returns a randomized value.
    /// </summary>
    /// <param name="minValue">The minimum random value.</param>
    /// <param name="maxValue">The maximum random value.</param>
    /// <returns></returns>
    private float GenerateRandomValue(float minValue, float maxValue)
    {
        switch (randomNumberMethod)
        {
            case RandomNumberMethod.NORMAL:
                return RandomNormalDistribution.Generate(minValue, maxValue);

            case RandomNumberMethod.LINEAR:
            default:
                return Mathf.Lerp(minValue, maxValue, Random.value);
        }
    }
    #endregion

    #region Light Change Event
    /// <summary>
    /// Changes the color to off (game inactive state).
    /// </summary>
    private void GoToOffLight()
    {
        UpdateActiveLights(LightState.OFF);

        StartRedGreenLoop(false);
    }

    /// <summary>
    /// Changes the state to green light.
    /// </summary>
    private void GoToGreenLight()
    {
        PlaySound(greenLightSound, greenLightSoundVolume);

        UpdateActiveLights(LightState.GREEN);
    }

    /// <summary>
    /// Changes the state to red light.
    /// </summary>
    private void GoToRedLight()
    {
        PlaySound(redLightSound, redLightSoundVolume);
        StartCoroutine(RedLightDetectionDelay());

        UpdateActiveLights(LightState.RED);
    }

    /// <summary>
    /// Waits before allowing movement detection during a red light.
    /// </summary>
    /// <returns></returns>
    private IEnumerator RedLightDetectionDelay()
    {
        canDetectPenaltyMovement = false;

        yield return new WaitForSeconds(timeBeforeMovementDetection[(int)CurrentMovementTrackingMethod]);

        canDetectPenaltyMovement = true;
    }

    /// <summary>
    /// Updates the status of the active lights in the scene.
    /// </summary>
    private void UpdateActiveLights(LightState newLightState)
    {
        lightState = newLightState;
        lightChangeEvent.Invoke(lightState);
    }
    #endregion
    #endregion

    #region End Game
    public IEnumerator EndGame()
    {
        yield return new WaitForEndOfFrame();
        GoToOffLight();
        EndGameUIHandler.EnableText(true);
        OutputData();
    }

    #region Output
    private void OutputData()
    {
        //failedRedLights;
        //movementTrackingMethod;
    }
    #endregion
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
