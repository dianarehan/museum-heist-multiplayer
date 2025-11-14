using UnityEngine;

namespace prefabs.SecurityGuard.scripts
{

   /// <summary>
   /// Makes the character's upper body or spine rotate to follow where the camera is looking
   /// This makes the character aim weapons in the direction the player is looking
   /// Attach this to the Player root object
   /// </summary>
   public class UpperBodyIK : MonoBehaviour
   {
       [Header("References")]
       [Tooltip(
           "The transform that the upper body should look at (usually CameraHolder or a point in front of camera)")]
       [SerializeField]
       private Transform aimTarget;

       [Tooltip("The character's animator component")] [SerializeField]
       private Animator animator;

       [Header("Body Rotation Settings")]
       [Tooltip("Which bone to rotate (Spine, Chest, or UpperChest recommended) - Only used for Humanoid rigs")]
       [SerializeField]
       private HumanBodyBones targetBone = HumanBodyBones.Spine;

       [Tooltip("Bone name for Generic rigs (e.g., 'mixamorig:Spine' or 'mixamorig:Spine1')")] [SerializeField]
       private string genericBoneName = "mixamorig:Spine";

       [Tooltip("How much the body rotates to follow aim (0 = no rotation, 1 = full rotation)")]
       [SerializeField, Range(0f, 1f)]
       private float bodyRotationWeight = 0.7f;

       [Tooltip("How smoothly the body rotates")] [SerializeField]
       private float rotationSpeed = 10f;

       [Tooltip("Maximum up/down angle the body can rotate (prevents unrealistic bending)")] [SerializeField]
       private float maxVerticalAngle = 60f;

       [Tooltip("Should the entire character model rotate with the Player? (Usually yes for FPS)")] [SerializeField]
       private bool rotateCharacterModel = true;

       [Header("Advanced Settings")]
       [Tooltip("Enable to use Unity's built-in IK system (requires Humanoid rig)")]
       [SerializeField]
       private bool useBuiltInIK = false;

       [Tooltip("IK Look At weight (only used if useBuiltInIK is true)")] [SerializeField, Range(0f, 1f)]
       private float ikLookAtWeight = 1f;

       private Transform spineBone;
       private Quaternion originalSpineRotation;
       private Transform characterModelTransform;

       private void Start()
       {
           InitializeBodyIK();
       }

       private void InitializeBodyIK()
       {
           // Get animator if not assigned
           if (animator == null)
           {
               animator = GetComponentInChildren<Animator>();
               if (animator == null)
               {
                   Debug.LogError("UpperBodyIK: No Animator found!");
                   enabled = false;
                   return;
               }
           }

           // Check if animator is Humanoid or Generic
           if (animator.isHuman)
           {
               Debug.Log("UpperBodyIK: Detected HUMANOID rig");

               // Get spine bone using Humanoid system
               spineBone = animator.GetBoneTransform(targetBone);

               if (spineBone == null)
               {
                   Debug.LogError($"UpperBodyIK: Could not find bone {targetBone} in Humanoid rig!");
                   enabled = false;
                   return;
               }
           }
           else
           {
               Debug.Log("UpperBodyIK: Detected GENERIC rig - searching for bone by name");

               // For Generic rigs, find bone by name
               spineBone = FindBoneByName(animator.transform, genericBoneName);

               if (spineBone == null)
               {
                   Debug.LogError($"UpperBodyIK: Could not find bone '{genericBoneName}' in Generic rig!");
                   Debug.Log("Available bones:");
                   LogAllBones(animator.transform, 0);
                   enabled = false;
                   return;
               }

               Debug.Log($"UpperBodyIK: Found spine bone: {spineBone.name}");
           }

           // Store original rotation
           if (spineBone != null)
           {
               originalSpineRotation = spineBone.localRotation;
           }

           // Get character model transform
           characterModelTransform = animator.transform;

           // Create aim target if not assigned
           if (aimTarget == null)
           {
               // Create a target point in front of the camera
               GameObject targetObj = new GameObject("AimTarget");
               aimTarget = targetObj.transform;
               aimTarget.SetParent(transform);
               aimTarget.localPosition = Vector3.forward * 10f + Vector3.up * 1.5f;
               Debug.Log("UpperBodyIK: Created aim target automatically");
           }
       }

