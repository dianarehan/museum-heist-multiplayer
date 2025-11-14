using UnityEngine;

public class GuardAnimatorController : MonoBehaviour
{
    Animator animator;
    CharacterController controller;

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // 1. Get player movement input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool isMoving = (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f);

        // 2. Walking
        animator.SetBool("IsWalking", isMoving);

        // 3. Jumping
        if (Input.GetButtonDown("Jump"))
        {
            animator.SetTrigger("Jump");
        }

        // 4. Running
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        animator.SetBool("IsRunning", isRunning);

        // 5. Picking up
        if (Input.GetKeyDown(KeyCode.E))
        {
            animator.SetTrigger("Pickup");
        }
    }
}
