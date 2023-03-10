/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Red Light Green Light
 * Creation Date: 2/1/2023 1:08:22 PM
 * 
 * Description: Handles the inputs into the settings menu and hooks to its changes.
*********************************/
using com.rfilkov.kinect;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class SettingsManager : MonoBehaviour
{
    #region Fields
    /// <summary>
    /// The audiosource for settings events.
    /// </summary>
    private AudioSource audioSource;

    [Tooltip("The sound made when clicking a button")]
    [SerializeField] private AudioClip clickSound;

    #region Saved Data
    /// <summary>
    /// The current index used for the timer.
    /// </summary>
    private static int timerIndex = 0;

    /// <summary>
    /// The current type of input to be used in the game.
    /// </summary>
    private static int inputType = 0;

    /// <summary>
    /// The current difficulty setting of the movement.
    /// </summary>
    private static int movementDifficulty = 1;

    /// <summary>
    /// Holds true if the audio should be on.
    /// </summary>
    private static bool enableAudio = true;
    #endregion

    /// <summary>
    /// The current settings slot hovered over.
    /// </summary>
    private int currentSettingsSlot = 0;

    /// <summary>
    /// The array of all child settings slots.
    /// </summary>
    private SettingsSlot[] settingsSlots;

    [Tooltip("The slot used for the timer setting")]
    [SerializeField] private IndexedSettingSlot timerSettingSlot;

    [Tooltip("The settings slot for the type of movement to use")]
    [SerializeField] private IndexedSettingSlot redLightDelaySlot;

    [Tooltip("The settings slot for the difficulty of the movement to use")]
    [SerializeField] private IndexedSettingSlot movementDifficultyTypeSettingSlot;

    [Tooltip("The toggle settings slot for muting/unmuting audio")]
    [SerializeField] private IndexedSettingSlot audioToggleSettingSlot;
    #endregion

    #region Functions
    #region Initialization
    /// <summary>
    /// Gets components and sets their initial states.
    /// </summary>
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        settingsSlots = GetComponentsInChildren<SettingsSlot>();
        settingsSlots[0].SetHover(true);
        GameController.ResetGameEvent.AddListener(ResetSettingsMenu);

        InitializeSettings();
    }

    /// <summary>
    /// Sets the initial state of the settings.
    /// </summary>
    private void InitializeSettings()
    {
        RefreshTimerSettings();
        RefreshMovementDifficulty();
        RefreshInputType();
        RefreshAudio();
    }

    private void ResetSettingsMenu()
    {
        gameObject.SetActive(true);
        settingsSlots[currentSettingsSlot].SetHover(false);
        currentSettingsSlot = 0;
        settingsSlots[currentSettingsSlot].SetHover(true);

        InitializeSettings();
    }
    #endregion

    #region Show Sensor Data
    private void OnEnable()
    {
        FindObjectOfType<KinectManager>().shouldDisplaySensorData = true;
    }

    private void OnDisable()
    {
        FindObjectOfType<KinectManager>().shouldDisplaySensorData = false;
    }
    #endregion

    #region Input
    /// <summary>
    /// Gets keyboard inputs for testing purposes.
    /// </summary>
    private void Update()
    {
        KeyboardInput();
    }

    private void KeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            UpdateSelectedSettingSlot(1);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            UpdateSelectedSettingSlot(-1);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ClickSlot();
        }
    }
    #endregion

    /// <summary>
    /// Updates the setting slot that is currently hovered over.
    /// </summary>
    /// <param name="mod"></param>
    private void UpdateSelectedSettingSlot(int mod)
    {
        settingsSlots[currentSettingsSlot].SetHover(false);
        currentSettingsSlot = (currentSettingsSlot+mod) % settingsSlots.Length;

        if(currentSettingsSlot < 0)
        {
            currentSettingsSlot = settingsSlots.Length - 1;
        }

        settingsSlots[currentSettingsSlot].SetHover(true);
        PlayChangeSound();
    }

    /// <summary>
    /// Performs the click event of the currently selected settings slot.
    /// </summary>
    private void ClickSlot()
    {
        settingsSlots[currentSettingsSlot].ClickEvent.Invoke();
        PlayChangeSound();
    }

    /// <summary>
    /// Plays the sound whenever a setting is clicked or hovered over.
    /// </summary>
    public void PlayChangeSound()
    {
        if (audioSource == null || !gameObject.activeInHierarchy) return;
        
        audioSource.PlayOneShot(clickSound);
    }

    /// <summary>
    /// Turns off the settings menu.
    /// </summary>
    public void DisableSettingsMenu()
    {
        gameObject.SetActive(false);
    }

    #region Timer
    /// <summary>
    /// Sets the timer to be the next index.
    /// </summary>
    public void SetTimer()
    {
        timerIndex++;
        timerIndex %= timerSettingSlot.GetSlotAmount();
        RefreshTimerSettings();
    }

    /// <summary>
    /// Refreshed the display setting and its hook.
    /// </summary>
    private void RefreshTimerSettings()
    {
        timerSettingSlot.SetCurrentSlotIndex(timerIndex);
        GameController.UpdateTimer(timerIndex);
    }
    #endregion

    #region Red Light Type
    /// <summary>
    /// Increments the index of the input.
    /// </summary>
    public void SetInputType()
    {
        inputType++;
        inputType %= redLightDelaySlot.GetSlotAmount();
        RefreshInputType();
    }

    /// <summary>
    /// Refreshed the display setting and its hook.
    /// </summary>
    private void RefreshInputType()
    {
        redLightDelaySlot.SetCurrentSlotIndex(inputType);
    }
    #endregion

    #region Movement Difficulty
    /// <summary>
    /// Increments the difficulty of movement index.
    /// </summary>
    public void SetMovementDifficulty()
    {
        movementDifficulty++;
        movementDifficulty %= movementDifficultyTypeSettingSlot.GetSlotAmount();
        RefreshMovementDifficulty();
    }

    /// <summary>
    /// Refreshed the display setting and its hook.
    /// </summary>
    private void RefreshMovementDifficulty()
    {
        movementDifficultyTypeSettingSlot.SetCurrentSlotIndex(movementDifficulty);
        PlayerMovement.UpdateMovementDifficulty(movementDifficulty);
    }
    #endregion

    #region Audio
    /// <summary>
    /// Toggles the audio on and off.
    /// </summary>
    public void SetAudio()
    {
        enableAudio = !enableAudio;
        RefreshAudio();
    }

    /// <summary>
    /// Refreshed the display setting and its hook.
    /// </summary>
    private void RefreshAudio()
    {
        var index = enableAudio ? 0 : 1;
        audioToggleSettingSlot.SetCurrentSlotIndex(index);
        AudioListener.volume = System.Convert.ToInt32(enableAudio);
    }
    #endregion
    #endregion
}
