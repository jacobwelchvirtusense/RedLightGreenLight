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
using UnityEditor.Localization.Plugins.XLIFF.V12;

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

    private float startingDistance;
    private float startingZ;
    private float closestZ = Mathf.Infinity;

    [Range(0.0f, 10.0f)]
    [Tooltip("The distance from the sensor where the patient wins")]
    [SerializeField] private float winningDistance = 0.1f;

    #region Movement
    /// <summary>
    /// All of the current movements being handled.
    /// </summary>
    private Queue<Coroutine> movementRoutines = new Queue<Coroutine>();

    #region Input Thresholds
    [Header("Inputs")]
    [Range(0.0f, 10.0f)]
    [Tooltip("The minimum height a patients foot must be raised by")]
    [SerializeField] private float minUpHeight = 0.1f;

    [Range(0.0f, 10.0f)]
    [Tooltip("The maximum height a patients foot may be raised by (limits movementspeed")]
    [SerializeField] private float maxUpHeight = 0.4f;

    [Range(0.0f, 10.0f)]
    [Tooltip("The minimum height that a patients foot must be brought down to (registers movement on reaching it)")]
    [SerializeField] private float minDownHeight = 0.05f;
    #endregion

    #region Speed
    [Header("Speed")]
    [Range(0.0f, 1.0f)]
    [Tooltip("The amount of time movement lasts for")]
    [SerializeField] private float movementTime = 0.5f;

    [Range(0.0f, 2.0f)]
    [Tooltip("The amount of time movement lasts for")]
    [SerializeField] private float minMovementTime = 0.5f;

    [Range(0.0f, 2.0f)]
    [Tooltip("The amount of time movement lasts for")]
    [SerializeField] private float maxMovementTime = 1.0f;

    [Range(0.0f, 0.5f)]
    [Tooltip("The amount of time lerping in and out of movement")]
    [SerializeField] private float movementSmoothTime = 0.2f;

    [Range(0.0f, 20.0f)]
    [Tooltip("The speed of the player when moving")]
    [SerializeField]
    private float movementSpeed = 7.0f;

    private float currentMovementSpeed = 0.0f;

    /*
    [Range(0.0f, 20.0f)]
    [Tooltip("The minimum speed used")]
    [SerializeField] private float minMovementSpeed = 5.0f;

    [Range(0.0f, 20.0f)]
    [Tooltip("The maximum speed used")]
    [SerializeField] private float maxMovementSpeed = 5.0f;*/
    #endregion
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
    public Material BoneMaterial;

    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager bodyManager;

    private Dictionary<JointType, JointType> _BoneMap = new Dictionary<JointType, JointType>()
    {
        { JointType.FootLeft, JointType.AnkleLeft },
        { JointType.AnkleLeft, JointType.KneeLeft },
        { JointType.KneeLeft, JointType.HipLeft },
        { JointType.HipLeft, JointType.SpineBase },

        { JointType.FootRight, JointType.AnkleRight },
        { JointType.AnkleRight, JointType.KneeRight },
        { JointType.KneeRight, JointType.HipRight },
        { JointType.HipRight, JointType.SpineBase },

        { JointType.HandTipLeft, JointType.HandLeft },
        { JointType.ThumbLeft, JointType.HandLeft },
        { JointType.HandLeft, JointType.WristLeft },
        { JointType.ElbowLeft, JointType.ShoulderLeft },
        { JointType.ShoulderLeft, JointType.SpineShoulder },
        { JointType.WristLeft, JointType.ElbowLeft },

        { JointType.HandTipRight, JointType.HandRight },
        { JointType.ThumbRight, JointType.HandRight },
        { JointType.HandRight, JointType.WristRight },
        { JointType.WristRight, JointType.ElbowRight },
        { JointType.ElbowRight, JointType.ShoulderRight },
        { JointType.ShoulderRight, JointType.SpineShoulder },

        { JointType.SpineBase, JointType.SpineMid },
        { JointType.SpineMid, JointType.SpineShoulder },
        { JointType.SpineShoulder, JointType.Neck },
        { JointType.Neck, JointType.Head },
    };

    private List<JointType> joints = new List<JointType>
    {
        JointType.AnkleLeft,
        JointType.AnkleRight
    };

    private bool footLowLocked = false;
    private Dictionary<JointType, float> footLow = new Dictionary<JointType, float>();
    private Dictionary<JointType, float> footHeight = new Dictionary<JointType, float>();
    #endregion
    #endregion

    #region Functions
    #region Initialization
    /// <summary>
    /// Performs all actions in the awake event.
    /// </summary>
    private void Awake()
    {
        startingZ = transform.position.z;

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

    private void InitializeDictionaries()
    {
        foreach(JointType joint in joints)
        {
            footHeight.Add(joint, 0.0f);
            footLow.Add(joint, Mathf.Infinity);
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
                    startingDistance = Length(startingPosition);
                    _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                }

                RefreshBodyObject(body, _Bodies[body.TrackingId]);
            }
        }
        #endregion

        if (CurrentGameMode == GameMode.STATIONARY) UpdatePlayerPositionStationary();
    }

    /// <summary>
    /// Creates the body in the scene.
    /// </summary>
    /// <param name="id">The id of the body to be created.</param>
    /// <returns></returns>
    private GameObject CreateBodyObject(ulong id)
    {
        GameObject body = new GameObject("Body:" + id);

        for (JointType jt = JointType.SpineBase; jt <= JointType.ThumbRight; jt++)
        {
            GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);

            LineRenderer lr = jointObj.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.material = BoneMaterial;
            lr.startWidth = 0.05f;
            lr.endWidth = 0.05f;

            jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            jointObj.name = jt.ToString();
            jointObj.transform.parent = body.transform;
        }

        return body;
    }

    private void RefreshBodyObject(Body body, GameObject bodyObject)
    {
        foreach (JointType joint in joints)
        {
            Joint sourceJoint = body.Joints[joint];
            Vector3 targetPosition = GetVector3FromJoint(sourceJoint);

            Transform jointObject = bodyObject.transform.Find(joint.ToString());
            jointObject.position = targetPosition;

            switch (CurrentGameMode)
            {
                case GameMode.RACE:
                    RefreshRace(body);
                    break;
                case GameMode.STATIONARY:
                default:
                    RefreshStationary(sourceJoint, joint);
                    break;
            }
        }
    }

    private Vector3 GetVector3FromJoint(Joint joint)
    {
        return new Vector3(joint.Position.X * playerSize, joint.Position.Y * playerSize, joint.Position.Z * playerSize) + transform.position;
    }
    #endregion

    #region Race Movement
    private void RefreshRace(Body body)
    {
        if (!hasFailedRedLight)
        {
            var currentPosition = body.Joints[JointType.SpineBase].Position;
            var distance = Length(currentPosition);

            SetPlayerZ(distance);
            CheckStateRace(distance);
        }
        else
        {

        }
    }

    private void CheckStateRace(float distance)
    {
        switch (gameController.lightState)
        {
            case LightState.RED:
                if (!hasFailedRedLight && gameController.canDetectPenaltyMovement) StartCoroutine(FailRedLight());
                break;

            case LightState.GREEN:
                if(distance < closestZ) closestZ = distance;
                break;

            case LightState.OFF:
            default:
                break;
        }
    }

    private void SetPlayerZ(float distance)
    {
        var pos = transform.position;
        pos.z = (startingDistance - distance) * playerSize + startingZ;

        transform.position = pos;
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
        if (!footLowLocked)
        {
            if (sourceJoint.Position.Y < footLow[joint])
            {
                footLow[joint] = sourceJoint.Position.Y;
            }
        }

        var distFromGround = sourceJoint.Position.Y - footLow[joint];

        if(joint == JointType.AnkleLeft)
        {
            print("Dist: " + distFromGround);
            print("Foot Low: " + footLow[joint]);
        }


        /*
        //CameraSpacePoint point = body.Joints[joint].Position;
        //var distFromGround = Floor.DistanceFrom(point);
        if (joint == JointType.AnkleLeft)
        {
            print("Floor: " + Floor.Height);
            print("Joint: " + jointObject.position.y);
            print("Target: " + targetPosition);
            print("Dist: " + distFromGround);
        }*/

        if (distFromGround > minUpHeight)
        {
            footLowLocked = true;
            var newHeight = Mathf.Clamp((float)distFromGround, minUpHeight, maxUpHeight);
            footHeight[joint] = newHeight > footHeight[joint] ? newHeight : footHeight[joint];
            CheckStateStationary(footHeight[joint]);
        }
        else if (distFromGround < minDownHeight && footHeight[joint] != 0)
        {
            //CheckStateStationary(footHeight[joint]);
            footHeight[joint] = 0.0f;
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
                movementRoutines.Enqueue(StartCoroutine(MovePlayerStationary(heightReached)));
                break;

            case LightState.OFF:
            default:
                break;
        }
    }

    private IEnumerator MovePlayerStationary(float height)
    {
        float t = movementTime;
        //var speed = Mathf.Lerp(minMovementSpeed, maxMovementSpeed, Mathf.InverseLerp(minUpHeight, maxUpHeight, height));
        var time = Mathf.Lerp(minMovementTime, maxMovementTime, Mathf.InverseLerp(minUpHeight, maxUpHeight, height));

        //print("Move Length Time: " + time);

        yield return new WaitForSeconds(time);

        /*
        while (t > 0)
        {
            yield return new WaitForEndOfFrame();
            var tempT = 1.0f;
            if (t > movementTime - movementSmoothTime)
            {
                tempT = Mathf.InverseLerp(movementTime, movementTime - movementSmoothTime, t);
            }
            else if(t < movementSmoothTime)
            {
                tempT = Mathf.InverseLerp(0.0f, 0.1f, t);
            }

            var tempSpeed = Mathf.Lerp(0, speed, tempT);

            gameController.UpdatePoints();
            transform.position += transform.forward * Time.deltaTime * tempSpeed;
            t -= Time.deltaTime;
        }*/

        movementRoutines.Dequeue();
    }

    private void UpdatePlayerPositionStationary()
    {
        var modifier = movementRoutines.Count != 0 && gameController.lightState == LightState.GREEN ? 1 : -1;

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

    /// <summary>
    /// Handles the event of failing a red light.
    /// </summary>
    /// <returns></returns>
    private IEnumerator FailRedLight()
    {
        hasFailedRedLight = true;

        var animID = CurrentGameMode == GameMode.RACE ? deathID : hitID;
        playerAnimator.SetTrigger(animID);

        yield return gameController.FailedRedRoutine();

        if(CurrentGameMode == GameMode.RACE)
        {
            SetPlayerZ(startingZ);

            while (transform.position.z > startingZ-0.25f)
            {
                yield return new WaitForFixedUpdate();
            }

            gameController.RestartGameRace();
        }

        hasFailedRedLight = false;
    }
    #endregion
}
