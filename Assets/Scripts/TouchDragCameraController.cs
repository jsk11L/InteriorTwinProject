using UnityEngine;
using UnityEngine.InputSystem;

public class TouchDragCameraController : MonoBehaviour
{
    private PlayerControls playerControls;
    private InputAction pointerPressAction;
    private InputAction pointerPositionAction;

    // Banderas de estado
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

        // Inicializar pitch basado en la rotación actual de la cámara
        currentPitch = GetCurrentPitch();
    }

    void OnDisable()
    {
        playerControls.Gameplay.Disable();
        pointerPressAction.performed -= OnPointerPress_Callback;
        pointerPressAction.canceled -= OnPointerRelease_Callback;
    }

    // El callback ahora solo levanta una bandera
    private void OnPointerPress_Callback(InputAction.CallbackContext context)
    {
        pointerJustPressed = true;
    }

    // Al soltar, detenemos el arrastre
    private void OnPointerRelease_Callback(InputAction.CallbackContext context)
    {
        isDragging = false;
    }

    void LateUpdate()
    {
        // 1. Comprobar si la cámara está en modo enfoque (lógica existente)
        if (InteractionStateManager.Instance != null && InteractionStateManager.Instance.IsObjectSelectedViewActive())
        {
            isDragging = false; 
            return;
        }

        // 2. Procesar un nuevo toque si ha ocurrido
        if (pointerJustPressed)
        {
            pointerJustPressed = false;

            // --- ¡NUEVA COMPROBACIÓN CRUCIAL! ---
            // Si otro script ya consumió el input de este frame, no hacemos nada.
            if (InteractionStateManager.Instance != null && InteractionStateManager.Instance.IsInputConsumedThisFrame)
            {
                isDragging = false; // Asegurarse de no empezar a arrastrar
                return;
            }
            // ------------------------------------

            // Si llegamos aquí, el input no fue consumido, así que procedemos como antes
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

        // 3. Si estamos en modo arrastre, mover la cámara (lógica existente)
        if (isDragging)
        {
            HandleCameraDrag();
        }
    }
    
    private void HandleCameraDrag()
    {
        Vector2 currentPointerPosition = pointerPositionAction.ReadValue<Vector2>();
        Vector2 delta = currentPointerPosition - lastPointerPosition;

        if (delta.sqrMagnitude > 0.01f) // Mover solo si hay un arrastre significativo
        {
            float xInput = invertX ? -delta.x : delta.x;
            float yInput = invertY ? -delta.y : delta.y;

            // Rotación Horizontal (Yaw): Alrededor del eje Y del mundo
            transform.Rotate(Vector3.up, -xInput * rotationSpeed, Space.World);

            // Rotación Vertical (Pitch): Calculada y limitada localmente
            currentPitch -= yInput * rotationSpeed;
            if (limitPitch)
            {
                currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
            }

            // Aplicar la rotación final
            transform.rotation = Quaternion.Euler(currentPitch, transform.eulerAngles.y, 0f); // Se fuerza el roll a 0
        }

        lastPointerPosition = currentPointerPosition;
    }

    private float GetCurrentPitch()
    {
        float pitch = transform.eulerAngles.x;
        // Convertir el ángulo de Euler a un rango de -180 a 180 para un manejo consistente
        if (pitch > 180f)
        {
            pitch -= 360f;
        }
        return pitch;
    }
}