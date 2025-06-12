using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InteractionStateManager : MonoBehaviour
{
    public static InteractionStateManager Instance { get; private set; }

    [Header("Referencias Principales")]
    public Camera mainCamera;
    public TouchDragCameraController touchDragCameraController;

    [Header("Enfoque de Cámara")]
    public Transform objectFocusCameraPosition;
    public float cameraFocusLerpSpeed = 5f;

    [Header("UI y Feedback")]
    public GameObject objectManipulationPanel;
    public Slider scaleSlider;
    public float depthStep = 0.1f; // Cuánto se mueve el objeto en cada clic
    public Material highlightMaterial;
    public GameObject bottomNavigationBar; // Arrastra aquí tu panel con el botón Inicio
    // En InteractionStateManager.cs
    [Header("Instanciación")]
    public Transform defaultSpawnPoint; // Arrastra tu DefaultSpawnPoint aquí
    public bool IsInputConsumedThisFrame { get; set; }

    // Variables de Estado Privadas
    private GameObject _selectedObject = null;
    private Material _originalMaterialOfSelected;
    private Renderer _rendererOfSelected;
    private bool _isObjectSelectedViewActive = false;
    private Vector3 _originalCameraPosition;
    private Quaternion _originalCameraRotation;
    private Vector3 _trueOriginalScale; // Guardará la escala REALMENTE original del objeto
    private Rigidbody _selectedRigidbody = null;

    void Start()
    {
        Debug.Log("InteractionStateManager: Escena DesignEnvironment cargada. Revisando si hay objetos para instanciar...");

        if (AppManager.Instance == null)
        {
            Debug.LogError("¡ERROR FATAL! AppManager.Instance es NULO. Asegúrate de que AppManager esté en tu primera escena (UIScene) y no se destruya.");
            return;
        }

        string objectIDToInstantiate = AppManager.Instance.objectToInstantiateNext;

        if (!string.IsNullOrEmpty(objectIDToInstantiate))
        {
            Debug.Log($"Se ha solicitado instanciar el objeto con ID: '{objectIDToInstantiate}'");

            GameObject prefabToCreate = AppManager.Instance.GetPrefabByID(objectIDToInstantiate);

            if (prefabToCreate == null)
            {
                Debug.LogError($"¡FALLO! No se encontró ningún prefab en la biblioteca del AppManager con el ID '{objectIDToInstantiate}'. Revisa la lista 'Object Library' en el Inspector del AppManager.");
                return;
            }

            if (defaultSpawnPoint == null)
            {
                Debug.LogError("¡FALLO! El 'Default Spawn Point' no está asignado en el Inspector de InteractionStateManager. Instanciando en la posición (0,0,0).");
                Instantiate(prefabToCreate, Vector3.zero, Quaternion.identity);
            }
            else
            {
                Debug.Log($"ÉXITO: Instanciando '{prefabToCreate.name}' en la posición {defaultSpawnPoint.position}.");
                Instantiate(prefabToCreate, defaultSpawnPoint.position, prefabToCreate.transform.rotation);
            }

            // Limpiar la variable para no instanciarlo de nuevo
            AppManager.Instance.objectToInstantiateNext = null;
        }
        else
        {
            Debug.Log("No hay objetos pendientes para instanciar.");
        }
    }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        if (mainCamera == null) mainCamera = Camera.main;
        if (touchDragCameraController == null) touchDragCameraController = mainCamera.GetComponent<TouchDragCameraController>();
        if (objectManipulationPanel != null) objectManipulationPanel.SetActive(false);
    }

    // --- Suscripción a eventos de escena ---
    void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded; // Escuchar cuándo se cierra una escena
    }

    void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded; // Dejar de escuchar
    }

    // Este método se llamará automáticamente cada vez que una escena se cierre
    private void OnSceneUnloaded(Scene scene)
    {
        // Comprobamos si la escena que se cerró es la biblioteca
        if (scene.name == "LibraryScene")
        {
            Debug.Log("LibraryScene se ha cerrado. Comprobando si hay que instanciar un objeto...");
            CheckForObjectInstantiation();
        }
    }

    // El método que contiene la lógica de instanciación, ahora llamado por el evento
    private void CheckForObjectInstantiation()
    {
        if (AppManager.Instance == null)
        {
            Debug.LogError("AppManager no encontrado.");
            return;
        }

        string objectIDToInstantiate = AppManager.Instance.objectToInstantiateNext;

        if (!string.IsNullOrEmpty(objectIDToInstantiate))
        {
            GameObject prefabToCreate = AppManager.Instance.GetPrefabByID(objectIDToInstantiate);
            if (prefabToCreate != null && defaultSpawnPoint != null)
            {
                Instantiate(prefabToCreate, defaultSpawnPoint.position, prefabToCreate.transform.rotation);
                Debug.Log($"ÉXITO: Se ha instanciado '{objectIDToInstantiate}'.");
            }
            else
            {
                Debug.LogError($"FALLO al instanciar: Prefab con ID '{objectIDToInstantiate}' o DefaultSpawnPoint no encontrado.");
            }
            AppManager.Instance.objectToInstantiateNext = null;
        }
        else
        {
            Debug.Log("No hay objetos pendientes para instanciar.");
        }
    }

    void Update()
    {
        // Reseteamos la bandera de "input consumido" al PRINCIPIO de cada frame.
        // Esto asegura que está limpia para cualquier nuevo evento de input en este frame.
        IsInputConsumedThisFrame = false;

        // El resto de la lógica de Update (el enfoque suave de la cámara) sigue aquí:
        if (_isObjectSelectedViewActive && _selectedObject != null && objectFocusCameraPosition != null)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, objectFocusCameraPosition.position, Time.deltaTime * cameraFocusLerpSpeed);
            Quaternion targetLookRotation = Quaternion.LookRotation(_selectedObject.transform.position - mainCamera.transform.position);
            mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, targetLookRotation, Time.deltaTime * cameraFocusLerpSpeed);
        }
    }

    public void SetSelectedObject(GameObject newSelection)
    {
        if (_selectedObject == newSelection) return;

        // --- LÓGICA PARA EL OBJETO DESELECCIONADO ANTERIORMENTE ---
        if (_selectedObject != null) 
        {
            // Restaurar material (esto ya lo teníamos)
            if (_rendererOfSelected != null)
            {
                _rendererOfSelected.material = _originalMaterialOfSelected;
            }

            // Restaurar estado físico (dejar que las físicas tomen el control)
            if (_selectedRigidbody != null) 
            {
                // ¡IMPORTANTE! Primero lo volvemos no-kinemático
                _selectedRigidbody.isKinematic = false; 

                // Y AHORA, si no usa gravedad, le quitamos la velocidad residual.
                if (!_selectedRigidbody.useGravity)
                {
                    _selectedRigidbody.linearVelocity = Vector3.zero;
                    _selectedRigidbody.angularVelocity = Vector3.zero;
                }
            }
            
        }

        // --- LÓGICA PARA EL NUEVO OBJETO SELECCIONADO ---
        _selectedObject = newSelection;

        if (_selectedObject != null)
        {
            // Obtener Rigidbody y hacerlo Kinemático (ignora físicas y gravedad)
            _selectedRigidbody = _selectedObject.GetComponent<Rigidbody>();
            if (_selectedRigidbody != null)
            {
                _selectedRigidbody.isKinematic = true;
            }

            // Lógica de highlight (ya la teníamos)
            _rendererOfSelected = _selectedObject.GetComponent<Renderer>();
            if (_rendererOfSelected != null) {
                _originalMaterialOfSelected = _rendererOfSelected.material;
                _rendererOfSelected.material = highlightMaterial;
            }

            // Lógica de cámara y UI (ya la teníamos)
            if (!_isObjectSelectedViewActive)
            {
                _originalCameraPosition = mainCamera.transform.position;
                _originalCameraRotation = mainCamera.transform.rotation;
            }
            _isObjectSelectedViewActive = true;
            if (touchDragCameraController != null) touchDragCameraController.enabled = false;

            // Lógica de slider de escala (ya la teníamos)
            MovableObjectInfo objectInfo = _selectedObject.GetComponent<MovableObjectInfo>();
            if (objectInfo != null) _trueOriginalScale = objectInfo.originalScale;
            else _trueOriginalScale = _selectedObject.transform.localScale;

            if (scaleSlider != null)
            {
                float currentScaleMultiplier = _selectedObject.transform.localScale.x / _trueOriginalScale.x;
                scaleSlider.onValueChanged.RemoveAllListeners();
                scaleSlider.value = currentScaleMultiplier;
                scaleSlider.onValueChanged.AddListener(OnScaleSliderChanged);
            }

            if (bottomNavigationBar != null) bottomNavigationBar.SetActive(false);

            ShowObjectManipulationUI(true);
        }
        else // Si newSelection es null, significa que estamos deseleccionando sin seleccionar uno nuevo.
        {
            // La lógica para restaurar el objeto anterior ya se ejecutó arriba.
            // Ahora solo limpiamos los estados restantes.
            _selectedRigidbody = null;
            _rendererOfSelected = null;

            _isObjectSelectedViewActive = false;
            if (touchDragCameraController != null) touchDragCameraController.enabled = true;

            if (bottomNavigationBar != null) bottomNavigationBar.SetActive(true);

            ShowObjectManipulationUI(false);
        }
    }

    public void ClearSelection()
    {
        // El método público para deseleccionar simplemente llama a SetSelectedObject con null.
        // Esto centraliza toda la lógica de cambio de estado en un solo lugar.
        SetSelectedObject(null);
    }

    private void ShowObjectManipulationUI(bool show)
    {
        if (objectManipulationPanel != null) objectManipulationPanel.SetActive(show);
    }

    // --- Métodos de Manipulación (conectados a UI) ---
    public void OnScaleSliderChanged(float value)
    {
        if (_selectedObject != null)
        {
            // El valor del slider (ej. 0.2 a 3.0) multiplica la escala original VERDADERA
            _selectedObject.transform.localScale = _trueOriginalScale * value;
        }
    }

    public void RotateSelectedObjectLeft()
    {
        if (_selectedObject != null)
        {
            // Rotamos alrededor del eje Y DEL MUNDO. Esto asegura un giro horizontal predecible (yaw).
            // Una rotación negativa en Y hace que el objeto gire hacia la derecha, mostrando su cara izquierda.
            _selectedObject.transform.Rotate(Vector3.up, -90f, Space.World);
        }
    }

    public void RotateSelectedObjectUp()
    {
        if (_selectedObject != null)
        {
            // Rotamos alrededor del eje X DE LA CÁMARA (su vector "derecha").
            // Esto produce una inclinación (pitch) consistente desde tu punto de vista.
            // Una rotación negativa alrededor de este eje hace que el objeto se incline "hacia arriba".
            _selectedObject.transform.Rotate(mainCamera.transform.right, -90f, Space.World);
        }
    }

    public void MoveObjectCloser()
    {
        if (_selectedObject == null) return;
        // Mover hacia la cámara (en la dirección opuesta al 'forward' de la cámara)
        _selectedObject.transform.position -= mainCamera.transform.forward * depthStep;
    }

    public void MoveObjectFarther()
    {
        if (_selectedObject == null) return;
        // Mover lejos de la cámara
        _selectedObject.transform.position += mainCamera.transform.forward * depthStep;
    }

    // En InteractionStateManager.cs
    public void DeleteSelectedObject()
    {
        if (_selectedObject != null)
        {
            GameObject objectToDestroy = _selectedObject;
            ClearSelection(); // Deselecciona primero para limpiar el estado
            Destroy(objectToDestroy); // Luego destruye el objeto
        }
    }

    public GameObject GetSelectedObject() => _selectedObject;
    public bool IsObjectSelectedViewActive() => _isObjectSelectedViewActive;
}