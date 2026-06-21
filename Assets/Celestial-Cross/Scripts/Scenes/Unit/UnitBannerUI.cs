using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

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
                bannerText.DOKill();
                // Crossfade animation
                bannerText.DOFade(0f, 0.15f).SetUpdate(true).OnComplete(() => {
                    bannerText.text = text;
                    bannerText.DOFade(1f, 0.2f).SetUpdate(true);
                    
                    // Subtle punch scale
                    bannerText.transform.DOKill();
                    bannerText.transform.localScale = Vector3.one;
                    bannerText.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f).SetUpdate(true);
                });
            }
        }
    }
}
