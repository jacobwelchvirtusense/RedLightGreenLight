/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Red Light Green Light
 * Creation Date: 12/27/2022 11:38:08 AM
 * 
 * Description: Handles the movement and tracking of movement
 *              for the player.
*********************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Windows.Kinect;
using Joint = Windows.Kinect.Joint;

using static GameSettings;
using static GameController;
using System;
using System.Linq;
using Unity.VisualScripting;

public class PlayerMovement : MonoBehaviour
{
    #region Fields
    /// <summary>
    /// Holds true if the user has failed the current red light
    /// </summary>
    private bool hasFailedRedLight = false;

    /// <summary>
    /// The size of the player model.
    /// </summary>
    private const float playerSize = 10.0f;

    private static PlayerMovement instance;

    #region Speed
    [Header("Speed")]
    [Range(0.0f, 20.0f)]
    [Tooltip("The amount of time lerping in and out of movement")]
    [SerializeField] private float movementSmoothTime = 0.2f;

    /// <summary>
    /// Reference to the player's current movement speed.
    /// </summary>
    private float currentMovementSpeed = 0.0f;
    #endregion

    #region Race Fields
    // Starting values of the user
    private float startingRaceZ;
    private float startingSensorDistance;

    // Current sensor value tracking
    private float currentSensorDistance;
    private float currentBestSensorDistance = Mathf.Infinity;
    private float currentBestGreenLightDistance = Mathf.Infinity;
    private float currentCharacterDistance = 0;

    [Header("Race")]
    [Range(0.0f, 20.0f)]
    [Tooltip("The speed of the player when moving")]
    [SerializeField]
    private float movementSpeedRace = 7.0f;

    [Range(0.0f, 10.0f)]
    [Tooltip("The amount of leeway alloted to players when having them return to the start")]
    [SerializeField] private float returnDistanceLeeway = 0.25f;

    [Range(0.0f, 10.0f)]
    [Tooltip("The amount of leeway alloted to players when having them return to the start")]
    [SerializeField] private float penaltyDetectionLeeway = 0.15f;

    [Range(0.0f, 10.0f)]
    [Tooltip("The amount of time to keep player animations during the race game mode")]
    [SerializeField] private float raceAnimationDuration = 0.5f;

    [Range(0.0f, 10.0f)]
    [Tooltip("The distance from the sensor where the patient wins")]
    [SerializeField] private float winningDistance = 0.1f;
    #endregion

    #region Stationary Fields
    [Header("Stationary")]
    [Range(0.0f, 20.0f)]
    [Tooltip("The speed of the player when moving")]
    [SerializeField]
    private float movementSpeedStationary = 7.0f;

    [Range(0.0f, 5.0f)]
    [Tooltip("The input delay between acceptable inputs")]
    [SerializeField] private float movementAnimationThreshold = 1.5f;

    [Range(0, 120)]
    [Tooltip("The max size of frames to hold for checking if players have moved feed")]
    [SerializeField] private int maxQueueSize = 25;

    [Range(0, 20)]
    [Tooltip("The max size of frames to check for wobble")]
    [SerializeField] private int wobbleCheckSize = 5;

    public enum MovementThresholdDifficulty { EASY, MEDIUM, HARD }

    [Tooltip("The current threshold difficulty for the player's movements")]
    [field:SerializeField] public MovementThresholdDifficulty CurrentMovementDifficulty { get; private set; } = MovementThresholdDifficulty.MEDIUM;
    public static MovementThresholdDifficulty CurrentMovementDifficultyAccessor { get=>instance.CurrentMovementDifficulty; }

    [SerializeField]
    private bool isKnee = false;

    [Range(0.0f, 10.0f)]
    [Tooltip("The minimum height a patients foot must be raised by")]
    [SerializeField] private float[] minUpHeightKnee = new float[] { 0.025f, 0.035f, 0.045f };

    private float MinUpHeightKnee { get => minUpHeightKnee[(int)CurrentMovementDifficulty]; }

