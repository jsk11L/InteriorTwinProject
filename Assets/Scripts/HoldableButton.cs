using UnityEngine;
using UnityEngine.EventSystems; 

public class HoldableButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    
    public bool IsPressed { get; private set; }

    
    public void OnPointerDown(PointerEventData eventData)
    {
        IsPressed = true;
    }

    
    public void OnPointerUp(PointerEventData eventData)
    {
        IsPressed = false;
    }

    
    private void OnDisable()
    {
        IsPressed = false;
    }
}