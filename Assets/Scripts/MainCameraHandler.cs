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

    /// <summary>
    /// The instance of the camera handler in the scene.
    /// </summary>
    private static MainCameraHandler instance;

    [Tooltip("The follow target of the camera")]
    [SerializeField] private Transform cameraFollowTarget;
    
    private static Transform CameraFollowTarget;

    private const float spinSpeed = 20;

    private float startingCameraDistance = 0;
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

        var thirdPersonFollow = mainVirtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        startingCameraDistance = thirdPersonFollow.CameraDistance;

        if (mainVirtualCamera != null) cameraShake = mainVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        GameController.ResetGameEvent.AddListener(ResetCamera);
    }

    /// <summary>
    /// Resets any need values back to their defaults for the camera.
    /// </summary>
    private void ResetCamera()
    {
        StopAllCoroutines();
        cameraFollowTarget.transform.rotation = Quaternion.identity;

        var thirdPersonFollow = mainVirtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        thirdPersonFollow.CameraDistance = startingCameraDistance;
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

    /// <summary>
    /// Handles the events that animate the camera.
    /// </summary>
    /// <param name="animationTag">The tag to call for the correct animation.</param>
    public static void AnimateCamera(string animationTag)
    {
        if(animationTag == "Win")
        {
            instance.StartCoroutine(SpinOnWin());
        }
    }

    /// <summary>
    /// Spins the camera upon the game being won.
    /// </summary>
    /// <returns></returns>
    private static IEnumerator SpinOnWin()
    {
        var thirdPersonFollow = mainVirtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();

        while (true)
        {
            thirdPersonFollow.CameraDistance = thirdPersonFollow.CameraDistance + Time.deltaTime * 4;
            thirdPersonFollow.CameraDistance = Mathf.Clamp(thirdPersonFollow.CameraDistance, 0, 5);

            CameraFollowTarget.Rotate(new Vector3(0, spinSpeed * Time.deltaTime, 0));
            yield return new WaitForEndOfFrame();
        }
    }
    #endregion
}
