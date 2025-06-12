using UnityEngine;

public class MovableObjectInfo : MonoBehaviour
{
    public string uniqueObjectID; 
    public Vector3 originalScale;

    void Awake()
    {
        // Guardar la escala solo si no ha sido guardada antes (importante para prefabs)
        if (originalScale == Vector3.zero)
        {
            originalScale = transform.localScale;
        }
    }

    // Se ejecuta cuando el objeto es creado e inicializado en la escena
    void Start()
    {
        // Se registra a sí mismo en la lista de objetos activos del AppManager
        if (AppManager.Instance != null)
        {
            AppManager.Instance.activeObjectIDs.Add(uniqueObjectID);
            Debug.Log($"Objeto '{uniqueObjectID}' registrado como activo.");
        }
    }

    // Se ejecuta justo antes de que el objeto sea destruido
    void OnDestroy()
    {
        // Se elimina a sí mismo de la lista de objetos activos
        if (AppManager.Instance != null)
        {
            AppManager.Instance.activeObjectIDs.Remove(uniqueObjectID);
            Debug.Log($"Objeto '{uniqueObjectID}' des-registrado.");
        }
    }
}