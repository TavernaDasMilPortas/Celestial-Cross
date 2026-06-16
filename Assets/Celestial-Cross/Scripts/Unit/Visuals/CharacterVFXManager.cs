using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

namespace CelestialCross.UnitVisuals
{
    public class CharacterVFXManager : MonoBehaviour
    {
        private static CharacterVFXManager _instance;
        private static bool applicationIsQuitting = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatic()
        {
            applicationIsQuitting = false;
            _instance = null;
        }

        public static CharacterVFXManager Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    return null;
                }

                if (_instance == null)
                {
                    var go = new GameObject("CharacterVFXManager");
                    _instance = go.AddComponent<CharacterVFXManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private Dictionary<Unit, SpriteRenderer> rendererCache = new Dictionary<Unit, SpriteRenderer>();
        private Material effectMaterial;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        private void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }

        public void RegisterRenderer(Unit unit, SpriteRenderer sr)
        {
            if (unit != null && sr != null)
            {
                rendererCache[unit] = sr;
                
                if (effectMaterial == null)
                {
                    Shader shader = Shader.Find("Custom/URP/SpriteOutline");
                    if (shader != null)
                    {
                        effectMaterial = new Material(shader);
                    }
                    else
                    {
                        Debug.LogWarning("[CharacterVFXManager] Shader 'Custom/URP/SpriteOutline' not found.");
                    }
                }
            }
        }

        public void UnregisterRenderer(Unit unit)
        {
            if (unit != null && rendererCache.ContainsKey(unit))
            {
                rendererCache.Remove(unit);
            }
        }

        public void PlayDamageEffect(Unit target, bool isCritical)
        {
            if (target == null || !rendererCache.TryGetValue(target, out var sr)) return;
            
            Color color = isCritical ? Color.yellow : Color.red;
            PlayEffect(sr, target.transform, color);
        }

        public void PlayHealEffect(Unit target)
        {
            if (target == null || !rendererCache.TryGetValue(target, out var sr)) return;
            
            PlayEffect(sr, target.transform, Color.green);
        }

        private void PlayEffect(SpriteRenderer sr, Transform tr, Color color)
        {
            if (sr == null || tr == null) return;

            // Cancela animações anteriores para não acumular
            tr.DOKill();
            sr.DOKill();

            // 1. Chacoalhar (Shake)
            tr.DOShakePosition(0.3f, strength: new Vector3(0.3f, 0, 0), vibrato: 10, randomness: 90, snapping: false, fadeOut: true);

            // 2. Piscar cor e Contorno (Outline)
            if (effectMaterial != null)
            {
                Material originalMat = sr.sharedMaterial;
                
                // Aplica o material de efeito temporário
                sr.sharedMaterial = effectMaterial;
                
                MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                sr.GetPropertyBlock(mpb);
                
                mpb.SetColor("_OutlineColor", color);
                mpb.SetFloat("_OutlineThickness", 1.5f);
                mpb.SetColor("_FlashColor", new Color(color.r, color.g, color.b, 0.7f));
                sr.SetPropertyBlock(mpb);

                // Anima o Alpha do Flash de volta para 0
                DOVirtual.Float(0.7f, 0f, 0.35f, (v) => 
                {
                    if (sr != null)
                    {
                        sr.GetPropertyBlock(mpb);
                        mpb.SetColor("_FlashColor", new Color(color.r, color.g, color.b, v));
                        if (v <= 0.05f)
                        {
                            mpb.SetFloat("_OutlineThickness", 0f);
                        }
                        sr.SetPropertyBlock(mpb);
                    }
                }).OnComplete(() => 
                {
                    if (sr != null)
                    {
                        // Retorna ao material original
                        sr.sharedMaterial = originalMat;
                        
                        // Limpa o PropertyBlock
                        sr.GetPropertyBlock(mpb);
                        mpb.SetColor("_FlashColor", new Color(0,0,0,0));
                        mpb.SetFloat("_OutlineThickness", 0f);
                        sr.SetPropertyBlock(mpb);
                    }
                });
            }
            else
            {
                // Fallback caso o shader falhe
                Color originalColor = sr.color;
                sr.DOColor(color, 0.1f).OnComplete(() => sr.DOColor(originalColor, 0.3f));
            }
        }
    }
}