    [Range(0.0f, 10.0f)]
    [Tooltip("The maximum height a patients foot may be raised by (limits movementspeed")]
    [SerializeField] private float[] maxUpHeightKnee = new float[] { 0.08f, 0.09f, 0.1f };

    private float MaxUpHeightKnee { get => maxUpHeightKnee[(int)CurrentMovementDifficulty]; }

    [Range(0.0f, 10.0f)]
    [Tooltip("The minimum height a patients foot must be raised by")]
    [SerializeField] private float minUpHeightFoot = 0.1f;

    [Range(0.0f, 10.0f)]
    [Tooltip("The maximum height a patients foot may be raised by (limits movementspeed")]
    [SerializeField] private float maxUpHeightFoot = 0.4f;

    [Space(InspectorValues.SPACE_BETWEEN_EDITOR_ELEMENTS)]

    #region Movement Times
    [Range(0.0f, 2.0f)]
    [Tooltip("The amount of time movement lasts for")]
    [SerializeField] private float minMovementTime = 0.5f;

    [Range(0.0f, 2.0f)]
    [Tooltip("The amount of time movement lasts for")]
    [SerializeField] private float maxMovementTime = 1.0f;
    #endregion

    [Space(InspectorValues.SPACE_BETWEEN_EDITOR_ELEMENTS)]

    [Range(0.0f, 2.0f)]
    [Tooltip("The minimum speed modifier")]
    [SerializeField] private float minStationarySpeed = 0.5f;

    [Range(0.0f, 2.0f)]
    [Tooltip("The maximum speed modifier")]
    [SerializeField] private float maxStationarySpeed = 1.5f;

    [Range(0.0f, 2.0f)]
    [Tooltip("The average time between steps for the minimum speed mod value")]
    [SerializeField] private float minSpeedAverageTimeBetween = 0.6f;

    [Range(0.0f, 2.0f)]
    [Tooltip("The average time between steps for the maximum speed mod value")]
    [SerializeField] private float maxSpeedAverageTimeBetween = 0.2f;

    [Space(InspectorValues.SPACE_BETWEEN_EDITOR_ELEMENTS)]

    [Range(0.0f, 2.0f)]
    [Tooltip("The input delay between acceptable inputs")]
    [SerializeField] private float jointInputDelay = 0.15f;

    [Range(0.0f, 2.0f)]
    [Tooltip("How long each speed increment is held for")]
    [SerializeField] private float inputSpeedHoldTime = 0.5f;

    [Space(InspectorValues.SPACE_BETWEEN_EDITOR_ELEMENTS)]

    [Header("Speed Lines")]
    [Tooltip("The particle system that renders the speed lines")]
    [SerializeField] private ParticleSystem speedLines;

    [Range(0.0f, 1.0f)]
    [Tooltip("The min threshold for speed lines to appear")]
    [SerializeField] private float speedLinesThreshold = 0.15f;

    [Range(0.0f, 1.0f)]
    [Tooltip("The min alpha color for speed lines")]
    [SerializeField] private float minSpeedLinesAlpha = 0.15f;

    [Range(0.0f, 1.0f)]
    [Tooltip("The max alpha color for speed lines")]
    [SerializeField] private float maxSpeedLinesAlpha = 0.8f;

    [Range(0.0f, 2.0f)]
    [Tooltip("The min simulaiton speed of particles for speed lines")]
    [SerializeField] private float minSpeedLinesSimulation = 0.5f;

    [Range(0.0f, 2.0f)]
    [Tooltip("The max simulaiton speed of particles for speed lines")]
    [SerializeField] private float maxSpeedLinesSimulation = 1.0f;

    /// <summary>
    /// The current speed modifier for the player.
    /// </summary>
    private float stationarySpeedModifier = 0.0f;

    private float StationarySpeedModifier { get => Mathf.Clamp(stationarySpeedModifier, 0.0f, maxStationarySpeed); }

    /// <summary>
    /// Holds reference to the y positions of players feet.
    /// </summary>
    private Dictionary<JointType, Queue<Vector3>> footTracking = new Dictionary<JointType, Queue<Vector3>>();

