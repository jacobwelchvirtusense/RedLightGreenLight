/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Red Light Green Light
 * Creation Date: 3/6/2023 8:56:59 AM
 * 
 * Description: TODO
*********************************/
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Windows.Kinect;

public class SensorFeedback : MonoBehaviour
{
    #region Fields
    private TextMeshProUGUI textMeshProUGUI;

    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager bodyManager;

    [Header("Distance From Sensor")]
    [Range(0.0f, 5.0f)]
    [Tooltip("The minimum z dist the user should be from the sensor")]
    [SerializeField] private float minZDist = 0.4f;

    [Tooltip("The message displayed when the user is too close to the sensor")]
    [SerializeField] private string zDistCloseMessage = "You are too close to the sensor!!!";

    [Space(InspectorValues.SPACE_BETWEEN_EDITOR_ELEMENTS)]

    [Range(0.0f, 5.0f)]
    [Tooltip("The maximum z dist the user should be from the sensor")]
    [SerializeField] private float maxZDist = 1.0f;

    [Tooltip("The message displayed when the user is too far from the sensor")]
    [SerializeField] private string zDistFarMessage = "You are too far from the sensor!!!";

    [Header("Distance From Center")]
    [Range(0.0f, 5.0f)]
    [Tooltip("The maximum x dist the user should be from the center of the sensor")]
    [SerializeField] private float maxXDist = 0.4f;

    [Tooltip("The message displayed when the user is to the right of the center")]
    [SerializeField] private string moveLeftMessage = "Please move left to the center of the sensor";

    [Tooltip("The message displayed when the user is to the left of the center")]
    [SerializeField] private string moveRightMessage = "Please move right to the center of the sensor";

    [Header("Lack of Data Messages")]
    [Tooltip("The message displayed when the kinnect is not detected")]
    [SerializeField] private string noSensorFound = "No sensor detected!!!";

    [Tooltip("The message displayed when the kinnect is not detecting a user")]
    [SerializeField] private string noUserFound = "No user detected!!!";

    private bool canShowMessage = true;
    #endregion

    #region Functions
    /// <summary>
    /// Initializes components
    /// </summary>
    private void Awake()
    {
        textMeshProUGUI = GetComponent<TextMeshProUGUI>();
        textMeshProUGUI.text = "";

        bodyManager = FindObjectOfType<BodySourceManager>();

        GameController.ResetGameEvent.AddListener(ResetFeeback);
    }

    private void ResetFeeback()
    {
        canShowMessage = true;
    }

    public void DisableFeedback()
    {
        canShowMessage = false;
    }

    /// <summary>
    /// Updates game from Kinnect data.
    /// </summary>
    private void FixedUpdate()
    {
        #region Get Kinect Data
        if (bodyManager == null) return;

        Body[] _data = bodyManager.GetData();

        if (_data == null)
        {
            SetText(noSensorFound);
            return;
        }

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
        if (_Bodies.Count == 0) SetText(noUserFound);

        foreach (var body in _data)
        {
            if (body == null) continue;

            if (body.IsTracked && body.TrackingId == centerID)
            {
                if (!_Bodies.ContainsKey(body.TrackingId))
                {
                    DispalyFeedback(body.Joints[JointType.SpineBase].Position);
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// Displays feedback to the user based on there position relative to the sensor.
    /// </summary>
    /// <param name="position">The position in the camera space of the sensor.</param>
    private void DispalyFeedback(CameraSpacePoint position)
    {
        if (position.Z < minZDist)
        {
            SetText(zDistCloseMessage);
        }
        else if (position.Z > maxZDist)
        {
            SetText(zDistFarMessage);
        }
        else if (position.X < -maxXDist)
        {
            SetText(moveRightMessage);
        }
        else if (position.X > maxXDist)
        {
            SetText(moveLeftMessage);
        }
        else
        {
            SetText("");
        }
    }

    private void SetText(string newText)
    {
        if (canShowMessage)
        {
            textMeshProUGUI.text = newText;
        }
        else
        {
            textMeshProUGUI.text = "";
        }
    }
    #endregion
}
