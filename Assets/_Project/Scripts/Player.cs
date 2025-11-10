using UnityEngine;
using UnityEngine.InputSystem.XR;

public class player_script : MonoBehaviour
{

    public float speed;
    public float runSpeed;
    public float crouchSpeed;
    private Animator anim;
    private Rigidbody rb;
    private Transform tr;
    private CharacterController controller;

    // first and 2nd person follow
    public Transform FPF;
    public Transform TPF;

    public Camera cameraObj;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        tr = GetComponent<Transform>();
        controller = GetComponent<CharacterController>();

        //float TPF_y = TPF.position.y;
        //float FPF_y = FPF.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(h, 0f, v).normalized;


        // get camera forward/right directions
        Vector3 camForward = cameraObj.transform.forward;
        Vector3 camRight = cameraObj.transform.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // move relative to camera direction
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
            TPF.position = new Vector3(TPF.position.x, 2f, TPF.position.z);
            FPF.position = new Vector3(FPF.position.x, 4f, FPF.position.z);
        }
        else
        {
            TPF.position = new Vector3(TPF.position.x, 4f, TPF.position.z);
            FPF.position = new Vector3(FPF.position.x, 6f, FPF.position.z);
        }

    }
}