    /// <summary>
    /// Holds reference to the y positions of players feet.
    /// </summary>
    private Queue<float> inputTimes = new Queue<float>();

    /// <summary>
    /// Holds reference to the y positions of players feet.
    /// </summary>
    private Dictionary<JointType, bool> footAcceptingInputs = new Dictionary<JointType, bool>();
    #endregion

    #region Animations
    /// <summary>
    /// The animator for the player model.
    /// </summary>
    private Animator playerAnimator;

    /// <summary>
    /// The current amount of time left in the movement animation duration.
    /// </summary>
    private float currentMovementDuration = 0.0f;

    // Animator ID for when the player is moving
    private string walkAnimationTag = "IsWalking";
    private int walkID;

    // Animator ID for when the player is breathing
    private string breatheAnimationTag = "IsBreathing";
    private int breatheID;

    // Animator ID for failing a red light in the stationary mode
    private string hitAnimationTag = "IsHit";
    private int hitID;

    // Animator ID for failing a red light in the race mode
    private string deathAnimationTag = "IsDead";
    private int deathID;

    // Animator ID for winning a race
    private string winAnimaitonTag = "HasWon";
    private int winID;

    // Animator ID for reseting animations
    private string resetAnimaitonTag = "Reset";
    private int resetID;
    #endregion

    //TODO
    #region Kinnect
    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager bodyManager;

    private JointType[] joints;
    private JointType[] kneeJoints = new JointType[]
    {
        JointType.KneeLeft,
        JointType.KneeRight
    };

    private JointType[] footJoints = new JointType[]
    {
        JointType.AnkleLeft,
        JointType.AnkleRight
    };
    #endregion
    #endregion

    #region Functions
    #region Initialization
    /// <summary>
    /// Performs all actions in the awake event.
    /// </summary>
    private void Awake()
    {
        startingRaceZ = transform.position.z;
        speedLines.gameObject.SetActive(false);
        instance = this;
        ResetGameEvent.AddListener(ResetPlayerMovement);

        InitializeComponents();
        InitializeDictionaries();
        InitializeAnimationIDs();
    }

    private void ResetPlayerMovement()
    {
        #region Reset Position
        var pos = transform.position;
        pos.z = startingRaceZ;
        transform.position = pos;
        #endregion

        #region Reset Animation
        playerAnimator.SetTrigger(resetID);
        playerAnimator.SetBool(winID, false);
        playerAnimator.SetBool(walkID, false);
        playerAnimator.SetBool(breatheID, false);
        #endregion

        #region Reset Particles
        speedLines.gameObject.SetActive(false);
        #endregion
    }

    /// <summary>
    /// Initializes all components for the player.
    /// </summary>
    private void InitializeComponents()
    {
        playerAnimator = GetComponentInChildren<Animator>();
        bodyManager = FindObjectOfType<BodySourceManager>();
    }

    /// <summary>
    /// Initializes the dictionaries with values.
    /// </summary>
    private void InitializeDictionaries()
    {
        joints = isKnee ? kneeJoints : footJoints;

        foreach(JointType joint in joints)
        {
            footTracking.Add(joint, new Queue<Vector3>());
            footAcceptingInputs.Add(joint, true);
        }
    }

    /// <summary>
    /// Stores the hashed animation tags (this is more optimized than hashing it every time an animation is used).
    /// </summary>
    private void InitializeAnimationIDs()
    {
        walkID = Animator.StringToHash(walkAnimationTag);
        hitID = Animator.StringToHash(hitAnimationTag);
        deathID = Animator.StringToHash(deathAnimationTag);
        winID = Animator.StringToHash(winAnimaitonTag);
        breatheID = Animator.StringToHash(breatheAnimationTag);
        resetID = Animator.StringToHash(resetAnimaitonTag);
    }
    #endregion

    public static void UpdateMovementDifficulty(int newIndex)
    {
        instance.CurrentMovementDifficulty = (MovementThresholdDifficulty)newIndex;
    }

