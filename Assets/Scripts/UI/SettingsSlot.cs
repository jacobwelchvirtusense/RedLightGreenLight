/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Apple Basket
 * Creation Date: 2/10/2023 2:13:12 PM
 * 
 * Description: TODO
*********************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SettingsSlot : MonoBehaviour
{
    #region Fields
    [Tooltip("The sprite to show when hovering over this slot")]
    [SerializeField] private Sprite hoverSprite;

    /// <summary>
    /// The default sprite of this settings slot.
    /// </summary>
    private Sprite startingSprite;

    /// <summary>
    /// The image to apply background sprites to.
    /// </summary>
    private Image slotImage;

    /// <summary>
    /// An event that is called when even this slot is clicked.
    /// </summary>
    public UnityEvent ClickEvent;
    #endregion

    #region Functions
    /// <summary>
    /// Initializes components and fields.
    /// </summary>
    private void Awake()
    {
        slotImage = GetComponent<Image>();
        startingSprite = slotImage.sprite;
    }

    /// <summary>
    /// Sets the hover state of this settings slot.
    /// </summary>
    /// <param name="shouldSet"></param>
    public void SetHover(bool shouldSet)
    {
        var newImage = shouldSet ? hoverSprite : startingSprite;
        slotImage.sprite = newImage;
    }
    #endregion
}
