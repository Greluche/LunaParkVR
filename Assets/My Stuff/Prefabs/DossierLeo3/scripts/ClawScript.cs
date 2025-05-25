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

    [Header("Audio")]
    public AudioClip movementSound;
    private AudioSource movementAudioSource;
    private bool wasMovingLastFrame = false;

    // Event for UI/other systems
    public UnityEvent<GameObject> OnDropCompleted = new UnityEvent<GameObject>();

    // Internal
    private UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable socketedInteractable = null;
    private Vector3 moveDir;
    private bool isDropping;
    private Vector3 startLocalPos;
    private Vector3 currentLocalPos;
    private Joystick joystickController;

//initialize position and finds the required components and audiosource
    void Start()
    {
        startLocalPos = transform.localPosition;
        currentLocalPos = startLocalPos;
        joystickController = FindFirstObjectByType<Joystick>();

        if (socketInteractor == null)
            socketInteractor = GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();

        if (socketInteractor != null)
        {
            socketInteractor.selectEntered.AddListener(OnSocketSelect);
            socketInteractor.selectExited.AddListener(OnSocketRelease);
            socketInteractor.socketActive = false;
        }
        
        movementAudioSource = gameObject.AddComponent<AudioSource>();
        movementAudioSource.clip = movementSound;
        movementAudioSource.loop = true; // So it keeps playing while moving
        movementAudioSource.playOnAwake = false;
    }

// method that unregister the event listeners when the claw is destroyed
    void OnDestroy()
    {
        if (socketInteractor != null)
        {
            socketInteractor.selectEntered.RemoveListener(OnSocketSelect);
            socketInteractor.selectExited.RemoveListener(OnSocketRelease);
        }
    }

// main method that handles the claw mvmt, plays and stops the sounds and update the claw position
    void Update()
    {
        bool isMoving = (!isDropping && moveDir != Vector3.zero);

        if (isMoving && !wasMovingLastFrame)
        {
            if (movementSound != null && !movementAudioSource.isPlaying)
                movementAudioSource.Play();
        }
        else if (!isMoving && wasMovingLastFrame)
        {
            if (movementAudioSource.isPlaying)
                movementAudioSource.Stop();
        }

        wasMovingLastFrame = isMoving;

        if (isMoving)
            SlideClaw();
    }

// methid to synchronize the physics of the claw after the mvmt
    void LateUpdate()
    {
        Physics.SyncTransforms();
    }

//when a toy is grabbed, sets the reference, and triggers haptic feedback
    private void OnSocketSelect(SelectEnterEventArgs args)
    {
        socketedInteractable = args.interactableObject;
        if (enableHaptics)
            SendHapticFeedback(grabHapticIntensity, grabHapticDuration);
    }

// when a toy is released +  triggers dropCompleted events
    private void OnSocketRelease(SelectExitEventArgs args)
    {
        if (args.interactableObject?.transform != null)
            OnDropCompleted.Invoke(args.interactableObject.transform.gameObject);
        socketedInteractable = null;
    }

// forces the claw to release the toys when above the dropZone (its initial pos)
    public void ForceSocketRelease()
    {
        if (socketInteractor != null && socketInteractor.hasSelection)
        {
            var toyObject = socketedInteractable?.transform?.gameObject;
            var toyRb = toyObject?.GetComponent<Rigidbody>();

            socketInteractor.socketActive = false;
            if (toyRb != null)
                StartCoroutine(ApplyForceAfterDelay(toyRb));
            StartCoroutine(ResetSocketAfterDelay());
        }
    }

// method to wait slightly before apllying a downward force to release the toy
    private IEnumerator ApplyForceAfterDelay(Rigidbody rb)
    {
        yield return new WaitForSeconds(0.1f);
        rb.AddForce(Vector3.down * 2f, ForceMode.Impulse);
    }

// to wait slightly before reactivating the socket
    private IEnumerator ResetSocketAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        if (!isDropping)
            socketInteractor.socketActive = true;
    }

// method to make the claw slide using the moveDir vector from setDirection
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

// called in the joystick script to set the claw direction
    public void SetDirection(Vector3 d)
    {
        moveDir = d;
        if (moveDir.magnitude > 1f)
            moveDir.Normalize();
        moveDir.y = 0f;
    }

// stops the claw mvmt
    public void StopMovement()
    {
        moveDir = Vector3.zero;
    }

// called by the button script when the button is pressed to drop the claw
    public void Drop()
    {
        if (!isDropping)
            StartCoroutine(DropRoutine());
    }

    // handles the full drop routine

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

// forces the claw to slide back to initial pos after a drop
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
        var targetFlat = new Vector3(targetPos.x, 0f, targetPos.z);
        float dist = Vector3.Distance(currentFlat, targetFlat);
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

// returns true when the claw can be controlled meaning its not dropping
    public bool CanControl() => !isDropping;

// haptic fb for the joystick
    private void SendHapticFeedback(float intensity, float duration)
    {
        joystickController?.SendHapticFeedback(intensity, duration);
    }
}