using UnityEngine;

namespace prefabs.SecurityGuard.scripts
{

    /// <summary>
    /// First Person Camera Controller with optional target following
    /// Handles camera rotation, head bobbing, and smooth target tracking
    /// </summary>
    public class FirstPersonCamera : MonoBehaviour
    {
        [Header("Camera References")]
        [Tooltip("The transform that represents the player's head/eye position")]
        [SerializeField]
        private Transform cameraHolder;

        [Tooltip("The actual camera component")] [SerializeField]
        private Camera fpCamera;

        [Header("Mouse Look Settings")] [SerializeField]
        private float mouseSensitivity = 2f;

        [SerializeField] private float maxLookAngle = 90f;

        [Header("Target Following (Optional)")] [Tooltip("Enable this to follow a target point")] [SerializeField]
        private bool followTarget = false;

        [Tooltip("The target point to look at")] [SerializeField]
        private Transform targetPoint;

        [Tooltip("How smoothly the camera follows the target (lower = smoother)")] [SerializeField]
        private float targetFollowSpeed = 5f;

        [Tooltip("Blend between manual control and target following (0-1)")] [SerializeField, Range(0f, 1f)]
        private float targetFollowWeight = 1f;

        [Header("Head Bob Settings")] [SerializeField]
        private bool enableHeadBob = true;

        [SerializeField] private float bobSpeed = 10f;
        [SerializeField] private float bobAmount = 0.05f;

        // Private variables
        private float rotationX = 0f;
        private float rotationY = 0f;
        private Vector3 originalCameraPosition;
        private float headBobTimer = 0f;

        private void Start()
        {
            InitializeCamera();
        }

        private void InitializeCamera()
        {
            // Lock and hide cursor for FPS experience
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Store original camera position for head bob
            if (cameraHolder != null)
            {
                originalCameraPosition = cameraHolder.localPosition;
            }

            // Validate references
            if (fpCamera == null)
            {
                fpCamera = GetComponentInChildren<Camera>();
                if (fpCamera == null)
                {
                    Debug.LogError("FirstPersonCamera: No camera found! Please assign a camera.");
                }
            }

            if (cameraHolder == null)
            {
                Debug.LogWarning("FirstPersonCamera: No camera holder assigned. Using camera transform.");
                cameraHolder = fpCamera.transform;
            }
        }

        private void Update()
        {
            HandleCameraRotation();
            HandleHeadBob();

            // Toggle cursor lock with Escape
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleCursorLock();
            }
        }

        private void HandleCameraRotation()
        {
            if (followTarget && targetPoint != null)
            {
                // Blend between manual control and target following
                HandleTargetFollowing();
            }
            else
            {
                // Standard mouse look
                HandleMouseLook();
            }
        }

        private void HandleMouseLook()
        {
            // Get mouse input
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // Rotate camera up/down (pitch)
            rotationX -= mouseY;
            rotationX = Mathf.Clamp(rotationX, -maxLookAngle, maxLookAngle);

            // Rotate player left/right (yaw)
            rotationY += mouseX;

            // Apply rotations
            if (cameraHolder != null)
            {
                cameraHolder.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
            }

            transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
        }

        private void HandleTargetFollowing()
        {
            // Calculate direction to target
            Vector3 directionToTarget = (targetPoint.position - cameraHolder.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

            // Extract pitch and yaw from target rotation
            Vector3 targetEuler = targetRotation.eulerAngles;
            float targetPitch = targetEuler.x > 180 ? targetEuler.x - 360 : targetEuler.x;
            float targetYaw = targetEuler.y;

            if (targetFollowWeight >= 0.99f)
            {
                // Full target following
                rotationX = -targetPitch;
                rotationY = targetYaw;
            }
            else if (targetFollowWeight > 0f)
            {
                // Blend mouse look with target following
                float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * (1f - targetFollowWeight);
                float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * (1f - targetFollowWeight);

                // Smoothly interpolate towards target
                float targetRotX = -targetPitch;
                float targetRotY = targetYaw;

                rotationX = Mathf.LerpAngle(rotationX - mouseY, targetRotX,
                    targetFollowWeight * Time.deltaTime * targetFollowSpeed);
                rotationY = Mathf.LerpAngle(rotationY + mouseX, targetRotY,
                    targetFollowWeight * Time.deltaTime * targetFollowSpeed);

                rotationX = Mathf.Clamp(rotationX, -maxLookAngle, maxLookAngle);
            }
            else
            {
                // No target following, just mouse look
                HandleMouseLook();
                return;
            }

            // Apply rotations smoothly
            Quaternion smoothPitch = Quaternion.Slerp(
                cameraHolder.localRotation,
                Quaternion.Euler(rotationX, 0f, 0f),
                Time.deltaTime * targetFollowSpeed
            );

            Quaternion smoothYaw = Quaternion.Slerp(
                transform.rotation,
                Quaternion.Euler(0f, rotationY, 0f),
                Time.deltaTime * targetFollowSpeed
            );

            cameraHolder.localRotation = smoothPitch;
            transform.rotation = smoothYaw;
        }

        private void HandleHeadBob()
        {
            if (!enableHeadBob || cameraHolder == null) return;

            // Only bob when moving (check if this should be triggered by movement script)
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            bool isMoving = Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f;

            if (isMoving)
            {
                // Increment timer
                headBobTimer += Time.deltaTime * bobSpeed;

                // Calculate bob offset
                float bobOffsetY = Mathf.Sin(headBobTimer) * bobAmount;
                float bobOffsetX = Mathf.Cos(headBobTimer * 0.5f) * bobAmount * 0.5f;

                // Apply bob
                cameraHolder.localPosition = originalCameraPosition + new Vector3(bobOffsetX, bobOffsetY, 0f);
            }
            else
            {
                // Reset to original position smoothly
                headBobTimer = 0f;
                cameraHolder.localPosition = Vector3.Lerp(
                    cameraHolder.localPosition,
                    originalCameraPosition,
                    Time.deltaTime * bobSpeed
                );
            }
        }

        private void ToggleCursorLock()
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        #region Public Methods

        /// <summary>
        /// Set the target point to follow
        /// </summary>
        public void SetTargetPoint(Transform target)
        {
            targetPoint = target;
        }

        /// <summary>
        /// Enable or disable target following
        /// </summary>
        public void SetFollowTarget(bool follow)
        {
            followTarget = follow;
        }

        /// <summary>
        /// Set the weight of target following (0 = no following, 1 = full following)
        /// </summary>
        public void SetTargetFollowWeight(float weight)
        {
            targetFollowWeight = Mathf.Clamp01(weight);
        }

        /// <summary>
        /// Set mouse sensitivity
        /// </summary>
        public void SetMouseSensitivity(float sensitivity)
        {
            mouseSensitivity = sensitivity;
        }

        /// <summary>
        /// Enable or disable head bob
        /// </summary>
        public void SetHeadBob(bool enable)
        {
            enableHeadBob = enable;
        }

        #endregion
    }
}