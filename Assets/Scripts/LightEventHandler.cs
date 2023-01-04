/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Red Light Green Light
 * Creation Date: 12/29/2022 3:13:43 PM
 * 
 * Description: Event listener for light change events.
*********************************/
using TMPro;
using UnityEngine;
using static GameController;

public class LightEventHandler : MonoBehaviour
{
    #region Fields
    [Tooltip("When this light should be enabled or disabled")]
    [SerializeField] private LightState LightState = LightState.GREEN;
    #endregion

    #region Functions
    /// <summary>
    /// Adds the correct event listener to this object.
    /// </summary>
    private void Start()
    {
        if (gameController == null) return;
        gameController.lightChangeEvent.AddListener(SetActive);

        if(TryGetComponent(out TextMeshProUGUI text)) text.color = Color.white;

        if(TryGetComponent(out Light light)) light.enabled = true;

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Sets this object active or inactive from the light event calls
    /// </summary>
    /// <param name="lightState">The new light state for the game.</param>
    private void SetActive(LightState lightState)
    {
        gameObject.SetActive(lightState == LightState);
    }
    #endregion
}
