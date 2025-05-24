// using System.Collections;
// using UnityEngine;
// using UnityEngine.XR.Interaction.Toolkit;
// using UnityEngine.Events;

// public class ClawScript : MonoBehaviour
// {
//     [Header("Movement")]
//     public float moveSpeed = 1f;
//     public float slideBackSpeed = 1.5f;
//     public float dropDist = 2f;
//     public float dropSpeed = 2f;
    
//     [Header("Machine Bounds")]
//     public Vector2 machineBoundsX = new Vector2(-1f, 1f);
//     public Vector2 machineBoundsZ = new Vector2(-1f, 1f);
//     public float machineHeight = 2f;
    
//     [Header("Return Position")]
//     public bool returnToInitialPosition = true;
//     public Transform returnPosition;
    
//     [Header("XR Socket Interaction")]
//     public UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socketInteractor;
    
//     [Header("Haptic Feedback")]
//     public bool enableHaptics = true;
//     [Range(0f, 1f)]
//     public float grabHapticIntensity = 0.8f;
//     public float grabHapticDuration = 0.3f;
//     [Range(0f, 1f)]
//     public float dropHapticIntensity = 0.4f;
//     public float dropHapticDuration = 0.2f;
    
//     [Header("Coordinate Space")]
//     public bool useLocalCoordinates = true;

//     // Event for UI/other systems
//     public UnityEvent<GameObject> OnDropCompleted = new UnityEvent<GameObject>();

//     // Internal
//     private UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable socketedInteractable = null;
//     private Vector3 moveDir;
//     private bool isDropping;
//     private Vector3 startLocalPos;
//     private Vector3 currentLocalPos;
//     private XRJoystickController joystickController;

//     void Start()
//     {
//         startLocalPos   = transform.localPosition;
//         currentLocalPos = startLocalPos;
//         joystickController = FindFirstObjectByType<XRJoystickController>();
        
//         if (socketInteractor == null)
//             socketInteractor = GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
        
//         if (socketInteractor != null)
//         {
//             socketInteractor.selectEntered.AddListener(OnSocketSelect);
//             socketInteractor.selectExited .AddListener(OnSocketRelease);
//             socketInteractor.socketActive = false;
//         }
//     }

//     void OnDestroy()
//     {
//         if (socketInteractor != null)
//         {
//             socketInteractor.selectEntered.RemoveListener(OnSocketSelect);
//             socketInteractor.selectExited.RemoveListener(OnSocketRelease);
//         }
//     }

//     void Update()
//     {
//         if (!isDropping && moveDir != Vector3.zero)
//             SlideClaw();
//     }

//     void LateUpdate()
//     {
//         Physics.SyncTransforms();
//     }

//     private void OnSocketSelect(SelectEnterEventArgs args)
//     {
//         socketedInteractable = args.interactableObject;
//         if (enableHaptics)
//             SendHapticFeedback(grabHapticIntensity, grabHapticDuration);
//     }

//     private void OnSocketRelease(SelectExitEventArgs args)
//     {
//         if (args.interactableObject?.transform != null)
//             OnDropCompleted.Invoke(args.interactableObject.transform.gameObject);
//         socketedInteractable = null;
//     }

//     public void ForceSocketRelease()
//     {
//         if (socketInteractor != null && socketInteractor.hasSelection)
//         {
//             var toyObject = socketedInteractable?.transform?.gameObject;
//             var toyRb     = toyObject?.GetComponent<Rigidbody>();

//             socketInteractor.socketActive = false;
//             if (toyRb != null)
//                 StartCoroutine(ApplyForceAfterDelay(toyRb));
//             StartCoroutine(ResetSocketAfterDelay());
//         }
//     }

//     private IEnumerator ApplyForceAfterDelay(Rigidbody rb)
//     {
//         yield return new WaitForSeconds(0.1f);
//         rb.AddForce(Vector3.down * 2f, ForceMode.Impulse);
//     }

//     private IEnumerator ResetSocketAfterDelay()
//     {
//         yield return new WaitForSeconds(0.5f);
//         if (!isDropping)
//             socketInteractor.socketActive = true;
//     }

//     void SlideClaw()
//     {
//         var newPos = currentLocalPos;
//         newPos.x += moveDir.x * moveSpeed * Time.deltaTime;
//         newPos.z += moveDir.z * moveSpeed * Time.deltaTime;
//         newPos.x = Mathf.Clamp(newPos.x, machineBoundsX.x, machineBoundsX.y);
//         newPos.z = Mathf.Clamp(newPos.z, machineBoundsZ.x, machineBoundsZ.y);
//         newPos.y = startLocalPos.y;

