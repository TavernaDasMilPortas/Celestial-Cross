using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace CelestialCross.Dialogue.Graph
{
    public class DialogueSceneryController : MonoBehaviour
    {
        [SerializeField] private RectTransform sceneryRoot;
        
        private DialogueScenery currentScenery;
        private List<GameObject> activeLayers = new List<GameObject>();
        
        public enum SceneryLoadMode { Animated, IdleOnly, Static }

        public void TransitionTo(DialogueScenery newScenery)
        {
            if (newScenery == currentScenery) return;

            if (currentScenery != null)
            {
                UnloadScenery(() => LoadScenery(newScenery, SceneryLoadMode.Animated));
            }
            else
            {
                LoadScenery(newScenery, SceneryLoadMode.Animated);
            }
        }

        public void LoadScenery(DialogueScenery scenery, SceneryLoadMode mode = SceneryLoadMode.Animated)
        {
            currentScenery = scenery;
            if (scenery == null || sceneryRoot == null) return;

            foreach (var layerDef in scenery.layers)
            {
                GameObject layerObj = new GameObject(layerDef.layerName);
                layerObj.transform.SetParent(sceneryRoot, false);
                
                RectTransform rt = layerObj.AddComponent<RectTransform>();
                rt.anchorMin = layerDef.anchorMin;
                rt.anchorMax = layerDef.anchorMax;
                rt.pivot = layerDef.pivot;
                rt.anchoredPosition = layerDef.anchoredPosition;
                rt.sizeDelta = layerDef.sizeDelta;
                rt.localRotation = Quaternion.Euler(0, 0, layerDef.rotation);
                rt.localScale = layerDef.scale;

                Image img = layerObj.AddComponent<Image>();
                img.sprite = layerDef.sprite;
                
                // Sorting is done by sibling index based on layer list order
                activeLayers.Add(layerObj);

                if (mode == SceneryLoadMode.Animated)
                {
                    PlayEntryAnimation(rt, img, layerDef);
                }
                else if (mode == SceneryLoadMode.IdleOnly)
                {
                    PlayIdleAnimation(rt, img, layerDef);
                }
                // Static mode plays no animations
            }
        }

        public void UnloadScenery(Action onComplete = null)
        {
            if (activeLayers.Count == 0)
            {
                currentScenery = null;
                onComplete?.Invoke();
                return;
            }

            int layersToComplete = activeLayers.Count;
            foreach (var layerObj in activeLayers)
            {
                var rt = layerObj.GetComponent<RectTransform>();
                var img = layerObj.GetComponent<Image>();
                
                // Find layer definition to know how to reverse
                int siblingIdx = rt.GetSiblingIndex();
                SceneryLayer layerDef = null;
                if (currentScenery != null && siblingIdx < currentScenery.layers.Count)
                    layerDef = currentScenery.layers[siblingIdx];
                    
                PlayExitAnimation(rt, img, layerDef, () => {
                    Destroy(layerObj);
                    layersToComplete--;
                    if (layersToComplete <= 0)
                    {
                        activeLayers.Clear();
                        currentScenery = null;
                        onComplete?.Invoke();
                    }
                });
            }
        }

        private void PlayEntryAnimation(RectTransform rt, Image img, SceneryLayer def)
        {
            var anim = def.entryAnimation;
            if (anim.type == SceneryAnimType.None)
            {
                PlayIdleAnimation(rt, img, def);
                return;
            }

            Sequence seq = DOTween.Sequence();

            switch (anim.type)
            {
                case SceneryAnimType.Fade:
                    Color c = img.color;
                    c.a = anim.fadeFrom;
                    img.color = c;
                    seq.Append(img.DOFade(1f, anim.duration).SetEase(anim.ease));
                    break;
                case SceneryAnimType.SlideLeft:
                case SceneryAnimType.SlideRight:
                case SceneryAnimType.SlideUp:
                case SceneryAnimType.SlideDown:
                    Vector2 targetPos = rt.anchoredPosition;
                    rt.anchoredPosition = anim.moveFrom;
                    seq.Append(rt.DOAnchorPos(targetPos, anim.duration).SetEase(anim.ease));
                    break;
                case SceneryAnimType.ScaleIn:
                    Vector3 targetScale = rt.localScale;
                    rt.localScale = anim.scaleFrom;
                    seq.Append(rt.DOScale(targetScale, anim.duration).SetEase(anim.ease));
                    break;
            }

            seq.OnComplete(() => PlayIdleAnimation(rt, img, def));
        }

        private void PlayExitAnimation(RectTransform rt, Image img, SceneryLayer def, Action onComplete)
        {
            if (def == null || def.entryAnimation.type == SceneryAnimType.None)
            {
                onComplete?.Invoke();
                return;
            }

            var anim = def.entryAnimation;
            rt.DOKill();
            img.DOKill();

            Sequence seq = DOTween.Sequence();

            switch (anim.type)
            {
                case SceneryAnimType.Fade:
                    seq.Append(img.DOFade(anim.fadeFrom, anim.duration).SetEase(anim.ease));
                    break;
                case SceneryAnimType.SlideLeft:
                case SceneryAnimType.SlideRight:
                case SceneryAnimType.SlideUp:
                case SceneryAnimType.SlideDown:
                    seq.Append(rt.DOAnchorPos(anim.moveFrom, anim.duration).SetEase(anim.ease));
                    break;
                case SceneryAnimType.ScaleIn:
                    seq.Append(rt.DOScale(anim.scaleFrom, anim.duration).SetEase(anim.ease));
                    break;
            }

            seq.OnComplete(() => onComplete?.Invoke());
        }

        private void PlayIdleAnimation(RectTransform rt, Image img, SceneryLayer def)
        {
            var anim = def.idleAnimation;
            if (anim.type == SceneryAnimType.None) return;

            switch (anim.type)
            {
                case SceneryAnimType.Float:
                    rt.DOAnchorPosY(rt.anchoredPosition.y + anim.floatAmplitude, anim.floatSpeed)
                      .SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
                    break;
                case SceneryAnimType.Pulse:
                    rt.DOScale(rt.localScale * anim.pulseScale, anim.floatSpeed)
                      .SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
                    break;
                case SceneryAnimType.Sway:
                    rt.DOLocalRotate(new Vector3(0, 0, anim.swayAngle), anim.floatSpeed)
                      .SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
                    break;
                case SceneryAnimType.SlowRotate:
                    rt.DORotate(new Vector3(0, 0, 360), anim.floatSpeed, RotateMode.FastBeyond360)
                      .SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
                    break;
            }
        }
    }
}
