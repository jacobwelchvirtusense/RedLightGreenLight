/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Red Light Green Light
 * Creation Date: 1/20/2023 3:09:47 PM
 * 
 * Description: TODO
*********************************/
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AvatarSelecter : MonoBehaviour
{
    #region Fields
    [Tooltip("The different character objects that can be used")]
    [SerializeField] private GameObject[] characterParents = new GameObject[0];

    [Tooltip("The different materials that can be used for characters")]
    [SerializeField] private Material[] materials = new Material[0];
    #endregion

    #region Functions
    private void Awake()
    {
        if(!gameObject.name.Contains("Clone"))
        InitializeAvatar();
    }

    public void InitializeAvatar()
    {
        var charParent = characterParents[Random.Range(0, characterParents.Length)];
        charParent.SetActive(true);

        charParent.GetComponent<SkinnedMeshRenderer>().material = materials[Random.Range(0, materials.Length)];
    }
    #endregion
}
