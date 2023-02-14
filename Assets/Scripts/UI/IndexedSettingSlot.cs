/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Apple Basket
 * Creation Date: 2/10/2023 2:50:45 PM
 * 
 * Description: TODO
*********************************/
using UnityEngine;

public class IndexedSettingSlot : SettingsSlot
{
    #region Fields
    [SerializeField] private GameObject[] settingsObjects;
    #endregion

    #region Functions
    public void SetCurrentSlotIndex(int index)
    {
        foreach(GameObject obj in settingsObjects)
        {
            obj.SetActive(false);
        }

        settingsObjects[index].SetActive(true);
    }

    public int GetSlotAmount()
    {
        return settingsObjects.Length;
    }
    #endregion
}
