using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using CelestialCross.UnitVisuals;

namespace CelestialCross.Pets.Editor
{
    public class PetUniversalPrefabGenerator
    {
        [MenuItem("Celestial Cross/Pets/Generate Universal Base Pet Prefab")]
        public static void GenerateUniversalPetPrefab()
        {
            string basePath = "Assets/Celestial-Cross";
            string resourcesPath = basePath + "/Resources";
            
            // Garante que a pasta Resources existe
            if (!AssetDatabase.IsValidFolder(basePath))
            {
                AssetDatabase.CreateFolder("Assets", "Celestial-Cross");
            }
            if (!AssetDatabase.IsValidFolder(resourcesPath))
            {
                AssetDatabase.CreateFolder(basePath, "Resources");
            }

            // 1. Cria as Animações "Dummy" (apenas para servir de ponteiro no override)
            AnimationClip idleClip = CreateOrGetClip(resourcesPath, "BasePet_Idle");
            AnimationClip skillClip = CreateOrGetClip(resourcesPath, "BasePet_Skill");

            // 2. Cria o Animator Controller
            string controllerPath = $"{resourcesPath}/BasePetAnimator.controller";
            
            // Deleta para forçar a re-criação limpa com as novas regras
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null)
            {
                AssetDatabase.DeleteAsset(controllerPath);
            }
            
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            
            // Adiciona Trigger Parameters
            controller.AddParameter("Idle", AnimatorControllerParameterType.Trigger);
                controller.AddParameter("Skill", AnimatorControllerParameterType.Trigger);

                // Mapeia máquina de estados
                AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;

                // Adiciona Estados
                AnimatorState idleState = rootStateMachine.AddState("BasePet_Idle");
                idleState.motion = idleClip;

                AnimatorState skillState = rootStateMachine.AddState("BasePet_Skill");
                skillState.motion = skillClip;

                rootStateMachine.defaultState = idleState;

                // Transições (AnyState -> Trigger)
                var anyToIdle = rootStateMachine.AddAnyStateTransition(idleState);
                anyToIdle.AddCondition(AnimatorConditionMode.If, 0, "Idle");
                anyToIdle.canTransitionToSelf = false;
                
                var anyToSkill = rootStateMachine.AddAnyStateTransition(skillState);
                anyToSkill.AddCondition(AnimatorConditionMode.If, 0, "Skill");
                anyToSkill.canTransitionToSelf = false;

                // Retorna ao Idle após acabar a skill (hasExitTime)
                var skillToIdle = skillState.AddTransition(idleState);
                skillToIdle.hasExitTime = true;
                skillToIdle.exitTime = 1f; // Volta no fim exato da animação
                skillToIdle.duration = 0f; // Sem transição de fade (2D)
            

            // 3. Monta o GameObject
            GameObject petObj = new GameObject("BasePetPrefab");
            
            var sr = petObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 5; // Boa prática já vir com render order pra não sumir atrás do mapa
            
            var anim = petObj.AddComponent<Animator>();
            anim.runtimeAnimatorController = controller;

            petObj.AddComponent<PetVisualController>();

            // 4. Salva como Prefab na pasta Resources
            string prefabPath = $"{resourcesPath}/BasePetPrefab.prefab";
            PrefabUtility.SaveAsPrefabAsset(petObj, prefabPath);

            // Deleta o lixo da cena
            Object.DestroyImmediate(petObj);

            // Refresca o arquivo do painel
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=cyan>[Sucesso]</color> Universal Base Pet Prefab gerado em: {resourcesPath}");
        }

        private static AnimationClip CreateOrGetClip(string path, string clipName)
        {
            string fullPath = $"{path}/{clipName}.anim";
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(fullPath);
            if (clip == null)
            {
                clip = new AnimationClip { name = clipName };
                AssetDatabase.CreateAsset(clip, fullPath);
            }
            return clip;
        }
    }
}
