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
    [Range(0.0f, 0.5f)]
    [Tooltip("The amount of time lerping in and out of movement")]
    [SerializeField] private float movementSmoothTime = 0.2f;

    [Range(0.0f, 20.0f)]
    [Tooltip("The speed of the player when moving")]
    [SerializeField]
    private float movementSpeed = 7.0f;

    /// <summary>
    /// Reference to the player's current movement speed.
    /// </summary>
    private float currentMovementSpeed = 0.0f;
    #endregion

    #region Race Fields
    // Holds values specific to the race game mode
    private float startingRaceZ;
    private float startingSensorDistance;
    private float currentSensorDistance = Mathf.Infinity;

    [Header("Race")]
    [Range(0.0f, 10.0f)]
    [Tooltip("The amount of leeway alloted to players when having them return to the start")]
    [SerializeField] private float returnDistanceLeeway = 0.25f;

    [Range(0.0f, 10.0f)]
    [Tooltip("The amount of time to keep player animations during the race game mode")]
    [SerializeField] private float raceAnimationDuration = 0.5f;

    private float currentMovementDuration = 0.0f;

    [Range(0.0f, 10.0f)]
    [Tooltip("The distance from the sensor where the patient wins")]
    [SerializeField] private float winningDistance = 0.1f;
    #endregion

    #region Stationary
    [Header("Stationary")]
    [Range(0, 120)]
    [Tooltip("The max size of frames to hold for checking if players have moved feed")]
    [SerializeField] private int maxQueueSize = 25;

    [Range(0.0f, 10.0f)]
    [Tooltip("The minimum height a patients foot must be raised by")]
    [SerializeField] private float minUpHeight = 0.1f;

    [Range(0.0f, 10.0f)]
    [Tooltip("The maximum height a patients foot may be raised by (limits movementspeed")]
    [SerializeField] private float maxUpHeight = 0.4f;

    [Space(InspectorValues.SPACE_BETWEEN_EDITOR_ELEMENTS)]

    [Range(0.0f, 2.0f)]
    [Tooltip("The amount of time movement lasts for")]
    [SerializeField] private float minMovementTime = 0.5f;

    [Range(0.0f, 2.0f)]
    [Tooltip("The amount of time movement lasts for")]
    [SerializeField] private float maxMovementTime = 1.0f;

    /// <summary>
    /// Holds reference to the y positions of players feet.
    /// </summary>
    private Dictionary<JointType, Queue<float>> footTracking = new Dictionary<JointType, Queue<float>>();
    #endregion

    #region Animations
    /// <summary>
    /// The animator for the player model.
    /// </summary>
    private Animator playerAnimator;

    // Animator ID for when the player is moving
    private string walkAnimationTag = "IsWalking";
    private int walkID;

    // Animator ID for failing a red light in the stationary mode
    private string hitAnimationTag = "IsHit";
    private int hitID;

    // Animator ID for failing a red light in the race mode
    private string deathAnimationTag = "IsDead";
    private int deathID;
    #endregion

    #region Kinnect
    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager bodyManager;

    [SerializeField]
    private bool isKnee = false;
    private List<JointType> joints;
    private List<JointType> kneeJoints = new List<JointType>
    {
        JointType.KneeLeft,
        JointType.KneeRight
    };

    private List<JointType> footJoints = new List<JointType>
    {
        JointType.KneeLeft,
        JointType.KneeRight
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
            footTracking.Add(joint, new Queue<float>());
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

        foreach (var body in _data)
        {
            if (body == null) continue;

            if (body.IsTracked) _trackedIds.Add(body.TrackingId);
        }
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

            if (body.IsTracked)
            {
                if (!_Bodies.ContainsKey(body.TrackingId))
                {
                    var startingPosition = body.Joints[JointType.SpineBase].Position;
                    startingSensorDistance = Length(startingPosition);
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
    /// <param name="body"></param>
    private void RefreshBodyObject(Body body)
    {
        foreach (JointType joint in joints)
        {
            Joint sourceJoint = body.Joints[joint];

            switch (CurrentGameMode)
            {
                case GameMode.RACE:
                    RaceMovement(body);
                    break;
                case GameMode.STATIONARY:
                default:
                    RefreshStationary(sourceJoint, joint);
                    UpdatePlayerPositionStationary();
                    break;
            }
        }
    }
    #endregion

    #region Race Movement
    private void RaceMovement(Body body)
    {
        var currentPosition = body.Joints[JointType.SpineBase].Position;
        var distance = Length(currentPosition);

        if (hasFailedRedLight)
        {
            currentSensorDistance = distance;
        }
        else
        {
            CheckStateRace(distance);
        }
    }

    private void CheckStateRace(float distance)
    {
        switch (gameController.lightState)
        {
            case LightState.RED:
                if (!hasFailedRedLight && gameController.canDetectPenaltyMovement && distance+0.25f< currentSensorDistance) StartCoroutine(FailRedLight());
                break;

            case LightState.GREEN:
                if (distance < currentSensorDistance)
                {
                    currentSensorDistance = distance;
                    SetPlayerZ(currentSensorDistance);

                    playerAnimator.SetBool(walkID, true);
                    
                    if(currentMovementDuration == 0)
                    {
                        StartCoroutine(StopMovingAnimations());
                    }

                    currentMovementDuration = raceAnimationDuration;


                    if (winningDistance > currentSensorDistance)
                    {
                        StartCoroutine(gameController.EndGame());
                    }
                }
                break;

            case LightState.OFF:
            default:
                break;
        }
    }

    private void SetPlayerZ(float distance)
    {
        var pos = transform.position;
        pos.z = (startingSensorDistance - distance) * playerSize + startingRaceZ;

        transform.position = pos;
    }

    private IEnumerator StopMovingAnimations()
    {
        do
        {
            yield return new WaitForFixedUpdate();
            currentMovementDuration -= Time.fixedDeltaTime;
        }
        while (currentMovementDuration > 0);


        print("Stop");

        currentMovementDuration = 0;
        playerAnimator.SetBool(walkID, false);
    }

    private IEnumerator MakePlayerRestart()
    {
        SetPlayerZ(startingSensorDistance);

        while (currentSensorDistance < startingSensorDistance - returnDistanceLeeway)
        {
            yield return new WaitForFixedUpdate();
        }

        gameController.RestartGameRace();
    }

    public float Length(CameraSpacePoint point)
    {
        return Mathf.Sqrt(
            point.X * point.X +
            point.Y * point.Y +
            point.Z * point.Z
        );
    }
    #endregion

    #region Stationary Movement
    private void RefreshStationary(Joint sourceJoint, JointType joint)
    {
        var footTrackingQueue = footTracking[joint];

        if(footTrackingQueue.Count == maxQueueSize)
        {
            if (isKnee)
            {
                if (footTrackingQueue.Max() > sourceJoint.Position.Z) // .006
                {
                    var distUp = footTrackingQueue.Min() - sourceJoint.Position.Z;

                    if (distUp > minUpHeight)
                    {
                        print(distUp);
                        var newHeight = Mathf.Clamp(distUp, minUpHeight, maxUpHeight);
                        CheckStateStationary(newHeight);
                    }
                }
            }
            else
            {
                if (footTrackingQueue.Max() < sourceJoint.Position.Y)
                {
                    var distUp = sourceJoint.Position.Y - footTrackingQueue.Min();

                    if (distUp > minUpHeight)
                    {
                        print(distUp);
                        var newHeight = Mathf.Clamp(distUp, minUpHeight, maxUpHeight);
                        CheckStateStationary(newHeight);
                    }
                }

            }

            footTrackingQueue.Dequeue();
        }

        if (isKnee)
        {
            footTrackingQueue.Enqueue(sourceJoint.Position.Z);
        }
        else
        {
            footTrackingQueue.Enqueue(sourceJoint.Position.Y);
        }
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
                var time = Mathf.Lerp(minMovementTime, maxMovementTime, Mathf.InverseLerp(minUpHeight, maxUpHeight, heightReached));

                if (currentMovementDuration == 0)
                {
                    currentMovementDuration = time;
                    StartCoroutine(MovePlayerStationary());
                    //.Enqueue());
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

    private IEnumerator MovePlayerStationary()
    {
        while (currentMovementDuration > 0)
        {
            yield return new WaitForEndOfFrame();

            currentMovementDuration -= Time.deltaTime;
        }

        currentMovementDuration = 0;
    }

    private void UpdatePlayerPositionStationary()
    {
        var modifier = currentMovementDuration != 0 && gameController.lightState == LightState.GREEN ? 1 : -1;

        currentMovementSpeed += Time.fixedDeltaTime / movementSmoothTime * modifier;
        currentMovementSpeed = Mathf.Clamp(currentMovementSpeed, 0.0f, movementSpeed);

        bool isWalking = currentMovementSpeed != 0;
        playerAnimator.SetBool(walkID, isWalking);

        if (isWalking)
        {
            gameController.UpdatePoints();

            transform.position += transform.forward * Time.fixedDeltaTime * currentMovementSpeed;
        }
    }
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
        hasFailedRedLight = true;

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
