using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

namespace CelestialCross.UI
{
    public class TargetButtonUI : MonoBehaviour, IPointerClickHandler
    {
        public TextMeshProUGUI numberText;
        public Image backgroundImage;

        [Header("Colors")]
        public Color defaultColor = Color.white;
        public Color selectedColor = Color.yellow;
        
        private int targetIndex;
        private Action<int> onClickAction;

        public void Setup(int index, Action<int> onClick)
        {
            targetIndex = index;
            onClickAction = onClick;
            
            if (numberText != null)
            {
                numberText.text = (index + 1).ToString();
            }
            
            SetSelected(false);
        }

        public void SetSelected(bool isSelected)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = isSelected ? selectedColor : defaultColor;
            }
            if (numberText != null)
            {
                numberText.color = isSelected ? Color.black : Color.white;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onClickAction?.Invoke(targetIndex);
        }
    }
}