//         currentLocalPos = newPos;
//         if (useLocalCoordinates)
//             transform.localPosition = currentLocalPos;
//         else
//             transform.position = transform.parent.TransformPoint(currentLocalPos);
//     }

//     public void SetDirection(Vector3 d)
//     {
//         moveDir = d;
//         if (moveDir.magnitude > 1f)
//             moveDir.Normalize();
//         moveDir.y = 0f;
//     }

//     public void StopMovement()
//     {
//         moveDir = Vector3.zero;
//     }

//     public void Drop()
//     {
//         if (!isDropping)
//             StartCoroutine(DropRoutine());
//     }

//     private IEnumerator DropRoutine()
//     {
//         isDropping = true;
//         var dropStartPos = currentLocalPos;
//         var dropTargetPos = dropStartPos + Vector3.down * dropDist;

//         // Lower
//         while (currentLocalPos.y > dropTargetPos.y)
//         {
//             currentLocalPos += Vector3.down * dropSpeed * Time.deltaTime;
//             if (useLocalCoordinates)
//                 transform.localPosition = currentLocalPos;
//             else
//                 transform.position = transform.parent.TransformPoint(currentLocalPos);
//             yield return null;
//         }

//         currentLocalPos.y = dropTargetPos.y;
//         if (useLocalCoordinates)
//             transform.localPosition = currentLocalPos;
//         else
//             transform.position = transform.parent.TransformPoint(currentLocalPos);

//         if (socketInteractor != null)
//             socketInteractor.socketActive = true;

//         yield return new WaitForSeconds(0.5f);

//         // Raise
//         while (currentLocalPos.y < startLocalPos.y)
//         {
//             currentLocalPos += Vector3.up * dropSpeed * Time.deltaTime;
//             if (useLocalCoordinates)
//                 transform.localPosition = currentLocalPos;
//             else
//                 transform.position = transform.parent.TransformPoint(currentLocalPos);
//             yield return null;
//         }

//         currentLocalPos.y = startLocalPos.y;
//         if (useLocalCoordinates)
//             transform.localPosition = currentLocalPos;
//         else
//             transform.position = transform.parent.TransformPoint(currentLocalPos);

//         // Return to initial if needed
//         if (returnToInitialPosition)
//             yield return StartCoroutine(ReturnToInitialPositionRoutine());

//         var grabbedToy = (socketInteractor != null && socketInteractor.hasSelection && socketedInteractable != null)
//             ? socketedInteractable.transform.gameObject
//             : null;

//         ForceSocketRelease();
//         OnDropCompleted.Invoke(grabbedToy);

//         if (enableHaptics)
//             SendHapticFeedback(dropHapticIntensity, dropHapticDuration);

//         isDropping = false;
//     }

//     private IEnumerator ReturnToInitialPositionRoutine()
//     {
//         Vector3 targetPos;
//         if (returnPosition != null)
//         {
//             if (useLocalCoordinates)
//             {
//                 targetPos = transform.parent.InverseTransformPoint(returnPosition.position);
//                 targetPos.y = startLocalPos.y;
//             }
//             else
//             {
//                 targetPos = returnPosition.position;
//                 targetPos.y = transform.position.y;
//             }
//         }
//         else targetPos = startLocalPos;

//         var currentFlat = new Vector3(currentLocalPos.x, 0f, currentLocalPos.z);
//         var targetFlat  = new Vector3(targetPos.x,      0f, targetPos.z);
//         float dist       = Vector3.Distance(currentFlat, targetFlat);
//         if (dist < 0.01f) yield break;

//         float startTime = Time.time;
//         while (dist > 0.01f && Time.time - startTime < 5f)
//         {
//             var dir = (targetFlat - currentFlat).normalized;
//             float step = Mathf.Min(slideBackSpeed * Time.deltaTime, dist);
//             currentLocalPos.x += dir.x * step;
//             currentLocalPos.z += dir.z * step;

//             if (useLocalCoordinates)
//                 transform.localPosition = currentLocalPos;
//             else
//                 transform.position = new Vector3(currentLocalPos.x, transform.position.y, currentLocalPos.z);

//             currentFlat = new Vector3(currentLocalPos.x, 0f, currentLocalPos.z);
//             dist = Vector3.Distance(currentFlat, targetFlat);
//             yield return null;
//         }

