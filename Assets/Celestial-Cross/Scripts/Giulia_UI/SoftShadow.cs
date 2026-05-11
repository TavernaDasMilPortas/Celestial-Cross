using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CelestialCross.UI
{
    [AddComponentMenu("Celestial Cross/UI/Soft Shadow")]
    public class SoftShadow : Shadow
    {
        [Range(1, 10)]
        [Tooltip("The number of shadow passes. Higher is smoother but more expensive.")]
        public int blurIterations = 4;

        [Range(0.1f, 20f)]
        [Tooltip("How far the blur spreads from the central shadow.")]
        public float blurSpread = 3f;

        [Range(0.1f, 1f)]
        [Tooltip("Intensity of the shadows. Lower values make it more transparent.")]
        public float alphaMultiplier = 0.5f;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (graphic != null)
                graphic.SetVerticesDirty();
        }
#endif

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive()) return;

            List<UIVertex> originalVerts = new List<UIVertex>();
            vh.GetUIVertexStream(originalVerts);

            if (originalVerts.Count == 0) return;

            List<UIVertex> finalVerts = new List<UIVertex>();
            
            // Central main shadow
            AddShadowPass(originalVerts, finalVerts, effectColor, new Vector2(effectDistance.x, effectDistance.y));

            // Blurred passes
            for (int i = 1; i <= blurIterations; i++)
            {
                Color32 fadedColor = effectColor;
                fadedColor.a = (byte)(effectColor.a * alphaMultiplier / i); 

                float sX = effectDistance.x + (i * blurSpread);
                float sY = effectDistance.y + (i * blurSpread);

                AddShadowPass(originalVerts, finalVerts, fadedColor, new Vector2(sX, sY));
                AddShadowPass(originalVerts, finalVerts, fadedColor, new Vector2(sX, -sY));
                AddShadowPass(originalVerts, finalVerts, fadedColor, new Vector2(-sX, sY));
                AddShadowPass(originalVerts, finalVerts, fadedColor, new Vector2(-sX, -sY));
            }

            // Original content on top
            finalVerts.AddRange(originalVerts);

            vh.Clear();
            vh.AddUIVertexTriangleStream(finalVerts);
        }

        private void AddShadowPass(List<UIVertex> originalVerts, List<UIVertex> finalVerts, Color32 color, Vector2 offset)
        {
            for (int i = 0; i < originalVerts.Count; i++)
            {
                UIVertex vt = originalVerts[i];
                
                Vector3 position = vt.position;
                position.x += offset.x;
                position.y += offset.y;
                vt.position = position;

                var newColor = color;
                if (useGraphicAlpha)
                    newColor.a = (byte)((newColor.a * vt.color.a) / 255);
                vt.color = newColor;

                finalVerts.Add(vt);
            }
        }
    }
}
