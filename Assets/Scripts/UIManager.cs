/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: DefaultCompany
 * Project: Apple Basket
 * Creation Date: 1/6/2023 10:25:04 AM
 * 
 * Description: Handles the functionality of all
 *              UI assets.
*********************************/
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    #region Fields
    /// <summary>
    /// The instance of the UI manager in the scene.
    /// </summary>
    private static UIManager instance;

    private static int timerStartingAmount = 0;

    // UI objects
    [SerializeField] private TextMeshProUGUI countDown;
    [SerializeField] private TextMeshProUGUI score;
    [SerializeField] private TextMeshProUGUI timerUI;
    [SerializeField] private TextMeshProUGUI combo;
    [SerializeField] private TextMeshProUGUI endMessage;

    private static TextMeshProUGUI CountDown;
    private static TextMeshProUGUI Score;
    private static TextMeshProUGUI TimerUI;
    private static TextMeshProUGUI Combo;
    private static TextMeshProUGUI EndMessage;

    // Images
    [SerializeField] private Image timerBar1;
    [SerializeField] private Image timerBar2;

    private static Image TimerBar1;
    private static Image TimerBar2;

    
    [Tooltip("The end game display for the amount of good apples caught")]
    [SerializeField] private EndGameDataDisplay goodApplesEndGame;
    
    [Tooltip("The end game display for the amount of bad apples caught")]
    [SerializeField] private EndGameDataDisplay badApplesEndGame;

    [Tooltip("The end game display for the amount of good apples missed")]
    [SerializeField] private EndGameDataDisplay goodApplesMissedEndGame;

    [Tooltip("The end game display for the amount of score the player earned")]
    [SerializeField] private EndGameDataDisplay scoreEndGame;
    
    [Tooltip("The end game display for the highest combo reached")]
    [SerializeField] private EndGameDataDisplay highestComboEndGame;

    private static EndGameDataDisplay GoodApplesEndGame;
    private static EndGameDataDisplay BadApplesEndGame;
    private static EndGameDataDisplay GoodApplesMissedEndGame;
    private static EndGameDataDisplay ScoreEndGame;
    private static EndGameDataDisplay HighestComboEndGame;
    #endregion

    #region Functions
    #region Initialization
    /// <summary>
    /// Initializes all aspects of the UI manager.
    /// </summary>
    private void Awake()
    {
        instance = this;
        GetUIReferences();
    }

    /// <summary>
    /// Gets references to all of the UI objects.
    /// </summary>
    private void GetUIReferences()
    {
        #region Countdown
        CountDown = countDown;
        countDown.gameObject.SetActive(false);
        #endregion

        #region Score
        Score = score;
        #endregion

        #region Timer
        TimerUI = timerUI;
        TimerBar1 = timerBar1;
        TimerBar2 = timerBar2;
        #endregion

        #region End Message
        EndMessage = endMessage;
        #endregion

        #region Combo
        Combo = combo;
        UpdateCombo(0);
        #endregion

        #region End Game Data
        GoodApplesEndGame = goodApplesEndGame;
        BadApplesEndGame = badApplesEndGame;
        GoodApplesMissedEndGame = goodApplesMissedEndGame;
        ScoreEndGame = scoreEndGame;
        HighestComboEndGame = highestComboEndGame;
        #endregion
    }

    private void Start()
    {
        EndMessage.gameObject.SetActive(false);
    }
    #endregion

    #region UI Updates
    /// <summary>
    /// Sets the new count in the countdown.
    /// </summary>
    /// <param name="newCount">The current count.</param>
    public static void UpdateCountdown(int newCount)
    {
        if (InstanceDoesntExist() || IsntValid(CountDown)) return;

        // Updates the countdown UI 
        CountDown.text = newCount.ToString();
        CountDown.gameObject.SetActive(true);
    }

    /// <summary>
    /// Updates the dipslay of the current score.
    /// </summary>
    /// <param name="newScore">The current score the player has.</param>
    public static void UpdateScore(int newScore)
    {
        if (InstanceDoesntExist() || IsntValid(Score)) return;

        // Updates the score UI 
        Score.text = newScore.ToString();
        UpdateScoreEndGame(newScore);
    }

    /// <summary>
    /// Updates the displayed score for at the end of the game.
    /// </summary>
    /// <param name="newScore">The new score the player has.</param>
    private static void UpdateScoreEndGame(int newScore)
    {
        if (IsntValid(ScoreEndGame)) return;
        ScoreEndGame.UpdateText(newScore.ToString());
    }

    /// <summary>
    /// Updates the count of apples displayed at the end of the game.
    /// </summary>
    /// <param name="goodApples">The amount of good apples.</param>
    /// <param name="badApples">The amount of bad apples.</param>
    public static void UpdateAppleCount(int goodApples, int badApples)
    {
        if (InstanceDoesntExist() || IsntValid(GoodApplesEndGame) || IsntValid(BadApplesEndGame)) return;
        GoodApplesEndGame.UpdateText(goodApples.ToString());
        BadApplesEndGame.UpdateText(badApples.ToString());
    }

    /// <summary>
    /// Updates the count of apples displayed at the end of the game.
    /// </summary>
    /// <param name="goodApples">The amount of good apples.</param>
    /// <param name="badApples">The amount of bad apples.</param>
    public static void UpdateApplesMissedCount(int goodApplesMissed)
    {
        if (InstanceDoesntExist() || IsntValid(GoodApplesMissedEndGame)) return;
        GoodApplesMissedEndGame.UpdateText(goodApplesMissed.ToString());
    }

    #region Timer
    public static void InitializeTimer(int startingTime)
    {
        timerStartingAmount = startingTime;

        if(startingTime != -1) UpdateTimer(startingTime);
        else
        {
            TimerUI.fontSize *= 2.0f;
            TimerUI.text = "\u221E";
            var pos = TimerUI.transform.position;
            pos.y += 6.0f;
            TimerUI.transform.position = pos;
        }
    }

    /// <summary>
    /// Updates the timer to its current time.
    /// </summary>
    /// <param name="newTime">The current time left of the timer.</param>
    public static void UpdateTimer(float newTime)
    {
        if (InstanceDoesntExist() || IsntValid(TimerUI)) return;

        // Updates the timer UI 
        TimerUI.text = GetTimerValue(newTime);
        TimerUI.gameObject.SetActive(newTime != 0);

        UpdateTimerBars(newTime);
    }

    public static string GetTimerValue(float newTime)
    {
        var seconds = (int)newTime;
        var minutes = seconds / 60;
        var leftOverSeconds = (seconds - (minutes * 60));
        string secondsDisplayed = "";

        if (leftOverSeconds < 10) secondsDisplayed += "0";
        secondsDisplayed += leftOverSeconds;

        return minutes.ToString() + ":" + secondsDisplayed;
    }

    private static void UpdateTimerBars(float newTime)
    {
        if (IsntValid(TimerBar1) || IsntValid(TimerBar2)) return;

        TimerBar1.fillAmount = newTime / timerStartingAmount;
        TimerBar2.fillAmount = newTime / timerStartingAmount;
    }

    public IEnumerator UpdateTimerBarsRoutine(int newTime)
    {
        var t = (float)newTime;
        var oneLess = newTime - 1;

        do
        {
            t -= Time.deltaTime;

            timerBar1.fillAmount = t / timerStartingAmount;
            timerBar2.fillAmount = t / timerStartingAmount;

            yield return new WaitForEndOfFrame();
        }
        while (t >= oneLess);
    }
    #endregion

    #region Combo
    /// <summary>
    /// Updates the displayed combo during the game.
    /// </summary>
    /// <param name="newCombo">The new combo of the player.</param>
    public static void UpdateCombo(int newCombo)
    {
        if (InstanceDoesntExist() || IsntValid(Combo)) return;

        Combo.text = "x" + newCombo.ToString();
    }

    /// <summary>
    /// Updates the highest displayed combo at the end of the game.
    /// </summary>
    /// <param name="highestCombo">The highest combo to display.</param>
    public static void UpdateHighestCombo(int highestCombo)
    {
        if (IsntValid(HighestComboEndGame)) return;
        HighestComboEndGame.UpdateText(highestCombo.ToString());
    }
    #endregion

    #region Display End Message
    /// <summary>
    /// Displays the message that should appear at the end of the game.
    /// </summary>
    public static void DisplayEndMessage()
    {
        if (InstanceDoesntExist() || IsntValid(EndMessage)) return;

        EndMessage.gameObject.SetActive(true);
    }
    #endregion
    #endregion

    #region Null Checks
    private static bool IsntValid(Component uiObject)
    {
        return uiObject == null;
    }

    private static bool InstanceDoesntExist()
    {
        return instance == null;
    }
    #endregion
    #endregion
}
