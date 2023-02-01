/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Red Light Green Light
 * Creation Date: 12/28/2022 2:23:46 PM
 * 
 * Description: Handles events for the main camera and it's camera shake
*********************************/
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraHandler : MonoBehaviour
{
    #region Fields
    /// <summary>
    /// The virtual camera that is mainly used.
    /// </summary>
    private static CinemachineVirtualCamera mainVirtualCamera;

    /// <summary>
    /// The camera shake for the cinemachine camera.
    /// </summary>
    private static CinemachineBasicMultiChannelPerlin cameraShake;

    private static MainCameraHandler instance;

    [Tooltip("The follow target of the camera")]
    [SerializeField] private Transform cameraFollowTarget;
    
    private static Transform CameraFollowTarget;

    private const float spinSpeed = 180;
    #endregion

    #region Functions
    /// <summary>
    /// Initializes Components.
    /// </summary>
    private void Awake()
    {
        instance = this;
        CameraFollowTarget = cameraFollowTarget;
        mainVirtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();

        if (mainVirtualCamera != null) cameraShake = mainVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    /// <summary>
    /// Applies camera shake for the main camera.
    /// </summary>
    /// <param name="amplitude">The min/max offset of the camera shake.</param>
    /// <param name="frequency">How rapid the camera shake is.</param>
    /// <param name="duration">How long the camera shake lasts.</param>
    /// <returns></returns>
    public static IEnumerator ApplyCameraShake(float amplitude, float frequency, float duration)
    {
        if (cameraShake == null) yield break;

        var t = duration;
        cameraShake.m_FrequencyGain = frequency;

        // Keeps camera shake for the duration
        while (t > 0)
        {
            // Slow declines amplitude
            var tempAmplitude = t > (duration/2) ? amplitude : Mathf.Lerp(0, amplitude, t / (duration/2));
            cameraShake.m_AmplitudeGain = tempAmplitude;

            yield return new WaitForEndOfFrame();

            t -= Time.deltaTime;
        }

        // Resets the camera shake
        cameraShake.m_AmplitudeGain = 0;
        cameraShake.m_FrequencyGain = 0;
    }

    public static void AnimateCamera(string animationTag)
    {
        if(animationTag == "Win")
        {
            instance.StartCoroutine(SpinOnWin());
        }
    }

    private static IEnumerator SpinOnWin()
    {
        while (true)
        {
            CameraFollowTarget.Rotate(new Vector3(0, spinSpeed * Time.deltaTime, 0));
            yield return new WaitForEndOfFrame();
        }
    }
    #endregion
}
