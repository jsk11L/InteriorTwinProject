using UnityEngine;
using UnityEngine.SceneManagement; 

public class MainMenuManager : MonoBehaviour
{
    
    public void LoadDesignScene()
    {
        
        SceneManager.LoadScene("DesignEnvironment");
    }
}