    #region Input Handling
    /// <summary>
    /// Updates game from Kinnect data.
    /// </summary>
    private void FixedUpdate()
    {
        #region Get Kinect Data
        if (bodyManager == null) return;

        Body[] _data = bodyManager.GetData();

        if (_data == null) return;

        List<ulong> _trackedIds = new List<ulong>();

        ulong centerID = 0;
        float currentLow = Mathf.Infinity;

        foreach (var body in _data)
        {
            if (body == null) continue;

            var lowCheck = Mathf.Abs(body.Joints[JointType.SpineBase].Position.X);

            if (body.IsTracked && lowCheck < currentLow)
            {
                centerID = body.TrackingId;
                currentLow = lowCheck;
            }
        }

        if (centerID != 0) _trackedIds.Add(centerID);
        #endregion

        #region Delete Untracked Bodies
        List<ulong> _knownIds = new List<ulong>(_Bodies.Keys);

        foreach (ulong trackingId in _knownIds)
        {
            if (!_trackedIds.Contains(trackingId))
            {
                Destroy(_Bodies[trackingId]);
                _Bodies.Remove(trackingId);
            }
        }
        #endregion

        #region Create & Refresh Kinect Bodies
        foreach (var body in _data)
        {
            if (body == null) continue;

            if (body.IsTracked && body.TrackingId == centerID)
            {
                if (!_Bodies.ContainsKey(body.TrackingId))
                {
                    var startingPosition = body.Joints[JointType.SpineBase].Position;
                    startingSensorDistance = Length(startingPosition);
                    currentCharacterDistance = startingSensorDistance;
                    _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                }

                RefreshBodyObject(body);
            }
        }
        #endregion
    }

    /// <summary>
    /// Creates the body in the scene.
    /// </summary>
    /// <param name="id">The id of the body to be created.</param>
    /// <returns></returns>
    private GameObject CreateBodyObject(ulong id)
    {
        GameObject body = new GameObject("Body:" + id);

        return body;
    }

    /// <summary>
    /// Takes kinnect input to move the player in the game.
    /// </summary>
    /// <param name="body">The kinnect body of the user.</param>
    private void RefreshBodyObject(Body body)
    {
        switch (CurrentGameMode)
        {
            case GameMode.RACE:
                RaceMovement(body);
                break;

            case GameMode.STATIONARY:
            default:
                if(gameController.lightState != LightState.OFF)
                {
                    foreach (JointType joint in joints)
                    {
                        Joint sourceJoint = body.Joints[joint];

                        StationaryMovement(sourceJoint, joint, body);
                    }

                    UpdateCharacterPositionStationary();
                }
                break;
        }


    }

    float minY, maxY;
    float minZ, maxZ;

    private bool WobbleDetection(List<Vector3> trackingQueue)
    {
        if (trackingQueue.Count < 3) return false;

        var first = trackingQueue.First();
        var last = trackingQueue.Last();
        var displacement = (last - first).magnitude;
        Vector3 minVector = first, maxVector = last;

        foreach(var pos in trackingQueue)
        {
            minVector = Vector3.Min(minVector, pos);
            maxVector = Vector3.Max(maxVector, pos);
        }

        minY = minVector.y;
        maxY = maxVector.y;
        minZ = minVector.z;
        maxZ = maxVector.z;

        var diff = maxVector - minVector;
        var maxDistance = diff.magnitude;
        var threshold = displacement * 1.25f;

        if(maxDistance > 0.0254f * 3)
        {
            //print("Failed Wobble Test");
        }

        if (isKnee) return false;

        return maxDistance > 0.0254f*5;
    }
    #endregion

    #region Race Movement
    /// <summary>
    /// Handles movement for the race game mode based on the kinnect input.
    /// </summary>
    /// <param name="body">The body to use data from for movement.</param>
    private void RaceMovement(Body body)
    {
        // Gets user distance from the sensor
        var currentPosition = body.Joints[JointType.SpineBase].Position;
        var distance = Length(currentPosition);
        currentSensorDistance = distance;

        // Updates character position, checks win state, and check fail state
        if (!hasFailedRedLight && gameController.lightState != LightState.OFF) 
        {
            if (distance < currentBestSensorDistance)
            {
                currentBestSensorDistance = distance;
                RefreshRaceAnimaitons();
                CheckPlayerWinRace();
            }

            // Sets character position
            currentCharacterDistance = Mathf.Lerp(currentCharacterDistance, currentBestSensorDistance, Time.fixedDeltaTime * movementSpeedRace);
            if (currentBestSensorDistance != Mathf.Infinity) SetPlayerZ(currentCharacterDistance);

            RaceStateHandling(distance);
        }
    }

