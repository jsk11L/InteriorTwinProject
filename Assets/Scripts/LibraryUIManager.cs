using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; 
using UnityEngine.Rendering.Universal; 
using System.Collections; 
using System.Collections.Generic; 

public class LibraryUIManager : MonoBehaviour
{
    [Header("Referencias de UI")]
    public GameObject libraryButtonPrefab; 
    public Transform buttonContainer;      

    [Header("Configuración de Botones")]
    public Color disabledButtonColor = new Color(0.5f, 0.5f, 0.5f, 0.7f); 

    
    void Start()
    {
        
        if (AppManager.Instance == null)
        {
            Debug.LogError("AppManager no encontrado. Asegúrate de que la escena inicial (UIScene) se cargue primero.");
            
            
            return;
        }

        
        PopulateLibrary();
    }

    
    void PopulateLibrary()
    {
        
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        
        List<LibraryItem> libraryItems = AppManager.Instance.objectLibrary;
        
        HashSet<string> activeIDs = AppManager.Instance.activeObjectIDs;

        
        foreach (var item in libraryItems)
        {
            GameObject buttonGO = Instantiate(libraryButtonPrefab, buttonContainer);
            
            
            TextMeshProUGUI buttonText = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                
                buttonText.text = item.uniqueID; 
            }

            Button button = buttonGO.GetComponent<Button>();

            
            if (activeIDs.Contains(item.uniqueID))
            {
                
                button.interactable = false;
                Image buttonImage = buttonGO.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = disabledButtonColor;
                }
            }
            else
            {
                
                string itemID = item.uniqueID; 
                button.onClick.AddListener(() => OnLibraryItemClicked(itemID));
            }
        }
    }

    
    public void OnLibraryItemClicked(string objectID)
    {
        
        if (AppManager.Instance != null)
        {
            AppManager.Instance.objectToInstantiateNext = objectID;
        }
        
        
        StartCoroutine(UnloadThisScene());
    }

    
    public void GoBackToDesignScene()
    {
        
        if (AppManager.Instance != null)
        {
            AppManager.Instance.objectToInstantiateNext = null;
        }
        
        
        StartCoroutine(UnloadThisScene());
    }

    
    private IEnumerator UnloadThisScene()
    {
        
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

        
        yield return SceneManager.UnloadSceneAsync("LibraryScene");
    }
}