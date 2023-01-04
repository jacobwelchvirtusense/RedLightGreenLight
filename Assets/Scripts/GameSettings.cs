/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Red Light Green Light
 * Creation Date: 1/3/2023 4:36:41 PM
 * 
 * Description: TODO
*********************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    #region Fields
    private static GameSettings gameSettings;

    #region Game Settings
    #region Movement Tracking Method
    /// <summary>
    /// The difficulty for tracking movement. Includes movement threshold and time before movement detection.
    /// </summary>
    public enum MovementTrackingMethod { EASY, MEDIUM, HARD }

    [field: Header("Game Settings")]
    [Tooltip("The current difficuly for tracking the players movement during a red light")]
    [SerializeField] private MovementTrackingMethod movementTrackingMethod = MovementTrackingMethod.MEDIUM;

    public static MovementTrackingMethod CurrentMovementTrackingMethod
    {
        get
        {
            if (gameSettings == null) return MovementTrackingMethod.MEDIUM;

            return gameSettings.movementTrackingMethod;
        }
    }
    #endregion

    #region Game Mode
    /// <summary>
    /// The game modes that can be played.
    /// </summary>
    public enum GameMode { RACE, STATIONARY, WHEEL }

    [Tooltip("The current game mode for this scene")]
    [SerializeField] private GameMode gameMode = GameMode.RACE;

    public static GameMode CurrentGameMode
    {
        get
        {
            if (gameSettings == null) return GameMode.STATIONARY;

            return gameSettings.gameMode;
        }
    }
    #endregion
    #endregion
    #endregion

    #region Functions
    /// <summary>
    /// Initializes the game settings in this scene.
    /// </summary>
    private void Awake()
    {
        gameSettings = this;   
    }
    #endregion
}
