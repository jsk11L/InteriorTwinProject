using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Rendering.Universal; 

public class DesignSceneUIManager : MonoBehaviour
{
    private Camera mainCamera;
    private UniversalAdditionalCameraData mainCameraData;

    void Awake()
    {
        
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
        
        
        StartCoroutine(LoadAndStackLibraryRoutine());
    }

    private IEnumerator LoadAndStackLibraryRoutine()
    {
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("LibraryScene", LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            yield return null; 
        }

        
        GameObject uiCameraObject = GameObject.FindWithTag("UICamera");
        if (uiCameraObject != null)
        {
            Camera uiCamera = uiCameraObject.GetComponent<Camera>();
            
            if (uiCamera != null && mainCameraData != null && !mainCameraData.cameraStack.Contains(uiCamera))
            {
                mainCameraData.cameraStack.Add(uiCamera);
            }
        }
    }
}