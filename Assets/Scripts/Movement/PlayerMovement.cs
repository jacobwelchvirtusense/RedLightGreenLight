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

    [Range(0.0f, 10.0f)]
    [Tooltip("The minimum height a patients foot must be raised by")]
    [SerializeField] private float minUpHeightKnee = 0.1f;

    [Range(0.0f, 10.0f)]
    [Tooltip("The maximum height a patients foot may be raised by (limits movementspeed")]
    [SerializeField] private float maxUpHeightKnee = 0.4f;

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
    [Tooltip("The input delay between acceptable inputs")]
    [SerializeField] private float jointInputDelay = 0.15f;

    [Range(0.0f, 2.0f)]
    [Tooltip("How long each speed increment is held for")]
    [SerializeField] private float inputSpeedHoldTime = 0.5f;

    [Range(0.0f, 2.0f)]
    [Tooltip("The increment for each input")]
    [SerializeField] private float inputSpeedIncrement = 0.3f;

    [Range(0.0f, 2.0f)]
    [Tooltip("The increment for each input")]
    [SerializeField] private float speedIncrementEaseInTime = 0.3f;

    [Range(0.0f, 2.0f)]
    [Tooltip("The increment for each input")]
    [SerializeField] private float speedIncrementEaseOutTime = 0.3f;

    [Range(0.0f, 5.0f)]
    [Tooltip("The max speed modifier the user can reach")]
    [SerializeField] private float maxStationarySpeedModifier = 2.0f;

    /// <summary>
    /// The current speed modifier for the player.
    /// </summary>
    private float stationarySpeedModifier = 0.0f;

    private float StationarySpeedModifier { get => Mathf.Clamp(stationarySpeedModifier, 0.0f, maxStationarySpeedModifier); }

    /// <summary>
    /// Holds reference to the y positions of players feet.
    /// </summary>
    private Dictionary<JointType, Queue<Vector3>> footTracking = new Dictionary<JointType, Queue<Vector3>>();

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
    #endregion

    //TODO
    #region Kinnect
    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager bodyManager;

    [SerializeField]
    private bool isKnee = false;
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

        InitializeComponents();
        InitializeDictionaries();
        InitializeAnimationIDs();
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
    }
    #endregion

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
                foreach (JointType joint in joints)
                {
                    Joint sourceJoint = body.Joints[joint];

                    var height = BodySourceManager.DistanceFrom(sourceJoint.Position);
                    
                    /*
                    if(height > minUpHeightFoot)
                    {
                        //print("Height: " + height);
                        CheckStateStationary(height);
                    }*/

                    StationaryMovement(sourceJoint, joint, body);
                }

                UpdateCharacterPositionStationary();
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
            print("Failed Wobble Test");
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

                        if (distUp > minUpHeightKnee)
                        {
                            if (footAcceptingInputs[joint])
                            {
                                StartCoroutine(DelayFootInputStationary(joint));
                                StartCoroutine(HoldStationaryModScore());
                            }

                            var newHeight = Mathf.Clamp(distUp, minUpHeightKnee, maxUpHeightKnee);
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
    private void UpdateStationaryModifier(float modifier)
    {
        stationarySpeedModifier += modifier;
        stationarySpeedModifier = Mathf.Clamp(stationarySpeedModifier, 0.0f, Mathf.Infinity);
    }

    private IEnumerator HoldStationaryModScore()
    {
        StartCoroutine(ModScoreEaseLoop(1, speedIncrementEaseInTime));

        yield return new WaitForSeconds(inputSpeedHoldTime);

        StartCoroutine(ModScoreEaseLoop(-1, speedIncrementEaseOutTime));
    }

    private IEnumerator ModScoreEaseLoop(int incrementDirection, float easeTime)
    {
        var t = easeTime;
        var totalChanged = 0.0f;

        while (t > 0.0f)
        {
            var toAdd = inputSpeedIncrement * incrementDirection * Time.fixedDeltaTime / easeTime;
            t -= Time.fixedDeltaTime;
            totalChanged += toAdd;

            UpdateStationaryModifier(toAdd);
            yield return new WaitForFixedUpdate();
        }

        print("total changed: " + totalChanged);
        print("Adjustment: " + (inputSpeedIncrement - totalChanged * incrementDirection));

        UpdateStationaryModifier((inputSpeedIncrement - totalChanged*incrementDirection)*incrementDirection);
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
                var inverseLerp = isKnee ? Mathf.InverseLerp(minUpHeightKnee, maxUpHeightKnee, heightReached) : Mathf.InverseLerp(minUpHeightFoot, maxUpHeightFoot, heightReached);
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

        //stationarySpeedModifier = 1;
        currentMovementSpeed = Mathf.Lerp(currentMovementSpeed, movementSpeedStationary * StationarySpeedModifier * modifier, Time.fixedDeltaTime*movementSmoothTime);

        //currentMovementSpeed += Time.fixedDeltaTime / movementSmoothTime * modifier * stationarySpeedModifier;
        //currentMovementSpeed = Mathf.Clamp(currentMovementSpeed, 0.0f, );

        // Sets animation active/inactive
        bool isWalking = currentMovementSpeed > movementAnimationThreshold && !hasFailedRedLight && gameController.lightState != LightState.OFF;
        playerAnimator.SetBool(walkID, isWalking);
        playerAnimator.SetBool(breatheID, playerAnimator.GetBool(breatheID) || isWalking);

        if (isWalking)
        {
            if(gameController.lightState == LightState.GREEN) gameController.UpdatePoints(0, StationarySpeedModifier);

            transform.position += transform.forward * Time.fixedDeltaTime * currentMovementSpeed;
        }
    }
    #endregion
    #endregion

    #region Fail Event
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

        hasFailedRedLight = false;
    }
    #endregion
    #endregion
}
