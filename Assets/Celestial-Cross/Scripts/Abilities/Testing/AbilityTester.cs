using UnityEngine;
using Sirenix.OdinInspector;
using Celestial_Cross.Scripts.Abilities;
using Celestial_Cross.Scripts.Combat.Execution;
using CelestialCross.Combat;

namespace Celestial_Cross.Scripts.Abilities.Testing
{
    public class AbilityTester : MonoBehaviour
    {
        [Tooltip("A unidade que irá conjurar a habilidade.")]
        public Unit Caster;

        [Tooltip("A habilidade a ser testada.")]
        [InlineEditor(InlineEditorObjectFieldModes.Boxed)]
        public AbilityBlueprint BlueprintToTest;

        [Tooltip("Qual momento/hook disparar agora?")]
        public CombatHook testHook = CombatHook.OnManualCast;

        [Button("Executar Habilidade (Teste)", ButtonSizes.Large)]
        [GUIColor(0.2f, 0.8f, 0.2f)]
        public void TestAbility()
        {
            if (Caster == null)
            {
                Caster = GetComponent<Unit>();
            }

            if (Caster == null)
            {
                Debug.LogError("[AbilityTester] Nenhuma Unidade Conjuradora (Caster) definida ou encontrada neste Game Object!");
                return;
            }

            if (BlueprintToTest == null)
            {
                Debug.LogError("[AbilityTester] Nenhuma AbilityBlueprint definida para o teste!");
                return;
            }

            if (AbilityExecutor.Instance == null)
            {
                var executorObj = new GameObject("AbilityExecutor");
                executorObj.AddComponent<AbilityExecutor>();
                Debug.LogWarning("[AbilityTester] AbilityExecutor não foi encontrado na cena. Criado um temporário.");
            }

            Debug.Log($"[AbilityTester] Solicitando execução de {BlueprintToTest.name} para o momento {testHook}...");
            AbilityExecutor.Instance.ExecuteAbility(Caster, BlueprintToTest, testHook, () =>
            {
                Debug.Log($"[AbilityTester] Teste da habilidade {BlueprintToTest.name} completamente finalizado!");
            });
        }
    }
}
