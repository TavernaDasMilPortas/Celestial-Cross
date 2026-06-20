using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace CelestialCross.UI.ProceduralGraphic
{
    /// <summary>
    /// Componente helper para animar automaticamente um ProceduralGraphic
    /// alternando entre seus keyframes via DOTween.
    /// </summary>
    [RequireComponent(typeof(ProceduralGraphic))]
    public class ProceduralGraphicAnimator : MonoBehaviour
    {
        [Title("Animator Settings")]
        [Tooltip("Toca a animação automaticamente ao ativar o objeto.")]
        public bool playOnAwake = true;
        
        public enum AnimationMode 
        { 
            [InspectorName("Timeline (Loop pelo Time)")] LoopAllKeyframes,
            [InspectorName("Ping-Pong (Entre 2 Keyframes)")] PingPong 
        }
        
        [EnumToggleButtons]
        public AnimationMode mode = AnimationMode.LoopAllKeyframes;

        [ShowIf("mode", AnimationMode.PingPong)]
        [ValueDropdown("GetKeyframeNames")]
        [LabelText("Keyframe Inicial (A)")]
        public string keyframeA;

        [ShowIf("mode", AnimationMode.PingPong)]
        [ValueDropdown("GetKeyframeNames")]
        [LabelText("Keyframe Final (B)")]
        public string keyframeB;

        [ShowIf("mode", AnimationMode.PingPong)]
        [LabelText("Duração da ida (seg)")]
        public float duration = 0.5f;

        [ShowIf("mode", AnimationMode.LoopAllKeyframes)]
        [Tooltip("Duração para ir do tempo 0 até o último tempo configurado nos keyframes.")]
        [LabelText("Duração total do ciclo (seg)")]
        public float totalLoopDuration = 1f;

        private ProceduralGraphic _graphic;
        private Tweener _currentTween;
        private Sequence _pingPongSequence;

        private void Awake()
        {
            _graphic = GetComponent<ProceduralGraphic>();
        }

        private void OnEnable()
        {
            if (playOnAwake)
            {
                // Aguarda o fim do frame para garantir que o ProceduralGraphic chamou seu próprio Awake
                DOVirtual.DelayedCall(0f, () => {
                    if (this != null && gameObject.activeInHierarchy) Play();
                });
            }
        }

        private void OnDisable()
        {
            Stop();
        }

        private IEnumerable<string> GetKeyframeNames()
        {
            var graphic = GetComponent<ProceduralGraphic>();
            if (graphic != null && graphic.Preset != null && graphic.Preset.Keyframes != null)
            {
                foreach(var k in graphic.Preset.Keyframes) yield return k.name;
            }
        }

        [Button("▶ Play Anim", ButtonSizes.Medium), HideInEditorMode]
        public void Play()
        {
            Stop();
            if (_graphic.Preset == null || _graphic.Preset.Keyframes.Count == 0) return;

            if (mode == AnimationMode.PingPong)
            {
                if (string.IsNullOrEmpty(keyframeA) || string.IsNullOrEmpty(keyframeB))
                {
                    Debug.LogWarning("[ProceduralGraphicAnimator] Configure os Keyframes A e B para o PingPong.");
                    return;
                }

                _pingPongSequence = DOTween.Sequence();
                
                // Primeiro vai pro A super rápido para garantir o estado inicial
                _pingPongSequence.Append(_graphic.DOTransition(keyframeA, 0.05f));
                
                // Vai pro B
                _pingPongSequence.AppendCallback(() => {
                    _currentTween = _graphic.DOTransition(keyframeB, duration).SetEase(Ease.InOutSine);
                });
                _pingPongSequence.AppendInterval(duration);
                
                // Volta pro A
                _pingPongSequence.AppendCallback(() => {
                    _currentTween = _graphic.DOTransition(keyframeA, duration).SetEase(Ease.InOutSine);
                });
                _pingPongSequence.AppendInterval(duration);
                
                _pingPongSequence.SetLoops(-1);
            }
            else if (mode == AnimationMode.LoopAllKeyframes)
            {
                float maxTime = _graphic.Preset.Keyframes[_graphic.Preset.Keyframes.Count - 1].time;
                if (maxTime <= 0) maxTime = 1f;

                _currentTween = _graphic.DOBlendTimeline(maxTime, totalLoopDuration)
                                        .SetEase(Ease.InOutSine)
                                        .SetLoops(-1, LoopType.Yoyo);
            }
        }

        [Button("⏹ Stop", ButtonSizes.Medium), HideInEditorMode]
        public void Stop()
        {
            if (_currentTween != null)
            {
                _currentTween.Kill();
                _currentTween = null;
            }
            if (_pingPongSequence != null)
            {
                _pingPongSequence.Kill();
                _pingPongSequence = null;
            }
        }
        
        [Button("⏹ Stop & Reset", ButtonSizes.Medium), HideInEditorMode]
        public void StopAndReset()
        {
            Stop();
            
            // Tenta voltar ao primeiro keyframe como pose base
            if (_graphic.Preset != null && _graphic.Preset.Keyframes.Count > 0)
            {
                _graphic.DOTransition(_graphic.Preset.Keyframes[0].name, 0.3f);
            }
        }
    }
}
