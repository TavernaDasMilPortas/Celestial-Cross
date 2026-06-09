using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CelestialCross.Scenes.Unit
{
    public class UnitBannerUI : MonoBehaviour
    {
        [Header("UI References")]
        public Image backgroundImage;
        public TextMeshProUGUI bannerText;

        public void SetBannerText(string text)
        {
            if (bannerText != null)
            {
                bannerText.text = text;
            }
        }
    }
}