       /// <summary>
       /// Recursively search for a bone by name in the hierarchy
       /// </summary>
       private Transform FindBoneByName(Transform parent, string boneName)
       {
           // Check if this is the bone we're looking for
           if (parent.name == boneName)
           {
               return parent;
           }

           // Search children recursively
           foreach (Transform child in parent)
           {
               Transform found = FindBoneByName(child, boneName);
               if (found != null)
               {
                   return found;
               }
           }

           return null;
       }

       /// <summary>
       /// Log all bones in the hierarchy for debugging
       /// </summary>
       private void LogAllBones(Transform parent, int depth)
       {
           string indent = new string(' ', depth * 2);
           Debug.Log($"{indent}- {parent.name}");

           foreach (Transform child in parent)
           {
               LogAllBones(child, depth + 1);
           }
       }

       private void LateUpdate()
       {
           if (animator == null || aimTarget == null) return;

           // Rotate character model to match player rotation (for turning)
           if (rotateCharacterModel && characterModelTransform != null)
           {
               characterModelTransform.rotation = transform.rotation;
           }

           // Manual spine rotation (works with Generic and Humanoid rigs)
           if (!useBuiltInIK && spineBone != null)
           {
               RotateSpineManually();
           }
       }

       private void RotateSpineManually()
       {
           // Calculate direction to aim target
           Vector3 directionToTarget = aimTarget.position - spineBone.position;

           // Convert to local space of the spine bone's parent
           if (spineBone.parent != null)
           {
               directionToTarget = spineBone.parent.InverseTransformDirection(directionToTarget);
           }

           // Calculate target rotation
           Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

           // Limit vertical angle to prevent unrealistic bending
           Vector3 targetEuler = targetRotation.eulerAngles;
           float angleX = targetEuler.x > 180 ? targetEuler.x - 360 : targetEuler.x;
           angleX = Mathf.Clamp(angleX, -maxVerticalAngle, maxVerticalAngle);
           targetEuler.x = angleX;
           targetRotation = Quaternion.Euler(targetEuler);

           // Blend between original rotation and target rotation
           Quaternion blendedRotation = Quaternion.Slerp(
               originalSpineRotation,
               targetRotation,
               bodyRotationWeight
           );

           // Apply rotation smoothly
           spineBone.localRotation = Quaternion.Slerp(
               spineBone.localRotation,
               blendedRotation,
               Time.deltaTime * rotationSpeed
           );
       }

       // Unity's built-in IK system (only works with Humanoid rigs)
       private void OnAnimatorIK(int layerIndex)
       {
           if (!useBuiltInIK || animator == null || aimTarget == null) return;

           // Set look at target
           animator.SetLookAtWeight(ikLookAtWeight);
           animator.SetLookAtPosition(aimTarget.position);
       }

       #region Public Methods

       /// <summary>
       /// Set the target position to aim at
       /// </summary>
       public void SetAimTarget(Transform target)
       {
           aimTarget = target;
       }

       /// <summary>
       /// Set how much the body rotates (0-1)
       /// </summary>
       public void SetBodyRotationWeight(float weight)
       {
           bodyRotationWeight = Mathf.Clamp01(weight);
       }

       /// <summary>
       /// Enable or disable body rotation
       /// </summary>
       public void SetEnabled(bool enabled)
       {
           this.enabled = enabled;

           // Reset to original rotation when disabled
           if (!enabled && spineBone != null)
           {
               spineBone.localRotation = originalSpineRotation;
           }
       }

       /// <summary>
       /// Toggle between built-in IK and manual rotation
       /// </summary>
       public void SetUseBuiltInIK(bool useIK)
       {
           if (useIK && !animator.isHuman)
           {
               Debug.LogWarning("Cannot use built-in IK with non-Humanoid rig");
               return;
           }

           useBuiltInIK = useIK;
       }

       #endregion

       #region Debug

       private void OnDrawGizmos()
       {
           if (!Application.isPlaying || spineBone == null || aimTarget == null) return;

           // Draw line from spine to aim target
           Gizmos.color = Color.yellow;
           Gizmos.DrawLine(spineBone.position, aimTarget.position);

           // Draw aim target
           Gizmos.color = Color.red;
           Gizmos.DrawWireSphere(aimTarget.position, 0.1f);

           // Draw spine bone
           Gizmos.color = Color.green;
           Gizmos.DrawWireSphere(spineBone.position, 0.05f);
       }

       #endregion
   }

}