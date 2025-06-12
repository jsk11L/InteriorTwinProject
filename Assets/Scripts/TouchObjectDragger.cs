using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class TouchObjectDragger : MonoBehaviour
{
    private PlayerControls playerControls;
    private Camera mainCamera;

    // Acciones de Input
    private InputAction pointerPositionAction;
    private InputAction pointerPressAction;

    // --- Variables de Estado y Arrastre ---
    // Usaremos 'currentTarget' consistentemente en todo el script.
    private GameObject currentTarget = null;
    private Rigidbody currentTargetRigidbody = null;
    private bool isDragging = false;
    public static bool isManipulatingObject = false;

    // Lógica Diferida del Clic
    private bool primaryPointerJustPressed = false;
    private Vector2 primaryPointerDownPosition;
    
    // Variables de Arrastre
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
        // --- LÍNEA CLAVE DE LA SOLUCIÓN ---
        // Resetea el foco del EventSystem para prevenir que la UI "secuestre" el input.
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
        // ------------------------------------

        // Lógica existente para terminar la manipulación del objeto
        if (currentTargetRigidbody != null && !currentTargetRigidbody.useGravity)
        {
            currentTargetRigidbody.linearVelocity = Vector3.zero;
            currentTargetRigidbody.angularVelocity = Vector3.zero;
        }
        
        isDragging = false;
        isManipulatingObject = false;
        currentTarget = null;
        currentTargetRigidbody = null;
    }

    void Update()
    {
        // 1. Procesar un nuevo clic si ha ocurrido
        if (primaryPointerJustPressed)
        {
            HandlePrimaryPointerDown();
            primaryPointerJustPressed = false; // Consumir la bandera
        }

        // 2. Determinar el estado de la manipulación basado en el input actual
        bool primaryFingerIsDown = pointerPressAction.IsPressed();
        
        // Si el dedo primario está abajo y tenemos un objetivo, estamos manipulando
        if (primaryFingerIsDown && currentTarget != null)
        {
            isManipulatingObject = true;
            // Si estamos arrastrando (y no pellizcando, cuando lo teníamos), procesar el arrastre
            if (isDragging)
            {
                HandleDrag();
            }
        }
        else
        {
            // Si no hay dedos presionados, no puede haber manipulación
            if (isDragging) isDragging = false;
            isManipulatingObject = false;
            currentTarget = null; // Limpiar objetivo si el dedo se levanta
        }
    }

    private void HandlePrimaryPointerDown()
    {
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            InteractionStateManager.Instance.IsInputConsumedThisFrame = true;
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(primaryPointerDownPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider != null && hit.collider.CompareTag("MovableObject"))
            {
                InteractionStateManager.Instance.IsInputConsumedThisFrame = true;
                
                currentTarget = hit.collider.gameObject;
                currentTargetRigidbody = currentTarget.GetComponent<Rigidbody>();
                isDragging = true;
                isManipulatingObject = true;
                InteractionStateManager.Instance.SetSelectedObject(currentTarget);

                interactionPlane = new Plane(mainCamera.transform.forward, currentTarget.transform.position);
                if (interactionPlane.Raycast(ray, out float distance))
                {
                    dragOffset = currentTarget.transform.position - ray.GetPoint(distance);
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
        // Obtener la posición deseada por el dedo en el plano de interacción
        Ray ray = mainCamera.ScreenPointToRay(pointerPositionAction.ReadValue<Vector2>());
        if (!interactionPlane.Raycast(ray, out float distance))
        {
            return; // No se pudo proyectar, no hacer nada
        }
        Vector3 targetPosition = ray.GetPoint(distance) + dragOffset;

        if (currentTargetRigidbody == null) return;

        // Calcular el vector de movimiento
        Vector3 currentPosition = currentTargetRigidbody.position;
        Vector3 movementVector = targetPosition - currentPosition;
        float movementDistance = movementVector.magnitude;

        // Si hay movimiento, usar SweepTest para predecir colisiones
        if (movementDistance > 0.001f)
        {
            if (currentTargetRigidbody.SweepTest(movementVector.normalized, out RaycastHit sweepHit, movementDistance))
            {
                // COLISIÓN PREDICHA: Mover el objeto solo hasta el punto de impacto
                float safeDistance = Mathf.Max(0, sweepHit.distance - 0.01f); // Pequeño margen
                currentTargetRigidbody.MovePosition(currentPosition + movementVector.normalized * safeDistance);
            }
            else
            {
                // CAMINO LIBRE: Mover a la posición deseada
                currentTargetRigidbody.MovePosition(targetPosition);
            }
        }
    }
}