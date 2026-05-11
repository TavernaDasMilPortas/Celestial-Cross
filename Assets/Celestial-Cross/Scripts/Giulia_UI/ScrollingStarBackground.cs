using UnityEngine;
using UnityEngine.UI;

namespace CelestialCross.UI
{
    [RequireComponent(typeof(RawImage))]
    public class ScrollingStarBackground : MonoBehaviour
    {
        public Vector2 scrollSpeed = new Vector2(0.1f, 0.1f);
        private RawImage _rawImage;

        void Awake()
        {
            _rawImage = GetComponent<RawImage>();
        }

        void Update()
        {
            if (_rawImage != null)
            {
                Rect uvRect = _rawImage.uvRect;
                uvRect.x += scrollSpeed.x * Time.deltaTime;
                uvRect.y += scrollSpeed.y * Time.deltaTime;
                _rawImage.uvRect = uvRect;
            }
        }
    }
}