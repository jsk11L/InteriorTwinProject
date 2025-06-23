using UnityEngine;
using UnityEngine.SceneManagement; 

public class NavigationManager : MonoBehaviour
{
    
    public void GoToMainMenuScene()
    {
        
        SceneManager.LoadScene("UIScene");
    }

    
    public void GoToDesignScene()
    {
        
        SceneManager.LoadScene("DesignEnvironment");
    }

    
    public void GoToEnvironmentsScene()
    {
        
        SceneManager.LoadScene("EnvironmentScene");
    }

    
    public void GoToProfileScene()
    {
        
        SceneManager.LoadScene("ProfileScene");
    }
}