    /// <summary>
    /// Handles movement based on the current state of the game.
    /// </summary>
    /// <param name="distance">The users current distance from the sensor.</param>
    private void RaceStateHandling(float distance)
    {
        switch (gameController.lightState)
        {
            // Checks if the user has move too much during a red light
            case LightState.RED:
                var distanceLeewayOvershot = distance + penaltyDetectionLeeway < currentBestGreenLightDistance;
                if (gameController.canDetectPenaltyMovement && distanceLeewayOvershot) StartCoroutine(FailRedLight());
                break;

            // Updates the current farthest distance reached during a red light
            case LightState.GREEN:
            default:
                if (currentBestSensorDistance < currentBestGreenLightDistance) currentBestGreenLightDistance = distance;
                break;
        }
    }

    /// <summary>
    /// Sets the position of the character.
    /// </summary>
    /// <param name="distance"></param>
    private void SetPlayerZ(float distance)
    {
        var pos = transform.position;
        pos.z = (startingSensorDistance - distance) * playerSize + startingRaceZ;

        transform.position = pos;

        print("Set Z");
    }

    #region Movement Animations
    /// <summary>
    /// Refreshes the timer on the race movement animations.
    /// </summary>
    private void RefreshRaceAnimaitons()
    {
        if (currentMovementDuration == 0)
        {
            playerAnimator.SetBool(walkID, true);
            playerAnimator.SetBool(breatheID, true);

            StartCoroutine(StopMovingRaceAnimations());
        }

        currentMovementDuration = raceAnimationDuration;
    }

    /// <summary>
    /// Loops until the movement animation should be stopped.
    /// </summary>
    /// <returns></returns>
    private IEnumerator StopMovingRaceAnimations()
    {
        do
        {
            yield return new WaitForFixedUpdate();
            currentMovementDuration -= Time.fixedDeltaTime;
        }
        while (currentMovementDuration > 0);

        currentMovementDuration = 0;
        playerAnimator.SetBool(walkID, false);
    }
    #endregion

    #region Fail
    /// <summary>
    /// Handles the event of failing a red light and having the user return to start.
    /// </summary>
    /// <returns></returns>
    private IEnumerator MakePlayerRestart()
    {
        SetPlayerZ(startingSensorDistance);

        while (currentSensorDistance < startingSensorDistance - returnDistanceLeeway)
        {
            yield return new WaitForFixedUpdate();
        }

        gameController.RestartGameRace();
    }

    /// <summary>
    /// Resets race fields to their starting amount.
    /// </summary>
    private void ResetRaceValues()
    {
        currentBestSensorDistance = Mathf.Infinity;
        currentBestGreenLightDistance = Mathf.Infinity;
        currentCharacterDistance = startingSensorDistance;
    }
    #endregion

    /// <summary>
    /// Checks if the player has won the race.
    /// </summary>
    private void CheckPlayerWinRace()
    {
        if (winningDistance > currentBestSensorDistance)
        {
            playerAnimator.SetTrigger(winID);
            StartCoroutine(gameController.EndGame());
        }
    }

    /// <summary>
    /// Calculates the users distance from the camera.
    /// </summary>
    /// <param name="point">The camera space kinnect position to check for distance.</param>
    /// <returns></returns>
    private float Length(CameraSpacePoint point)
    {
        return Mathf.Sqrt(
            point.X * point.X +
            point.Y * point.Y +
            point.Z * point.Z
        );
    }
    #endregion

