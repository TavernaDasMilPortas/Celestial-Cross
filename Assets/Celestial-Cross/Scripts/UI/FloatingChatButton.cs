using UnityEngine;
using UnityEngine.EventSystems;

namespace CelestialCross.UI
{
    public class FloatingChatButton : MonoBehaviour, IPointerClickHandler
    {
        public GameObject chatPanelToToggle;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (chatPanelToToggle != null)
            {
                chatPanelToToggle.SetActive(!chatPanelToToggle.activeSelf);
            }
        }
    }
}
