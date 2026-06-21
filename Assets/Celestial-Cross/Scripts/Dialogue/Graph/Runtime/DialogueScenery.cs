using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace CelestialCross.Dialogue.Graph
{
    public enum SceneryAnimType
    {
        None, Fade, SlideLeft, SlideRight, SlideUp, SlideDown, ScaleIn,
        Float, Pulse, Sway, SlowRotate
    }

    [Serializable]
    public class SceneryAnimation
    {
        public SceneryAnimType type = SceneryAnimType.None;
        public float duration = 0.5f;
        public Ease ease = Ease.OutQuad;
        
        // Parameters
        public Vector2 moveFrom;
        public float fadeFrom = 0f;
        public Vector3 scaleFrom = Vector3.zero;
        public float floatAmplitude = 10f;
        public float floatSpeed = 1f;
        public float swayAngle = 5f;
        public float pulseScale = 1.05f;
    }

    [Serializable]
    public class SceneryLayer
    {
        public string layerName = "New Layer";
        public Sprite sprite;
        public int sortingOrder = 0;
        
        // Transform definition
        public Vector2 anchorMin = Vector2.zero;
        public Vector2 anchorMax = Vector2.one;
        public Vector2 pivot = new Vector2(0.5f, 0.5f);
        public Vector2 anchoredPosition = Vector2.zero;
        public Vector2 sizeDelta = Vector2.zero;
        public float rotation = 0f;
        public Vector3 scale = Vector3.one;
        
        public SceneryAnimation entryAnimation = new SceneryAnimation();
        public SceneryAnimation idleAnimation = new SceneryAnimation();
    }

    [CreateAssetMenu(fileName = "NewDialogueScenery", menuName = "Celestial Cross/Dialogue/Scenery")]
    public class DialogueScenery : ScriptableObject
    {
        public string sceneryName;
        public List<SceneryLayer> layers = new List<SceneryLayer>();
    }
}
