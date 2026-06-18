using UnityEngine;
using DG.Tweening;

namespace CelestialCross.VFX
{
    public class CometDeathVFX : MonoBehaviour
    {
        [Header("References")]
        public SpriteRenderer headSprite;
        public TrailRenderer trail;

        [Header("Animation Settings")]
        public float flyDuration = 0.5f; 
        public float flyDistance = 30f; // Distância que o cometa percorre no chão (X e Z)

        public void Play(Color color, Vector3 startPos)
        {
            transform.position = startPos;
            
            if (headSprite != null) headSprite.color = color;
            
            if (trail != null)
            {
                trail.startColor = color;
                trail.endColor = new Color(color.r, color.g, color.b, 0f);
            }

            // Pega uma direção aleatória em um círculo (X e Z)
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            
            // O cometa atira para longe nessa direção, mantendo o Y ligeiramente acima do chão
            Vector3 targetPos = startPos + new Vector3(randomDir.x * flyDistance, 1f, randomDir.y * flyDistance);

            Sequence seq = DOTween.Sequence();

            // Salto parabólico ou direto para cima
            seq.Join(transform.DOMove(targetPos, flyDuration).SetEase(Ease.InExpo));
            
            // Vai sumindo enquanto voa
            if (headSprite != null)
            {
                seq.Join(headSprite.DOFade(0f, flyDuration).SetEase(Ease.InQuad));
            }

            seq.OnComplete(() =>
            {
                Destroy(gameObject);
            });
        }
    }
}