//         if (useLocalCoordinates)
//         {
//             currentLocalPos.x = targetPos.x;
//             currentLocalPos.z = targetPos.z;
//             transform.localPosition = currentLocalPos;
//         }
//         else
//         {
//             var finalPos = new Vector3(targetPos.x, transform.position.y, targetPos.z);
//             transform.position = finalPos;
//             currentLocalPos = transform.localPosition;
//         }
//     }

//     public bool CanControl() => !isDropping;

//     private void SendHapticFeedback(float intensity, float duration)
//     {
//         joystickController?.SendHapticFeedback(intensity, duration);
//     }
// }

using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

public class ClawScript : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1f;
    public float slideBackSpeed = 1.5f;
    public float dropDist = 2f;
    public float dropSpeed = 2f;
    
    [Header("Machine Bounds")]
    public Vector2 machineBoundsX = new Vector2(-1f, 1f);
    public Vector2 machineBoundsZ = new Vector2(-1f, 1f);
    public float machineHeight = 2f;
    
    [Header("Return Position")]
    public bool returnToInitialPosition = true;
    public Transform returnPosition;
    
    [Header("XR Socket Interaction")]
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socketInteractor;
    
    [Header("Haptic Feedback")]
    public bool enableHaptics = true;
    [Range(0f, 1f)]
    public float grabHapticIntensity = 0.8f;
    public float grabHapticDuration = 0.3f;
    [Range(0f, 1f)]
    public float dropHapticIntensity = 0.4f;
    public float dropHapticDuration = 0.2f;
    
    [Header("Coordinate Space")]
    public bool useLocalCoordinates = true;

    // Event for UI/other systems
    public UnityEvent<GameObject> OnDropCompleted = new UnityEvent<GameObject>();

    // Internal
    private UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable socketedInteractable = null;
    private Vector3 moveDir;
    private bool isDropping;
    private Vector3 startLocalPos;
    private Vector3 currentLocalPos;
    private JoystickControl joystickController;

    void Start()
    {
        startLocalPos   = transform.localPosition;
        currentLocalPos = startLocalPos;
        joystickController = FindFirstObjectByType<JoystickControl>();
        
        if (socketInteractor == null)
            socketInteractor = GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
        
        if (socketInteractor != null)
        {
            socketInteractor.selectEntered.AddListener(OnSocketSelect);
            socketInteractor.selectExited .AddListener(OnSocketRelease);
            socketInteractor.socketActive = false;
        }
    }

    void OnDestroy()
    {
        if (socketInteractor != null)
        {
            socketInteractor.selectEntered.RemoveListener(OnSocketSelect);
            socketInteractor.selectExited.RemoveListener(OnSocketRelease);
        }
    }

    void Update()
    {
        if (!isDropping && moveDir != Vector3.zero)
            SlideClaw();
    }

    void LateUpdate()
    {
        Physics.SyncTransforms();
    }

    private void OnSocketSelect(SelectEnterEventArgs args)
    {
        socketedInteractable = args.interactableObject;
        if (enableHaptics)
            SendHapticFeedback(grabHapticIntensity, grabHapticDuration);
    }

    private void OnSocketRelease(SelectExitEventArgs args)
    {
        if (args.interactableObject?.transform != null)
            OnDropCompleted.Invoke(args.interactableObject.transform.gameObject);
        socketedInteractable = null;
    }

    public void ForceSocketRelease()
    {
        if (socketInteractor != null && socketInteractor.hasSelection)
        {
            var toyObject = socketedInteractable?.transform?.gameObject;
            var toyRb     = toyObject?.GetComponent<Rigidbody>();

            socketInteractor.socketActive = false;
            if (toyRb != null)
                StartCoroutine(ApplyForceAfterDelay(toyRb));
            StartCoroutine(ResetSocketAfterDelay());
        }
    }

    private IEnumerator ApplyForceAfterDelay(Rigidbody rb)
    {
        yield return new WaitForSeconds(0.1f);
        rb.AddForce(Vector3.down * 2f, ForceMode.Impulse);
    }

    private IEnumerator ResetSocketAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        if (!isDropping)
            socketInteractor.socketActive = true;
    }

    void SlideClaw()
    {
        var newPos = currentLocalPos;
        newPos.x += moveDir.x * moveSpeed * Time.deltaTime;
        newPos.z += moveDir.z * moveSpeed * Time.deltaTime;
        newPos.x = Mathf.Clamp(newPos.x, machineBoundsX.x, machineBoundsX.y);
        newPos.z = Mathf.Clamp(newPos.z, machineBoundsZ.x, machineBoundsZ.y);
        newPos.y = startLocalPos.y;

        currentLocalPos = newPos;
        if (useLocalCoordinates)
            transform.localPosition = currentLocalPos;
        else
            transform.position = transform.parent.TransformPoint(currentLocalPos);
    }

    public void SetDirection(Vector3 d)
    {
        Debug.Log($"[Claw] SetDirection({d})");
        moveDir = d;
        if (moveDir.magnitude > 1f)
            moveDir.Normalize();
        moveDir.y = 0f;
    }

    public void StopMovement()
    {
        moveDir = Vector3.zero;
    }

    public void Drop()
    {
        if (!isDropping)
            StartCoroutine(DropRoutine());
    }

    private IEnumerator DropRoutine()
    {
        isDropping = true;
        var dropStartPos = currentLocalPos;
        var dropTargetPos = dropStartPos + Vector3.down * dropDist;

        // Lower
        while (currentLocalPos.y > dropTargetPos.y)
        {
            currentLocalPos += Vector3.down * dropSpeed * Time.deltaTime;
            if (useLocalCoordinates)
                transform.localPosition = currentLocalPos;
            else
                transform.position = transform.parent.TransformPoint(currentLocalPos);
            yield return null;
        }

        currentLocalPos.y = dropTargetPos.y;
        if (useLocalCoordinates)
            transform.localPosition = currentLocalPos;
        else
            transform.position = transform.parent.TransformPoint(currentLocalPos);

        if (socketInteractor != null)
            socketInteractor.socketActive = true;

        yield return new WaitForSeconds(0.5f);

        // Raise
        while (currentLocalPos.y < startLocalPos.y)
        {
            currentLocalPos += Vector3.up * dropSpeed * Time.deltaTime;
            if (useLocalCoordinates)
                transform.localPosition = currentLocalPos;
            else
                transform.position = transform.parent.TransformPoint(currentLocalPos);
            yield return null;
        }

        currentLocalPos.y = startLocalPos.y;
        if (useLocalCoordinates)
            transform.localPosition = currentLocalPos;
        else
            transform.position = transform.parent.TransformPoint(currentLocalPos);

        // Return to initial if needed
        if (returnToInitialPosition)
            yield return StartCoroutine(ReturnToInitialPositionRoutine());

        var grabbedToy = (socketInteractor != null && socketInteractor.hasSelection && socketedInteractable != null)
            ? socketedInteractable.transform.gameObject
            : null;

        ForceSocketRelease();
        OnDropCompleted.Invoke(grabbedToy);

        if (enableHaptics)
            SendHapticFeedback(dropHapticIntensity, dropHapticDuration);

        isDropping = false;
    }

    private IEnumerator ReturnToInitialPositionRoutine()
    {
        Vector3 targetPos;
        if (returnPosition != null)
        {
            if (useLocalCoordinates)
            {
                targetPos = transform.parent.InverseTransformPoint(returnPosition.position);
                targetPos.y = startLocalPos.y;
            }
            else
            {
                targetPos = returnPosition.position;
                targetPos.y = transform.position.y;
            }
        }
        else targetPos = startLocalPos;

        var currentFlat = new Vector3(currentLocalPos.x, 0f, currentLocalPos.z);
        var targetFlat  = new Vector3(targetPos.x,      0f, targetPos.z);
        float dist       = Vector3.Distance(currentFlat, targetFlat);
        if (dist < 0.01f) yield break;

        float startTime = Time.time;
        while (dist > 0.01f && Time.time - startTime < 5f)
        {
            var dir = (targetFlat - currentFlat).normalized;
            float step = Mathf.Min(slideBackSpeed * Time.deltaTime, dist);
            currentLocalPos.x += dir.x * step;
            currentLocalPos.z += dir.z * step;

            if (useLocalCoordinates)
                transform.localPosition = currentLocalPos;
            else
                transform.position = new Vector3(currentLocalPos.x, transform.position.y, currentLocalPos.z);

            currentFlat = new Vector3(currentLocalPos.x, 0f, currentLocalPos.z);
            dist = Vector3.Distance(currentFlat, targetFlat);
            yield return null;
        }

        if (useLocalCoordinates)
        {
            currentLocalPos.x = targetPos.x;
            currentLocalPos.z = targetPos.z;
            transform.localPosition = currentLocalPos;
        }
        else
        {
            var finalPos = new Vector3(targetPos.x, transform.position.y, targetPos.z);
            transform.position = finalPos;
            currentLocalPos = transform.localPosition;
        }
    }

    public bool CanControl() => !isDropping;

    private void SendHapticFeedback(float intensity, float duration)
    {
        joystickController?.SendHapticFeedback(intensity, duration);
    }
}