using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Rendering.Universal; // Necesario para URP

public class DesignSceneUIManager : MonoBehaviour
{
    private Camera mainCamera;
    private UniversalAdditionalCameraData mainCameraData;

    void Awake()
    {
        // Obtener la cámara principal y sus datos de URP
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCameraData = mainCamera.GetUniversalAdditionalCameraData();
        }
    }

    public void LoadMainMenuScene()
    {
        SceneManager.LoadScene("UIScene");
    }

    public void LoadLibraryScene()
    {
        if (InteractionStateManager.Instance != null)
        {
            InteractionStateManager.Instance.ClearSelection();
        }
        
        // Iniciar una corutina para cargar la escena y luego apilar las cámaras
        StartCoroutine(LoadAndStackLibraryRoutine());
    }

    private IEnumerator LoadAndStackLibraryRoutine()
    {
        // Cargar la escena de forma aditiva
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("LibraryScene", LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            yield return null; // Esperar a que la escena se cargue completamente
        }

        // Una vez cargada, encontrar la cámara de la UI por su tag
        GameObject uiCameraObject = GameObject.FindWithTag("UICamera");
        if (uiCameraObject != null)
        {
            Camera uiCamera = uiCameraObject.GetComponent<Camera>();
            // Añadir la cámara de la UI a la "pila" de la cámara principal
            if (uiCamera != null && mainCameraData != null && !mainCameraData.cameraStack.Contains(uiCamera))
            {
                mainCameraData.cameraStack.Add(uiCamera);
            }
        }
    }
}