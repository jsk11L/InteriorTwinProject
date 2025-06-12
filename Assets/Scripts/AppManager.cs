using UnityEngine;
using System.Collections.Generic; // Para usar Listas y HashSets

// Clase para definir cada item de nuestra biblioteca de objetos
[System.Serializable]
public class LibraryItem
{
    public string uniqueID; // Un nombre único, ej: "silla_moderna_01"
    public GameObject prefab;   // El prefab del objeto 3D
    public Sprite icon;       // Un icono para mostrar en el botón de la UI (opcional)
}

public class AppManager : MonoBehaviour
{
    public static AppManager Instance { get; private set; }

    [Header("Biblioteca de Objetos")]
    public List<LibraryItem> objectLibrary = new List<LibraryItem>();

    // --- Estado Persistente ---
    // Guarda los IDs de los objetos que ya están en la escena de diseño
    public HashSet<string> activeObjectIDs = new HashSet<string>();
    // Guarda el ID del próximo objeto a instanciar al cambiar de escena
    public string objectToInstantiateNext;


    private void Awake()
    {
        // Patrón Singleton para que solo haya un AppManager y no se destruya
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    // Método para obtener un prefab de la biblioteca usando su ID único
    public GameObject GetPrefabByID(string id)
    {
        foreach (var item in objectLibrary)
        {
            if (item.uniqueID == id)
            {
                return item.prefab;
            }
        }
        Debug.LogWarning("No se encontró ningún prefab con el ID: " + id);
        return null;
    }
}