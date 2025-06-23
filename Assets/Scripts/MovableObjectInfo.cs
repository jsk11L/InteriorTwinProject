using UnityEngine;

public class MovableObjectInfo : MonoBehaviour
{
    public string uniqueObjectID; 
    public Vector3 originalScale;

    void Awake()
    {
        
        if (originalScale == Vector3.zero)
        {
            originalScale = transform.localScale;
        }
    }

    
    void Start()
    {
        
        if (AppManager.Instance != null)
        {
            AppManager.Instance.activeObjectIDs.Add(uniqueObjectID);
            Debug.Log($"Objeto '{uniqueObjectID}' registrado como activo.");
        }
    }

    
    void OnDestroy()
    {
        
        if (AppManager.Instance != null)
        {
            AppManager.Instance.activeObjectIDs.Remove(uniqueObjectID);
            Debug.Log($"Objeto '{uniqueObjectID}' des-registrado.");
        }
    }
}