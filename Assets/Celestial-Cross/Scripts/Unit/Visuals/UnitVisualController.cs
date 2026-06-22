using UnityEngine;
using System.Collections.Generic;
using CelestialCross.UnitVisuals;
using CelestialCross.UI.ProceduralGraphic;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class UnitVisualController : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Unit parentUnit;
    
    private PaperCutBorderGenerator paperCutBorder;

    private static readonly int CombatTrig = Animator.StringToHash("Combat");
    private static readonly int IdleTrig = Animator.StringToHash("Idle");

    public void Init(Unit unit)
    {
        parentUnit = unit;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (unit.UnitData != null)
        {
            ApplyAnimations(unit.UnitData);
        }

        CharacterVFXManager.Instance.RegisterRenderer(unit, spriteRenderer);

        // Busca o componente de borda (pode estar neste GameObject ou num filho)
        paperCutBorder = GetComponentInChildren<PaperCutBorderGenerator>(true);
        if (paperCutBorder != null)
        {
            SetPaperCutActive(false);
        }
    }

    private void OnDestroy()
    {
        if (parentUnit != null)
        {
            CharacterVFXManager.Instance.UnregisterRenderer(parentUnit);
        }
    }

    private void ApplyAnimations(UnitData data)
    {
        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"[UnitVisualController] Nenhum AnimatorController base no prefab de {data.displayName}. As animações injetadas serão ignoradas.");
            return;
        }

        // Criamos um Override interno sem que o usuário precise configurar as burocracias
        AnimatorOverrideController ovc = new AnimatorOverrideController(animator.runtimeAnimatorController);
        
        // Mapeamos os clipes originais do BaseController com os que estão no UnitData
        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>(ovc.overridesCount);
        ovc.GetOverrides(overrides);

        for (int i = 0; i < overrides.Count; ++i)
        {
            // Note que o BaseUnitController devere ter clipes placeholder chamados "Idle" e "Combat" 
            string clipName = overrides[i].Key.name;
            if (clipName.Contains("Idle") && data.idleAnim != null)
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, data.idleAnim);
            else if (clipName.Contains("Combat") && data.combatIdleAnim != null)
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, data.combatIdleAnim);
        }

        ovc.ApplyOverrides(overrides);
        animator.runtimeAnimatorController = ovc;
    }

    /// <summary>
    /// Alterna o estado de combate. Usado quando é o turno desta unidade.
    /// </summary>
    public void SetCombatState(bool isActiveTurn)
    {
        if (isActiveTurn)
            animator.SetTrigger(CombatTrig);
        else
            animator.SetTrigger(IdleTrig);

        if (paperCutBorder != null)
        {
            SetPaperCutActive(isActiveTurn);
        }
    }

    private void SetPaperCutActive(bool active)
    {
        if (paperCutBorder == null) return;
        
        // Se a borda estiver em um GameObject separado, desativamos o objeto inteiro.
        // Se estiver no mesmo objeto (improvável, pois usa CanvasRenderer), desativamos apenas o componente.
        if (paperCutBorder.gameObject != this.gameObject)
        {
            paperCutBorder.gameObject.SetActive(active);
        }
        else
        {
            paperCutBorder.enabled = active;
            var graphic = paperCutBorder.GetComponent<UnityEngine.UI.Graphic>();
            if (graphic != null) graphic.enabled = active;
        }
    }

    /// <summary>
    /// Flipa o sprite com base no alvo clicado ou borda de spawn.
    /// Chamado externamente (Ex: TargetSelector ou BattleLevelBuilder).
    /// </summary>
    public void FaceDirection(Vector2Int targetPos)
    {
        if (parentUnit == null || spriteRenderer == null) return;

        int currentX = parentUnit.GridPosition.x;
        int targetX = targetPos.x;

        if (targetX > currentX)
        {
            spriteRenderer.flipX = false; // Olha pra direita
        }
        else if (targetX < currentX)
        {
            spriteRenderer.flipX = true; // Olha pra esquerda
        }
        // Se targetX == currentX (em cima ou embaixo), não flipa
    }

    /// <summary>
    /// Força o flip inicial (usado durante o spawn no BattleLevelBuilder)
    /// </summary>
    public void ForceFlip(bool lookLeft)
    {
        if (spriteRenderer != null)
            spriteRenderer.flipX = lookLeft;
    }
}
