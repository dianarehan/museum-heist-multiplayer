using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{
    [SerializeField] private float speed=5f;
    [SerializeField] private float runSpeed=6f;
    [SerializeField] private float crouchSpeed=2f;
    // first and 2nd person follow
    [SerializeField] private Transform firstPersonFollow;
    [SerializeField] private Transform thirdPersonFollow;
    [SerializeField] private Camera cameraObj;

    private Animator anim;
    private Rigidbody rb;
    private Transform tr;
    private CharacterController controller;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        tr = GetComponent<Transform>();
        controller = GetComponent<CharacterController>();
        if(cameraObj == null)
        {
            cameraObj = Camera.main;
        }
    }

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(h, 0f, v).normalized;


        Vector3 camForward = cameraObj.transform.forward;
        Vector3 camRight = cameraObj.transform.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = (camForward * v + camRight * h).normalized;

        if (move.magnitude >= 0.1f)
        {
            anim.SetBool("walking", true);
            tr.rotation = Quaternion.LookRotation(moveDir, Vector3.up);
            float move_speed = anim.GetBool("running") ? runSpeed : anim.GetBool("crouching") ?  crouchSpeed : speed;
            controller.Move(moveDir * move_speed * Time.deltaTime);
        }
        else
        {
            anim.SetBool("walking", false);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && anim.GetBool("walking"))
        {
            anim.SetBool("running", !anim.GetBool("running"));
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            anim.SetBool("crouching", !anim.GetBool("crouching"));
        }

        if (anim.GetBool("crouching"))
        {
            thirdPersonFollow.position = new Vector3(thirdPersonFollow.position.x, 2f, thirdPersonFollow.position.z);
            firstPersonFollow.position = new Vector3(firstPersonFollow.position.x, 4f, firstPersonFollow.position.z);
        }
        else
        {
            thirdPersonFollow.position = new Vector3(thirdPersonFollow.position.x, 4f, thirdPersonFollow.position.z);
            firstPersonFollow.position = new Vector3(firstPersonFollow.position.x, 6f, firstPersonFollow.position.z);
        }
    }
}