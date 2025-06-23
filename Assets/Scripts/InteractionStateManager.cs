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
    public HoldableButton moveCloserButton;
    public HoldableButton moveFartherButton;
    public float depthStep = 0.1f; 
    public Material highlightMaterial;
    public GameObject bottomNavigationBar; 
    
    [Header("Instanciación")]
    public Transform defaultSpawnPoint; 
    public bool IsInputConsumedThisFrame { get; set; }

    
    private GameObject _selectedObject = null;
    private Material _originalMaterialOfSelected;
    private Renderer _rendererOfSelected;
    private bool _isObjectSelectedViewActive = false;
    private Vector3 _originalCameraPosition;
    private Quaternion _originalCameraRotation;
    private Vector3 _trueOriginalScale; 
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

    
    void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded; 
    }

    void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded; 
    }

    
    private void OnSceneUnloaded(Scene scene)
    {
        
        if (scene.name == "LibraryScene")
        {
            Debug.Log("LibraryScene se ha cerrado. Comprobando si hay que instanciar un objeto...");
            CheckForObjectInstantiation();
        }
    }

    
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
        
        
        IsInputConsumedThisFrame = false;

        if (_selectedObject != null && _isObjectSelectedViewActive)
        {
            
            if (moveCloserButton != null && moveCloserButton.IsPressed)
            {
                
                _selectedObject.transform.position -= mainCamera.transform.forward * depthStep * Time.deltaTime;
            }
            else if (moveFartherButton != null && moveFartherButton.IsPressed)
            {
                
                _selectedObject.transform.position += mainCamera.transform.forward * depthStep * Time.deltaTime;
            }
            


            
            if (objectFocusCameraPosition != null)
            {
                mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, objectFocusCameraPosition.position, Time.deltaTime * cameraFocusLerpSpeed);
                Quaternion targetLookRotation = Quaternion.LookRotation(_selectedObject.transform.position - mainCamera.transform.position);
                mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, targetLookRotation, Time.deltaTime * cameraFocusLerpSpeed);
            }
        }
    }

    public void SetSelectedObject(GameObject newSelection)
    {
        if (_selectedObject == newSelection) return;

        
        if (_selectedObject != null) 
        {
            
            if (_rendererOfSelected != null)
            {
                _rendererOfSelected.material = _originalMaterialOfSelected;
            }

            
            if (_selectedRigidbody != null) 
            {
                
                _selectedRigidbody.isKinematic = false; 

                
                if (!_selectedRigidbody.useGravity)
                {
                    _selectedRigidbody.linearVelocity = Vector3.zero;
                    _selectedRigidbody.angularVelocity = Vector3.zero;
                }
            }
            
        }

        
        _selectedObject = newSelection;

        if (_selectedObject != null)
        {
            
            _selectedRigidbody = _selectedObject.GetComponent<Rigidbody>();
            if (_selectedRigidbody != null)
            {
                _selectedRigidbody.isKinematic = true;
            }

            
            _rendererOfSelected = _selectedObject.GetComponent<Renderer>();
            if (_rendererOfSelected != null) {
                _originalMaterialOfSelected = _rendererOfSelected.material;
                _rendererOfSelected.material = highlightMaterial;
            }

            
            if (!_isObjectSelectedViewActive)
            {
                _originalCameraPosition = mainCamera.transform.position;
                _originalCameraRotation = mainCamera.transform.rotation;
            }
            _isObjectSelectedViewActive = true;
            if (touchDragCameraController != null) touchDragCameraController.enabled = false;

            
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
        else 
        {
            
            
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
        
        
        SetSelectedObject(null);
    }

    private void ShowObjectManipulationUI(bool show)
    {
        if (objectManipulationPanel != null) objectManipulationPanel.SetActive(show);
    }

    
    public void OnScaleSliderChanged(float value)
    {
        if (_selectedObject != null)
        {
            
            _selectedObject.transform.localScale = _trueOriginalScale * value;
        }
    }

    public void RotateSelectedObjectLeft()
    {
        if (_selectedObject != null)
        {
            
            
            _selectedObject.transform.Rotate(Vector3.up, -90f, Space.World);
        }
    }

    public void RotateSelectedObjectUp()
    {
        if (_selectedObject != null)
        {
            
            
            
            _selectedObject.transform.Rotate(mainCamera.transform.right, -90f, Space.World);
        }
    }

    
    public void DeleteSelectedObject()
    {
        if (_selectedObject != null)
        {
            GameObject objectToDestroy = _selectedObject;
            ClearSelection(); 
            Destroy(objectToDestroy); 
        }
    }

    public GameObject GetSelectedObject() => _selectedObject;
    public bool IsObjectSelectedViewActive() => _isObjectSelectedViewActive;
}