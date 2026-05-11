using UnityEngine;
using UnityEngine.UI;

namespace CelestialCross.UI
{
    [RequireComponent(typeof(Graphic))]
    public class UIHoleRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
    {
        [Tooltip("If true, the UI is clickable EVERYWHERE EXCEPT the transparent parts (Alpha < 0.1) of its image. Use this to let clicks pass through the 'hole'.")]
        public bool blockClickOnTransparent = true;
        
        private Graphic _graphic;

        void Awake()
        {
            _graphic = GetComponent<Graphic>();
            
            // Note: UGUI native alpha testing requires Read/Write enabled on the Texture.
            // If using a RawImage, we can fallback to the Graphic Raycast setting:
            if (_graphic is Image image)
            {
                image.alphaHitTestMinimumThreshold = 0.1f;
            }
        }

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            if (!blockClickOnTransparent) return true;

            // In most cases, just setting alphaHitTestMinimumThreshold on Unity's Image does the job.
            // However, with custom RawImage / Shaders, it might let clicks block. 
            // If you want clicks to PASS THROUGH the hole, we just return true where it's NOT a hole.
            
            // For custom shaders with a Mask, Unity natively supports passing clicks through transparent
            // pixels IF alphaHitTestMinimumThreshold is used with an "Image" component.
            
            return true; 
        }
    }
}