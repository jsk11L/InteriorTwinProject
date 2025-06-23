using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class TouchObjectDragger : MonoBehaviour
{
    private PlayerControls playerControls;
    private Camera mainCamera;

    private InputAction pointerPositionAction;
    private InputAction pointerPressAction;

    
    private Rigidbody currentTargetRigidbody = null; 
    private bool isDragging = false;
    public static bool isManipulatingObject = false; 

    
    private bool primaryPointerJustPressed = false;
    private Vector2 primaryPointerDownPosition;
    
    
    private Vector3 dragOffset;
    private Plane interactionPlane;

    void Awake()
    {
        playerControls = new PlayerControls();
        mainCamera = Camera.main;
    }

    void OnEnable()
    {
        pointerPositionAction = playerControls.Gameplay.PointerPosition;
        pointerPressAction = playerControls.Gameplay.PointerPress;
        playerControls.Gameplay.Enable();

        pointerPressAction.performed += OnPrimaryPointerDown_Callback;
        pointerPressAction.canceled += OnPrimaryPointerUp_Callback;
    }

    void OnDisable()
    {
        if (playerControls != null)
        {
            playerControls.Gameplay.Disable();
            pointerPressAction.performed -= OnPrimaryPointerDown_Callback;
            pointerPressAction.canceled -= OnPrimaryPointerUp_Callback;
        }
    }

    private void OnPrimaryPointerDown_Callback(InputAction.CallbackContext context)
    {
        primaryPointerJustPressed = true;
        primaryPointerDownPosition = pointerPositionAction.ReadValue<Vector2>();
    }

    private void OnPrimaryPointerUp_Callback(InputAction.CallbackContext context)
    {
        
        isDragging = false;
        isManipulatingObject = false;
        currentTargetRigidbody = null; 

        
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    void Update()
    {
        
        if (primaryPointerJustPressed)
        {
            HandlePrimaryPointerDown();
            primaryPointerJustPressed = false;
        }

        
        if (isDragging && pointerPressAction.IsPressed())
        {
            HandleDrag();
        }
    }

    private void HandlePrimaryPointerDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            InteractionStateManager.Instance.IsInputConsumedThisFrame = true;
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(primaryPointerDownPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject selectedObjectGlobal = InteractionStateManager.Instance.GetSelectedObject();
            
            if (hit.collider != null && hit.collider.CompareTag("MovableObject"))
            {
                GameObject hitObject = hit.collider.gameObject;
                InteractionStateManager.Instance.IsInputConsumedThisFrame = true;

                
                if (hitObject == selectedObjectGlobal)
                {
                    
                    isDragging = true;
                    isManipulatingObject = true;
                    currentTargetRigidbody = hitObject.GetComponent<Rigidbody>();
                    
                    
                    interactionPlane = new Plane(mainCamera.transform.forward, hitObject.transform.position);
                    if (interactionPlane.Raycast(ray, out float distance))
                    {
                        dragOffset = hitObject.transform.position - ray.GetPoint(distance);
                    }
                }
                else
                {
                    
                    InteractionStateManager.Instance.SetSelectedObject(hitObject);
                    isDragging = false; 
                }
            }
            else 
            {
                if (selectedObjectGlobal != null)
                {
                    InteractionStateManager.Instance.IsInputConsumedThisFrame = true;
                }
                InteractionStateManager.Instance.ClearSelection();
            }
        }
        else 
        {
            if (InteractionStateManager.Instance.GetSelectedObject() != null)
            {
                InteractionStateManager.Instance.IsInputConsumedThisFrame = true;
            }
            InteractionStateManager.Instance.ClearSelection();
        }
    }

    private void HandleDrag()
    {
        if (currentTargetRigidbody == null) return;

        Ray ray = mainCamera.ScreenPointToRay(pointerPositionAction.ReadValue<Vector2>());
        if (interactionPlane.Raycast(ray, out float distance))
        {
            Vector3 targetPosition = ray.GetPoint(distance) + dragOffset;
            
            
            Vector3 currentPosition = currentTargetRigidbody.position;
            Vector3 movementVector = targetPosition - currentPosition;
            float movementDistance = movementVector.magnitude;

            if (movementDistance > 0.001f)
            {
                if (currentTargetRigidbody.SweepTest(movementVector.normalized, out RaycastHit sweepHit, movementDistance))
                {
                    
                    float safeDistance = Mathf.Max(0, sweepHit.distance - 0.01f);
                    currentTargetRigidbody.MovePosition(currentPosition + movementVector.normalized * safeDistance);
                }
                else
                {
                    
                    currentTargetRigidbody.MovePosition(targetPosition);
                }
            }
        }
    }
}