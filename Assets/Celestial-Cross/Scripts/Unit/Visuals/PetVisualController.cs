using UnityEngine;

namespace CelestialCross.UnitVisuals
{
    [RequireComponent(typeof(Animator))]
    public class PetVisualController : MonoBehaviour
    {
        private Animator animator;
        private static readonly int IdleHash = Animator.StringToHash("Idle");
        private static readonly int SkillHash = Animator.StringToHash("Skill");

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void PlayIdle()
        {
            if (animator != null)
                animator.SetTrigger(IdleHash);
        }

        public void PlaySkill()
        {
            if (animator != null)
                animator.SetTrigger(SkillHash);
        }

        /// <summary>
        /// Configura a posição inicial relativa ao dono.
        /// </summary>
        public void Setup(Transform owner, Vector3 offset, Vector3 scale)
        {
            transform.SetParent(owner);
            transform.localPosition = offset;
            transform.localScale = scale;
            transform.localRotation = Quaternion.identity;
        }
        
        public void FaceDirection(bool lookLeft)
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.flipX = lookLeft;
        }
    }
}
