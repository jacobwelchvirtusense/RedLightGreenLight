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
            var copy = Instantiate(gameObject, transform.position + new Vector3(0, 0, 229.8f), Quaternion.identity);
            var animationNumber = Random.Range(1, 7);

            animator = GetComponent<Animator>();
            animator.SetInteger("AnimationNumber", animationNumber);

            copy.GetComponent<DuplicateNpc>().InitializeCopy(animationNumber, transform);
        }
        else
        {
            isCopy = true;
            isMovingNpc = TryGetComponent(out MovingNPC moving);
        }
    }

    public void InitializeCopy(int animationNumber, Transform originalReference)
    {
        animator = GetComponent<Animator>();
        animator.SetInteger("AnimationNumber", animationNumber);
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