    //TODO Currently working on updating the movement detection
    #region Stationary Movement
    private void StationaryMovement(Joint sourceJoint, JointType joint, Body body)
    {
        var footTrackingQueue = footTracking[joint];
        footTrackingQueue.Enqueue(new Vector3(sourceJoint.Position.X, sourceJoint.Position.Y, sourceJoint.Position.Z));

        if (footTrackingQueue.Count == maxQueueSize)
        {
            if (!WobbleDetection(footTrackingQueue.ToList().GetRange(maxQueueSize-wobbleCheckSize, wobbleCheckSize)))
            {
                if (isKnee)
                {
                    if (minZ == sourceJoint.Position.Z)
                    {
                        var distUp = maxZ - sourceJoint.Position.Z;

                        if (distUp > MinUpHeightKnee)
                        {
                            if (footAcceptingInputs[joint])
                            {
                                StartCoroutine(DelayFootInputStationary(joint));
                                StartCoroutine(HoldInput());
                            }

                            var newHeight = Mathf.Clamp(distUp, MinUpHeightKnee, MaxUpHeightKnee);
                            CheckStateStationary(newHeight);
                        }
                    }
                }
                else
                {
                    if (maxY == sourceJoint.Position.Y)
                    {
                        var distUp = sourceJoint.Position.Y - minY;
                        print("Dist Up: " + distUp);

                        if (distUp > minUpHeightFoot)
                        {
                            var newHeight = Mathf.Clamp(distUp, minUpHeightFoot, maxUpHeightFoot);
                            CheckStateStationary(newHeight);
                        }
                    }
                }

            }

            footTrackingQueue.Dequeue();
        }
    }

    private IEnumerator DelayFootInputStationary(JointType joint)
    {
        footAcceptingInputs[joint] = false;

        yield return new WaitForSeconds(jointInputDelay);

        footAcceptingInputs[joint] = true;
    }

    #region Speed Modifier
    private IEnumerator HoldInput()
    {
        inputTimes.Enqueue(Time.time);
        yield return new WaitForSeconds(inputSpeedHoldTime);
        inputTimes.Dequeue();
    }

    private float CalculateSpeedModifier()
    {
        if (inputTimes.Count == 0) return 0;
        else if (inputTimes.Count == 1) return minStationarySpeed;
        else
        {
            List<float> timeBetween = new List<float>();
            var inputTimesList = inputTimes.ToList();

            for (int i = 0; i < inputTimes.Count - 1; i++)
            {
                timeBetween.Add(Mathf.Abs(inputTimesList[i] - inputTimesList[i + 1]));
            }

            /*
            print("Average: " + timeBetween.Average());
            print("Count: " + timeBetween.Count);*/

            var inverseLerp = Mathf.InverseLerp(minSpeedAverageTimeBetween, maxSpeedAverageTimeBetween, timeBetween.Average());

            return Mathf.Lerp(minStationarySpeed, maxStationarySpeed, inverseLerp);
        }
    }
    #endregion

    /// <summary>
    /// Gets the vector3 position of the joint object.
    /// </summary>
    /// <param name="joint">The joint whose position is being coverted to Vector3.</param>
    /// <returns></returns>
    private Vector3 GetVector3FromJoint(Joint joint)
    {
        return new Vector3(joint.Position.X * playerSize, joint.Position.Y * playerSize, joint.Position.Z * playerSize);
    }

    private void CheckStateStationary(float heightReached)
    {
        if (gameController == null) return;

        switch (gameController.lightState)
        {
            case LightState.RED:
                if (!hasFailedRedLight && gameController.canDetectPenaltyMovement) StartCoroutine(FailRedLight());
                break;

            case LightState.GREEN:
                var inverseLerp = isKnee ? Mathf.InverseLerp(MinUpHeightKnee, MaxUpHeightKnee, heightReached) : Mathf.InverseLerp(minUpHeightFoot, maxUpHeightFoot, heightReached);
                var time = Mathf.Lerp(minMovementTime, maxMovementTime, inverseLerp);

                if (currentMovementDuration == 0)
                {
                    currentMovementDuration = time;
                    StartCoroutine(MoveCharacterStationary());
                }
                else if(currentMovementDuration < time)
                {
                    currentMovementDuration = time;
                }
                break;

            case LightState.OFF:
            default:
                break;
        }
    }

