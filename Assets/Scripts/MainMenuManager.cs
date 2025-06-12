using UnityEngine;
using UnityEngine.SceneManagement; // ¡Muy importante añadir esta línea!

public class MainMenuManager : MonoBehaviour
{
    // Este método será llamado por el botón "Diseñar"
    public void LoadDesignScene()
    {
        // Carga la escena usando el nombre exacto del archivo de escena que pusiste en Build Settings
        SceneManager.LoadScene("DesignEnvironment");
    }
}