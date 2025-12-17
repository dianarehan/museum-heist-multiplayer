using GLTFast.Schema;
using Photon.Pun;
using UnityEngine;

public class PlayerController : MonoBehaviourPunCallbacks
{
    // --- Variables from Script 1 (safer [SerializeField]) ---
    [SerializeField] private float speed = 5f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float crouchSpeed = 2f;
    
    // --- Variables with clearer names from Script 1 ---
    [SerializeField] private Transform firstPersonFollow;
    [SerializeField] private Transform thirdPersonFollow;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioListener audioListener;

    private Camera cameraObj;

    private Animator anim;
    private Rigidbody rb;
    private Transform tr;
    //private CharacterController controller;

    public GameObject head;

    void Start()
    {

        // If this is NOT our local player turn off its camera & input logic
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            if (playerCamera != null) playerCamera.enabled = false;
            if (audioListener != null) audioListener.enabled = false;
            // No need for Update input on remote players
            return;
        }


        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true) return;
        
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        tr = GetComponent<Transform>();
        SkinnedMeshRenderer headRender = head.GetComponent<SkinnedMeshRenderer>();
        headRender.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        //controller = GetComponent<CharacterController>();

        // --- Fallback from Script 1 ---
        if (playerCamera != null)
        {
            cameraObj = playerCamera;
        }
        else
        {
            cameraObj = Camera.main;
        }

        // --- Scale-relative logic from Script 2 ---
        if (thirdPersonFollow != null)
        {
            thirdPersonFollow.position = new Vector3(
                thirdPersonFollow.position.x,
                6f * (tr.localScale.y / 1),
                thirdPersonFollow.position.z
            );
        }
        if (firstPersonFollow != null)
        {
            firstPersonFollow.position = new Vector3(
                firstPersonFollow.position.x,
                //firstPersonFollow.position.y,
                10f * (tr.localScale.y / 1),
                firstPersonFollow.position.z
            );
        }
    }

    void Update()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected) return;

        

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(h, 0f, v).normalized;

        // Get camera forward/right directions
        Vector3 camForward = cameraObj.transform.forward;
        Vector3 camRight = cameraObj.transform.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // Move relative to camera direction
        Vector3 moveDir = (camForward * v + camRight * h).normalized;

        if (move.magnitude >= 0.1f)
        {
            anim.SetBool("walking", true);
            tr.rotation = Quaternion.LookRotation(moveDir, Vector3.up);
            float move_speed = anim.GetBool("running") ? runSpeed : anim.GetBool("crouching") ? crouchSpeed : speed;
            //controller.Move(moveDir * move_speed * Time.deltaTime);
            rb.MovePosition(rb.position + moveDir * move_speed * Time.deltaTime);

            // --- CRITICAL MERGE from Script 2: Move camera points with player ---
            /*
            if (firstPersonFollow != null)
            {
                firstPersonFollow.position = transform.position + new Vector3(0, 10.0f, 0);
                firstPersonFollow.rotation = cameraObj.transform.rotation;

            }
            
            if (thirdPersonFollow != null)
            {
                thirdPersonFollow.Translate(moveDir * move_speed * Time.deltaTime);
            }*/

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

        // --- MERGED from Script 2: Use scale-relative logic for height ---
        if (anim.GetBool("crouching"))
        {
            if (thirdPersonFollow != null)
            {
                thirdPersonFollow.position = new Vector3(thirdPersonFollow.position.x, 4f * (tr.localScale.y / 1), thirdPersonFollow.position.z);
            }
            if (firstPersonFollow != null)
            {
                firstPersonFollow.position = new Vector3(firstPersonFollow.position.x, 6f * (tr.localScale.y / 1), firstPersonFollow.position.z);
            }
        }
        else
        {
            if (thirdPersonFollow != null)
            {
                thirdPersonFollow.position = new Vector3(thirdPersonFollow.position.x, 6f * (tr.localScale.y / 1), thirdPersonFollow.position.z);
            }
            if (firstPersonFollow != null)
            {
                firstPersonFollow.position = new Vector3(firstPersonFollow.position.x, 8f * (tr.localScale.y / 1), firstPersonFollow.position.z);
            }
        }

        // Pick Up: for now!, Raycast to be added
        if (Input.GetKeyDown(KeyCode.E))
        {
            anim.SetTrigger("pickUp");
        }
    }
}