    #region Character Movement
    /// <summary>
    /// Updates the current duration of the stationary movement.
    /// </summary>
    /// <returns></returns>
    private IEnumerator MoveCharacterStationary()
    {
        while (currentMovementDuration > 0)
        {
            yield return new WaitForEndOfFrame();

            currentMovementDuration -= Time.deltaTime;
        }

        currentMovementDuration = 0;
    }

    /// <summary>
    /// Updates the characters position in the stationary game mode.
    /// </summary>
    private void UpdateCharacterPositionStationary()
    {
        var modifier = currentMovementDuration != 0 ? 1 : 0;

        stationarySpeedModifier = CalculateSpeedModifier();
        currentMovementSpeed = Mathf.Lerp(currentMovementSpeed, movementSpeedStationary * StationarySpeedModifier * modifier, Time.fixedDeltaTime*movementSmoothTime);

        // Sets animation active/inactive
        bool isWalking = currentMovementSpeed > movementAnimationThreshold && !hasFailedRedLight && gameController.lightState != LightState.OFF;
        playerAnimator.SetBool(walkID, isWalking);
        playerAnimator.SetBool(breatheID, playerAnimator.GetBool(breatheID) || isWalking);

        CheckSpeedLineState();

        if (isWalking)
        {
            if(gameController.lightState == LightState.GREEN || gameController.lightState == LightState.YELLOW) gameController.UpdatePoints(0, StationarySpeedModifier);

            transform.position += transform.forward * Time.fixedDeltaTime * currentMovementSpeed;
        }
        else
        {
            speedLines.gameObject.SetActive(false);
        }
    }

    private void CheckSpeedLineState()
    {
        var inverseSpeedLerp = Mathf.InverseLerp(minStationarySpeed, maxStationarySpeed, StationarySpeedModifier);

        if (inverseSpeedLerp >= speedLinesThreshold)
        {
            speedLines.gameObject.SetActive(true);
            SetSpeedLines(inverseSpeedLerp);
        }
        else
        {
            speedLines.gameObject.SetActive(false);
        }
    }

    private void SetSpeedLines(float lerp)
    {
        print("Inverse: " + lerp);

        var minColor = Color.white;
        minColor.a = minSpeedLinesAlpha;
        var maxColor = Color.white;
        maxColor.a = maxSpeedLinesAlpha;

        speedLines.startColor = Color.Lerp(minColor, maxColor, lerp);
        speedLines.playbackSpeed = Mathf.Lerp(minSpeedLinesSimulation, maxSpeedLinesSimulation, lerp);
    }
    #endregion
    #endregion

    #region Win Event
    public void PlayWinAnimation()
    {
        if(transform.position.z < -30)
        {
            var zPos = transform.position;
            zPos.z += 229.8f;
            transform.position = zPos;
        }

        speedLines.gameObject.SetActive(false);

        playerAnimator.SetInteger("Dance Number", UnityEngine.Random.Range(0, 5));
        playerAnimator.SetBool(winID, true);
    }
    #endregion

    #region Fail Event
    public static void ForceRedLightFail()
    {
        instance.StartCoroutine(instance.FailRedLight());
    }

    /// <summary>
    /// Handles the event of failing a red light.
    /// </summary>
    /// <returns></returns>
    private IEnumerator FailRedLight()
    {
        // Handles fail animations
        var animID = CurrentGameMode == GameMode.RACE ? deathID : hitID;
        playerAnimator.SetTrigger(animID);

        // Handles reset event
        hasFailedRedLight = true;
        ResetRaceValues();
        yield return gameController.FailedRedRoutine();

        #region Checks for player returning to start
        if (CurrentGameMode == GameMode.RACE)
        {
            yield return MakePlayerRestart();
        }
        #endregion

        if(!TutorialManager.IsPlaying)
        hasFailedRedLight = false;
    }

    public static void AllowMovementAgain()
    {
        instance.hasFailedRedLight = false;
    }
    #endregion
    #endregion
}
