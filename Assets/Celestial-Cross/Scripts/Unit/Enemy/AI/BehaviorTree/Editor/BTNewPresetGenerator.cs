#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Editor
{
    public static class BTNewPresetGenerator
    {
        [MenuItem("Celestial Cross/AI/Generate 3 Custom AIs")]
        public static void GenerateCustomPresets()
        {
            string folder = "Assets/Celestial-Cross/Data/AI";
            if (!AssetDatabase.IsValidFolder(folder))
            {
                System.IO.Directory.CreateDirectory(Application.dataPath + "/Celestial-Cross/Data/AI");
                AssetDatabase.Refresh();
            }

            string presetsFolder = "Assets/Celestial-Cross/Data/AI/Presets";
            if (!AssetDatabase.IsValidFolder(presetsFolder))
            {
                System.IO.Directory.CreateDirectory(Application.dataPath + "/Celestial-Cross/Data/AI/Presets");
                AssetDatabase.Refresh();
            }

            CreateWarrior(presetsFolder);
            CreateTacticalAssassin(presetsFolder);
            CreateBossPhased(presetsFolder);

            AssetDatabase.SaveAssets();
            Debug.Log("[BTNewPresetGenerator] 3 custom presets de IA gerados com sucesso em: " + presetsFolder);
        }

        private static BehaviorTreeSO GetOrCreateSO(string folder, string name, string treeName, string description)
        {
            string path = $"{folder}/{name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<BehaviorTreeSO>(path);
            BehaviorTreeSO so = existing != null ? existing : ScriptableObject.CreateInstance<BehaviorTreeSO>();
            so.treeName = treeName;
            so.description = description;
            so.NodeData = new List<BTNodeData>();
            so.NodeLinks = new List<BTLinkData>();
            
            if (existing == null)
            {
                AssetDatabase.CreateAsset(so, path);
            }
            return so;
        }

        private static string GetCompJson(params string[] ports)
        {
            var data = new Runtime.BTCompositeData();
            foreach (var p in ports) data.ports.Add(p);
            return JsonUtility.ToJson(data);
        }

        private static void CreateWarrior(string folder)
        {
            var so = GetOrCreateSO(folder, "Preset_Warrior", "Warrior", "Anda até o inimigo mais próximo e depois ataca.");

            so.NodeData.AddRange(new[]
            {
                new BTNodeData { Guid = "root", NodeTitle = "Root", NodeType = "BTRootEditorNode", Position = new Vector2(100, 300) },
                new BTNodeData { Guid = "selector", NodeTitle = "Selector", NodeType = "BTSelectorEditorNode", Position = new Vector2(300, 300), JsonData = GetCompJson("Passo_0", "Passo_1") },
                
                // Attack branch
                new BTNodeData { Guid = "seq_attack", NodeTitle = "Sequence Attack", NodeType = "BTSequenceEditorNode", Position = new Vector2(500, 150), JsonData = GetCompJson("Passo_0", "Passo_1") },
                new BTNodeData { Guid = "cond_range", NodeTitle = "Target In Range", NodeType = "BTConditionTargetInRangeEditorNode", Position = new Vector2(750, 50) },
                new BTNodeData { Guid = "act_attack", NodeTitle = "Cast Damage Skill", NodeType = "BTActionUseAbilityEditorNode", Position = new Vector2(750, 200), JsonData = "{\"category\":0}" },
                new BTNodeData { Guid = "get_target", NodeTitle = "Get Closest Enemy", NodeType = "BTGetTargetEditorNode", Position = new Vector2(1050, 200), JsonData = "{\"faction\":0,\"strategy\":0}" },

                // Move branch
                new BTNodeData { Guid = "act_move", NodeTitle = "Move to Target", NodeType = "BTActionMoveEditorNode", Position = new Vector2(500, 450), JsonData = "{\"intent\":0}" }
            });

            so.NodeLinks.AddRange(new[]
            {
                new BTLinkData { ParentGuid = "root", ParentPort = "Child", ChildGuid = "selector", ChildPort = "Parent" },
                
                new BTLinkData { ParentGuid = "selector", ParentPort = "Passo_0", ChildGuid = "seq_attack", ChildPort = "Parent" },
                new BTLinkData { ParentGuid = "selector", ParentPort = "Passo_1", ChildGuid = "act_move", ChildPort = "Parent" },

                new BTLinkData { ParentGuid = "seq_attack", ParentPort = "Passo_0", ChildGuid = "cond_range", ChildPort = "Parent" },
                new BTLinkData { ParentGuid = "seq_attack", ParentPort = "Passo_1", ChildGuid = "act_attack", ChildPort = "Parent" },

                new BTLinkData { ParentGuid = "get_target", ParentPort = "Target", ChildGuid = "cond_range", ChildPort = "Target" },
                new BTLinkData { ParentGuid = "get_target", ParentPort = "Target", ChildGuid = "act_attack", ChildPort = "Target" },
                new BTLinkData { ParentGuid = "get_target", ParentPort = "Target", ChildGuid = "act_move", ChildPort = "Target" }
            });

            EditorUtility.SetDirty(so);
        }

        private static void CreateTacticalAssassin(string folder)
        {
            var so = GetOrCreateSO(folder, "Preset_TacticalAssassin", "Tactical Assassin", "Foca exclusivamente no inimigo com menor HP. Usa habilidades de dano e flanqueamento.");

            so.NodeData.AddRange(new[]
            {
                new BTNodeData { Guid = "root", NodeTitle = "Root", NodeType = "BTRootEditorNode", Position = new Vector2(100, 300) },
                new BTNodeData { Guid = "selector", NodeTitle = "Selector", NodeType = "BTSelectorEditorNode", Position = new Vector2(300, 300), JsonData = GetCompJson("Passo_0", "Passo_1") },

                // Attack branch
                new BTNodeData { Guid = "seq_assassinate", NodeTitle = "Sequence Assassinate", NodeType = "BTSequenceEditorNode", Position = new Vector2(550, 150), JsonData = GetCompJson("Passo_0", "Passo_1") },
                new BTNodeData { Guid = "cond_atk_ready", NodeTitle = "Damage Ready", NodeType = "BTConditionAbilityReadyEditorNode", Position = new Vector2(800, 50), JsonData = "{\"category\":0}" },
                new BTNodeData { Guid = "act_strike", NodeTitle = "Strike Target", NodeType = "BTActionUseAbilityEditorNode", Position = new Vector2(800, 200), JsonData = "{\"category\":0}" },

                // Flank branch
                new BTNodeData { Guid = "act_flank", NodeTitle = "Flank Target", NodeType = "BTActionMoveEditorNode", Position = new Vector2(550, 450), JsonData = "{\"intent\":2}" },

                // Weakest Enemy Provider
                new BTNodeData { Guid = "get_weakest_enemy", NodeTitle = "Get Weakest Enemy", NodeType = "BTGetTargetEditorNode", Position = new Vector2(1100, 300), JsonData = "{\"faction\":0,\"strategy\":2}" }
            });

            so.NodeLinks.AddRange(new[]
            {
                new BTLinkData { ParentGuid = "root", ParentPort = "Child", ChildGuid = "selector", ChildPort = "Parent" },

                new BTLinkData { ParentGuid = "selector", ParentPort = "Passo_0", ChildGuid = "seq_assassinate", ChildPort = "Parent" },
                new BTLinkData { ParentGuid = "selector", ParentPort = "Passo_1", ChildGuid = "act_flank", ChildPort = "Parent" },

                new BTLinkData { ParentGuid = "seq_assassinate", ParentPort = "Passo_0", ChildGuid = "cond_atk_ready", ChildPort = "Parent" },
                new BTLinkData { ParentGuid = "seq_assassinate", ParentPort = "Passo_1", ChildGuid = "act_strike", ChildPort = "Parent" },

                new BTLinkData { ParentGuid = "get_weakest_enemy", ParentPort = "Target", ChildGuid = "act_strike", ChildPort = "Target" },
                new BTLinkData { ParentGuid = "get_weakest_enemy", ParentPort = "Target", ChildGuid = "act_flank", ChildPort = "Target" }
            });

            EditorUtility.SetDirty(so);
        }

        private static void CreateBossPhased(string folder)
        {
            var so = GetOrCreateSO(folder, "Preset_BossPhased", "Phased Boss", "Chefe complexo de 2 fases. Fase 1: Foca em ataques de área. Fase 2 (HP < 40%): Ganha escudo, se cura e ataca intensamente.");

            so.NodeData.AddRange(new[]
            {
                new BTNodeData { Guid = "root", NodeTitle = "Root", NodeType = "BTRootEditorNode", Position = new Vector2(100, 300) },
                
                // Value Switch on HP
                new BTNodeData { Guid = "val_switch", NodeTitle = "Value Switch", NodeType = "BTValueSwitchEditorNode", Position = new Vector2(300, 300), JsonData = "{\"cases\":[{\"portName\":\"Fase2\",\"operatorType\":1,\"threshold\":40.0},{\"portName\":\"Fase1\",\"operatorType\":4,\"threshold\":40.0}]}" },
                new BTNodeData { Guid = "get_hp_boss", NodeTitle = "Get Numeric Data", NodeType = "BTGetNumericDataEditorNode", Position = new Vector2(300, 500), JsonData = "{\"dataType\":0}" }, // SelfHPPercent

                // --- PHASE 1 ---
                new BTNodeData { Guid = "sel_phase1", NodeTitle = "Selector Phase 1", NodeType = "BTSelectorEditorNode", Position = new Vector2(550, 100), JsonData = GetCompJson("Passo_0", "Passo_1", "Passo_2") },
                
                // AoE Sequence (every 2 turns if hits 2+)
                new BTNodeData { Guid = "seq_aoe", NodeTitle = "Sequence AoE", NodeType = "BTSequenceEditorNode", Position = new Vector2(800, -20), JsonData = GetCompJson("Passo_0", "Passo_1", "Passo_2") },
                new BTNodeData { Guid = "check_turn", NodeTitle = "Check Value", NodeType = "BTCheckValueEditorNode", Position = new Vector2(1050, -100), JsonData = "{\"operatorType\":5,\"threshold\":2.0}" },
                new BTNodeData { Guid = "get_turn", NodeTitle = "Get Numeric Data", NodeType = "BTGetNumericDataEditorNode", Position = new Vector2(1300, -100), JsonData = "{\"dataType\":4}" }, // TurnNumber
                new BTNodeData { Guid = "cond_aoe_hits", NodeTitle = "AoE Hit Count >= 2", NodeType = "BTConditionAoEHitCountEditorNode", Position = new Vector2(1050, 20), JsonData = "{\"minHitCount\":2}" },
                new BTNodeData { Guid = "act_aoe", NodeTitle = "Cast AoE Damage", NodeType = "BTActionUseAbilityEditorNode", Position = new Vector2(1050, 140), JsonData = "{\"category\":0}" },

                // Single Target Sequence
                new BTNodeData { Guid = "act_atk_weak", NodeTitle = "Attack Weakest", NodeType = "BTActionUseAbilityEditorNode", Position = new Vector2(800, 200), JsonData = "{\"category\":0}" },
                // Move (Approach)
                new BTNodeData { Guid = "act_move_p1", NodeTitle = "Approach Target", NodeType = "BTActionMoveEditorNode", Position = new Vector2(800, 320), JsonData = "{\"intent\":0}" },


                // --- PHASE 2 ---
                new BTNodeData { Guid = "sel_phase2", NodeTitle = "Selector Phase 2", NodeType = "BTSelectorEditorNode", Position = new Vector2(550, 600), JsonData = GetCompJson("Passo_0", "Passo_1", "Passo_2") },

                // Shield Sequence (if no UltimateShield buff)
                new BTNodeData { Guid = "seq_shield", NodeTitle = "Sequence Buff Shield", NodeType = "BTSequenceEditorNode", Position = new Vector2(800, 450), JsonData = GetCompJson("Passo_0", "Passo_1") },
                new BTNodeData { Guid = "cond_no_shield", NodeTitle = "No UltimateShield Buff", NodeType = "BTConditionTargetHasBuffEditorNode", Position = new Vector2(1050, 380), JsonData = "{\"isBuff\":true,\"modifierId\":\"UltimateShield\"}" },
                new BTNodeData { Guid = "act_shield", NodeTitle = "Cast Shield Buff", NodeType = "BTActionUseAbilityEditorNode", Position = new Vector2(1050, 490), JsonData = "{\"category\":2}" },

                // Panic Heal Sequence (HP < 20%)
                new BTNodeData { Guid = "seq_panic_heal", NodeTitle = "Sequence Panic Heal", NodeType = "BTSequenceEditorNode", Position = new Vector2(800, 620), JsonData = GetCompJson("Passo_0", "Passo_1") },
                new BTNodeData { Guid = "check_panic", NodeTitle = "Check Value", NodeType = "BTCheckValueEditorNode", Position = new Vector2(1050, 580), JsonData = "{\"operatorType\":1,\"threshold\":20.0}" },
                new BTNodeData { Guid = "get_hp_panic", NodeTitle = "Get Numeric Data", NodeType = "BTGetNumericDataEditorNode", Position = new Vector2(1300, 580), JsonData = "{\"dataType\":0}" }, // SelfHPPercent
                new BTNodeData { Guid = "act_heal_self", NodeTitle = "Cast Self Heal", NodeType = "BTActionUseAbilityEditorNode", Position = new Vector2(1050, 680), JsonData = "{\"category\":1}" },

                // Berserk Attack
                new BTNodeData { Guid = "act_berserk_atk", NodeTitle = "Berserk Attack", NodeType = "BTActionUseAbilityEditorNode", Position = new Vector2(800, 780), JsonData = "{\"category\":0}" },

                // Target Providers
                new BTNodeData { Guid = "get_self", NodeTitle = "Get Self", NodeType = "BTGetTargetEditorNode", Position = new Vector2(1350, 520), JsonData = "{\"faction\":2,\"strategy\":0}" },
                new BTNodeData { Guid = "get_weakest", NodeTitle = "Get Weakest Enemy", NodeType = "BTGetTargetEditorNode", Position = new Vector2(1350, 200), JsonData = "{\"faction\":0,\"strategy\":2}" }
            });

            so.NodeLinks.AddRange(new[]
            {
                new BTLinkData { ParentGuid = "root", ParentPort = "Child", ChildGuid = "val_switch", ChildPort = "Parent" },
                new BTLinkData { ParentGuid = "get_hp_boss", ParentPort = "Value", ChildGuid = "val_switch", ChildPort = "Value" },

                // Switch Cases Links
                new BTLinkData { ParentGuid = "val_switch", ParentPort = "Fase1", ChildGuid = "sel_phase1", ChildPort = "Parent" },
                new BTLinkData { ParentGuid = "val_switch", ParentPort = "Fase2", ChildGuid = "sel_phase2", ChildPort = "Parent" },

                // Phase 1 Children
                new BTLinkData { ParentGuid = "sel_phase1", ParentPort = "Passo_0", ChildGuid = "seq_aoe", ChildPort = "Parent" },
                new BTLinkData { ParentGuid = "sel_phase1", ParentPort = "Passo_1", ChildGuid = "act_atk_weak", ChildPort = "Parent" },
                new BTLinkData { ParentGuid = "sel_phase1", ParentPort = "Passo_2", ChildGuid = "act_move_p1", ChildPort = "Parent" },

                // Seq AoE Children
                new BTLinkData { ParentGuid = "seq_aoe", ParentPort = "Passo_0", ChildGuid = "check_turn", ChildPort = "Parent" },
                new BTLinkData { ParentGuid = "get_turn", ParentPort = "Value", ChildGuid = "check_turn", ChildPort = "Value" },
                new BTLinkData { ParentGuid = "seq_aoe", ParentPort = "Passo_1", ChildGuid = "cond_aoe_hits", ChildPort = "Parent" },
                new BTLinkData { ParentGuid = "seq_aoe", ParentPort = "Passo_2", ChildGuid = "act_aoe", ChildPort = "Parent" },

                // Phase 2 Children
                new BTLinkData { ParentGuid = "sel_phase2", ParentPort = "Passo_0", ChildGuid = "seq_shield", ChildPort = "Parent" },
                new BTLinkData { ParentGuid = "sel_phase2", ParentPort = "Passo_1", ChildGuid = "seq_panic_heal", ChildPort = "Parent" },
                new BTLinkData { ParentGuid = "sel_phase2", ParentPort = "Passo_2", ChildGuid = "act_berserk_atk", ChildPort = "Parent" },

                // Seq Shield Children
                new BTLinkData { ParentGuid = "seq_shield", ParentPort = "Passo_0", ChildGuid = "cond_no_shield", ChildPort = "Parent" },
                new BTLinkData { ParentGuid = "seq_shield", ParentPort = "Passo_1", ChildGuid = "act_shield", ChildPort = "Parent" },

                // Seq Panic Heal Children
                new BTLinkData { ParentGuid = "seq_panic_heal", ParentPort = "Passo_0", ChildGuid = "check_panic", ChildPort = "Parent" },
                new BTLinkData { ParentGuid = "get_hp_panic", ParentPort = "Value", ChildGuid = "check_panic", ChildPort = "Value" },
                new BTLinkData { ParentGuid = "seq_panic_heal", ParentPort = "Passo_1", ChildGuid = "act_heal_self", ChildPort = "Parent" },

                // Data Connections
                new BTLinkData { ParentGuid = "get_weakest", ParentPort = "Target", ChildGuid = "act_aoe", ChildPort = "Target" },
                new BTLinkData { ParentGuid = "get_weakest", ParentPort = "Target", ChildGuid = "act_atk_weak", ChildPort = "Target" },
                new BTLinkData { ParentGuid = "get_weakest", ParentPort = "Target", ChildGuid = "act_move_p1", ChildPort = "Target" },
                new BTLinkData { ParentGuid = "get_weakest", ParentPort = "Target", ChildGuid = "act_berserk_atk", ChildPort = "Target" },

                new BTLinkData { ParentGuid = "get_self", ParentPort = "Target", ChildGuid = "act_shield", ChildPort = "Target" },
                new BTLinkData { ParentGuid = "get_self", ParentPort = "Target", ChildGuid = "act_heal_self", ChildPort = "Target" }
            });

            EditorUtility.SetDirty(so);
        }
    }
}
#endif
