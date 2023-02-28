/*********************************
 * Created by: Jacob Welch
 * Email: jacobw@virtusense.com
 * Company: Virtusense
 * Project: Red Light Green Light
 * Creation Date: 2/8/2023 11:38:57 AM
 * 
 * Description: Handles the duplicating of NPCs and the
 * syncing of them.
*********************************/
using System.Collections;
using UnityEngine;

public class DuplicateNpc : MonoBehaviour
{
    #region Fields
    /// <summary>
    /// The animator for the idle npc.
    /// </summary>
    private Animator animator;

    /// <summary>
    /// Holds true if this npc is a copy of another npc.
    /// </summary>
    private bool isCopy = false;

    /// <summary>
    /// Holds true if this npc is a moving npc.
    /// </summary>
    private bool isMovingNpc = false;

    /// <summary>
    /// Reference to the original transform if this is a copy.
    /// </summary>
    private Transform originalReference;
    #endregion

    #region Functions
    // Start is called before the first frame update
    private void Start()
    {
        if (!gameObject.name.Contains("Clone"))
        {
            //GameController.ResetGameEvent.AddListener(InitializeAvatar);

            var copy = Instantiate(gameObject, transform.position + new Vector3(0, 0, 229.8f), Quaternion.identity);
            var animationDelay = Random.Range(0.0f, 2.0f);
            var animationNumber = Random.Range(1, 12);

            StartCoroutine(InitializeAnimation(animationDelay, animationNumber));

            copy.GetComponent<DuplicateNpc>().InitializeCopy(animationNumber, animationDelay, transform);
        }
        else
        {
            isCopy = true;
            isMovingNpc = TryGetComponent(out MovingNPC moving);
        }
    }

    private IEnumerator InitializeAnimation(float delay, int animationNumber)
    {
        yield return new WaitForSeconds(delay);
        animator = GetComponent<Animator>();
        animator.SetInteger("AnimationNumber", animationNumber);
    }

    public void InitializeCopy(int animationNumber, float animationDelay, Transform originalReference)
    {
        StartCoroutine(InitializeAnimation(animationDelay, animationNumber));
        this.originalReference = originalReference;
    }

    private void FixedUpdate()
    {
        if(isMovingNpc && isCopy)
        {
            transform.position = originalReference.position + new Vector3(0, 0, 229.8f);
        }
    }
    #endregion
}
