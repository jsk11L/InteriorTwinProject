using UnityEngine;
using UnityEngine.InputSystem;

public class TouchDragCameraController : MonoBehaviour
{
    private PlayerControls playerControls;
    private InputAction pointerPressAction;
    private InputAction pointerPositionAction;

    
    private bool isDragging = false;
    private bool pointerJustPressed = false;

    private Vector2 lastPointerPosition;

    [Header("Configuración de Rotación")]
    public float rotationSpeed = 0.2f;
    public bool invertX = false;
    public bool invertY = false;

    [Header("Límites de Inclinación Vertical (Pitch)")]
    public bool limitPitch = true;
    public float minPitch = -80f;
    public float maxPitch = 80f;
    private float currentPitch = 0f;

    void Awake()
    {
        playerControls = new PlayerControls();
    }

    void OnEnable()
    {
        pointerPressAction = playerControls.Gameplay.PointerPress;
        pointerPositionAction = playerControls.Gameplay.PointerPosition;
        playerControls.Gameplay.Enable();

        pointerPressAction.performed += OnPointerPress_Callback;
        pointerPressAction.canceled += OnPointerRelease_Callback;

        
        currentPitch = GetCurrentPitch();
    }

    void OnDisable()
    {
        playerControls.Gameplay.Disable();
        pointerPressAction.performed -= OnPointerPress_Callback;
        pointerPressAction.canceled -= OnPointerRelease_Callback;
    }

    
    private void OnPointerPress_Callback(InputAction.CallbackContext context)
    {
        pointerJustPressed = true;
    }

    
    private void OnPointerRelease_Callback(InputAction.CallbackContext context)
    {
        isDragging = false;
    }

    void LateUpdate()
    {
        
        if (InteractionStateManager.Instance != null && InteractionStateManager.Instance.IsObjectSelectedViewActive())
        {
            isDragging = false; 
            return;
        }

        
        if (pointerJustPressed)
        {
            pointerJustPressed = false;

            
            
            if (InteractionStateManager.Instance != null && InteractionStateManager.Instance.IsInputConsumedThisFrame)
            {
                isDragging = false; 
                return;
            }
            

            
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                isDragging = false;
            }
            else
            {
                isDragging = true;
                lastPointerPosition = pointerPositionAction.ReadValue<Vector2>();
            }
        }

        
        if (isDragging)
        {
            HandleCameraDrag();
        }
    }
    
    private void HandleCameraDrag()
    {
        Vector2 currentPointerPosition = pointerPositionAction.ReadValue<Vector2>();
        Vector2 delta = currentPointerPosition - lastPointerPosition;

        if (delta.sqrMagnitude > 0.01f) 
        {
            float xInput = invertX ? -delta.x : delta.x;
            float yInput = invertY ? -delta.y : delta.y;

            
            transform.Rotate(Vector3.up, -xInput * rotationSpeed, Space.World);

            
            currentPitch -= yInput * rotationSpeed;
            if (limitPitch)
            {
                currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
            }

            
            transform.rotation = Quaternion.Euler(currentPitch, transform.eulerAngles.y, 0f); 
        }

        lastPointerPosition = currentPointerPosition;
    }

    private float GetCurrentPitch()
    {
        float pitch = transform.eulerAngles.x;
        
        if (pitch > 180f)
        {
            pitch -= 360f;
        }
        return pitch;
    }
}