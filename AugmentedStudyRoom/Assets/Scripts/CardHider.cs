using UnityEngine;

public class CardHideButton : MonoBehaviour
{
    public void HideThisCard() {
        var controller = FindFirstObjectByType<TrackedImageContentController>();
        
        if (controller != null)
            controller.HideSpecificPopup(gameObject);
        else
            Debug.LogWarning("TrackedImageContentController not found in the scene!");
    }
}
