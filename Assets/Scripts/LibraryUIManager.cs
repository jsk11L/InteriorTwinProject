using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // Necesario para TextMeshPro. Asegúrate de tenerlo importado.
using UnityEngine.Rendering.Universal; // Necesario para URP
using System.Collections; // Necesario para Corutinas
using System.Collections.Generic; // Necesario para Listas

public class LibraryUIManager : MonoBehaviour
{
    [Header("Referencias de UI")]
    public GameObject libraryButtonPrefab; // El prefab de un botón que usarás como plantilla
    public Transform buttonContainer;      // El objeto "Content" dentro de tu ScrollView

    [Header("Configuración de Botones")]
    public Color disabledButtonColor = new Color(0.5f, 0.5f, 0.5f, 0.7f); // Color para botones de objetos ya instanciados

    // El método Start se ejecuta una vez cuando la escena de la biblioteca se carga
    void Start()
    {
        // Asegurarse de que el AppManager exista antes de continuar
        if (AppManager.Instance == null)
        {
            Debug.LogError("AppManager no encontrado. Asegúrate de que la escena inicial (UIScene) se cargue primero.");
            // Opcional: Cargar la escena de menú si no se encuentra
            // SceneManager.LoadScene("UIScene"); 
            return;
        }

        // Llamar al método que crea y configura los botones
        PopulateLibrary();
    }

    // Este método lee la biblioteca del AppManager y crea los botones en la UI
    void PopulateLibrary()
    {
        // Limpiar botones antiguos si por alguna razón ya existían
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        // Obtener la lista de objetos de la biblioteca del AppManager
        List<LibraryItem> libraryItems = AppManager.Instance.objectLibrary;
        // Obtener la lista de los IDs de objetos que ya están en la escena de diseño
        HashSet<string> activeIDs = AppManager.Instance.activeObjectIDs;

        // Crear un botón por cada objeto en la biblioteca
        foreach (var item in libraryItems)
        {
            GameObject buttonGO = Instantiate(libraryButtonPrefab, buttonContainer);
            
            // Configurar el texto del botón con el ID del objeto
            TextMeshProUGUI buttonText = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                // Puedes usar el ID o un nombre más amigable si lo añades a la clase LibraryItem
                buttonText.text = item.uniqueID; 
            }

            Button button = buttonGO.GetComponent<Button>();

            // Comprobar si el objeto ya está en la escena de diseño
            if (activeIDs.Contains(item.uniqueID))
            {
                // Si ya está, desactivar el botón y cambiar su color para que se vea "grisado"
                button.interactable = false;
                Image buttonImage = buttonGO.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = disabledButtonColor;
                }
            }
            else
            {
                // Si el objeto está disponible, asignar la función al OnClick del botón
                string itemID = item.uniqueID; // Copiar a variable local para la lambda
                button.onClick.AddListener(() => OnLibraryItemClicked(itemID));
            }
        }
    }

    // Este método se llama cuando se hace clic en un botón de un objeto disponible
    public void OnLibraryItemClicked(string objectID)
    {
        // Guardar el ID del objeto que queremos instanciar
        if (AppManager.Instance != null)
        {
            AppManager.Instance.objectToInstantiateNext = objectID;
        }
        
        // Descargar esta escena para volver al entorno de diseño
        StartCoroutine(UnloadThisScene());
    }

    // Este método se llama desde el botón "Volver"
    public void GoBackToDesignScene()
    {
        // Asegurarse de no instanciar nada
        if (AppManager.Instance != null)
        {
            AppManager.Instance.objectToInstantiateNext = null;
        }
        
        // Descargar esta escena para volver
        StartCoroutine(UnloadThisScene());
    }

    // Corutina para descargar la escena de forma segura, quitando primero la cámara de la pila
    private IEnumerator UnloadThisScene()
    {
        // Antes de descargar la escena, quitamos nuestra cámara de la pila de la cámara principal
        Camera mainCamera = Camera.main;
        GameObject uiCameraObject = GameObject.FindWithTag("UICamera");

        if (mainCamera != null && uiCameraObject != null)
        {
            var mainCameraData = mainCamera.GetUniversalAdditionalCameraData();
            Camera uiCamera = uiCameraObject.GetComponent<Camera>();

            if (mainCameraData != null && uiCamera != null && mainCameraData.cameraStack.Contains(uiCamera))
            {
                mainCameraData.cameraStack.Remove(uiCamera);
            }
        }

        // Ahora sí, descargamos la escena de la biblioteca
        yield return SceneManager.UnloadSceneAsync("LibraryScene");
    }
}