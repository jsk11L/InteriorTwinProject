using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LibraryItem
{
    public string uniqueID;
    public GameObject prefab;
    public Sprite icon;
}

public class AppManager : MonoBehaviour
{
    public static AppManager Instance { get; private set; }

    [Header("Biblioteca de Objetos")]
    public List<LibraryItem> objectLibrary = new List<LibraryItem>();
    public HashSet<string> activeObjectIDs = new HashSet<string>();
    public string objectToInstantiateNext;


    private void Awake()
    {
        
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