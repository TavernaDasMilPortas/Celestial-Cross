using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Text;
using System.Collections.Generic;
using CelestialCross.Giulia_UI;
using CelestialCross.UI.Skills;

namespace CelestialCross.EditorArea
{
    public class InventorySceneInspector : EditorWindow
    {
        [MenuItem("Celestial Cross/3. UI Builders/4. Utilities/Inspect Active Scene Inventory")]
        public static void InspectInventoryScene()
        {
            var now = global::System.DateTime.Now;
            var sb = new StringBuilder();
            sb.AppendLine("==========================================================================");
            sb.AppendLine("INVENTORY & ABILITIES SCENE INSPECTION REPORT");
            sb.AppendLine($"Generated on: {now}");
            sb.AppendLine("==========================================================================");
            sb.AppendLine();

            // Find InventoryUI
            var inventory = Object.FindObjectOfType<InventoryUI>();
            if (inventory == null)
            {
                sb.AppendLine("WARNING: InventoryUI component not found in the active scene!");
            }
            else
            {
                sb.AppendLine(">>> INVENTORY UI COMPONENT REFERENCES <<<");
                sb.AppendLine($"GameObject Name: {inventory.name}");
                sb.AppendLine($"Path: {GetHierarchyPath(inventory.transform)}");
                sb.AppendLine();
                sb.AppendLine($"unitIconImage: {GetRefName(inventory.unitIconImage)}");
                sb.AppendLine($"unitStatsText: {GetRefName(inventory.unitStatsText)}");
                sb.AppendLine($"unitAbilitiesText: {GetRefName(inventory.unitAbilitiesText)}");
                sb.AppendLine($"unitAbilitiesContainer: {GetRefName(inventory.unitAbilitiesContainer)}");
                sb.AppendLine($"unitEquipContainer: {GetRefName(inventory.unitEquipContainer)}");
                
                sb.AppendLine("unitEquipButtons:");
                if (inventory.unitEquipButtons != null)
                {
                    for (int i = 0; i < inventory.unitEquipButtons.Length; i++)
                    {
                        sb.AppendLine($"  [{i}]: {GetRefName(inventory.unitEquipButtons[i])}");
                    }
                }
                else
                {
                    sb.AppendLine("  (null)");
                }

                sb.AppendLine("unitEquipTexts:");
                if (inventory.unitEquipTexts != null)
                {
                    for (int i = 0; i < inventory.unitEquipTexts.Length; i++)
                    {
                        sb.AppendLine($"  [{i}]: {GetRefName(inventory.unitEquipTexts[i])}");
                    }
                }
                else
                {
                    sb.AppendLine("  (null)");
                }

                sb.AppendLine($"equipArtifactButton: {GetRefName(inventory.equipArtifactButton)}");
                sb.AppendLine($"equipArtifactText: {GetRefName(inventory.equipArtifactText)}");
                sb.AppendLine($"cancelEquipButton: {GetRefName(inventory.cancelEquipButton)}");
                sb.AppendLine($"unequipArtifactButton: {GetRefName(inventory.unequipArtifactButton)}");
                sb.AppendLine($"manageArtifactButton: {GetRefName(inventory.manageArtifactButton)}");
                sb.AppendLine($"upgradeModal: {GetRefName(inventory.upgradeModal)}");
                sb.AppendLine($"managePetButton: {GetRefName(inventory.managePetButton)}");
                sb.AppendLine($"petManageModal: {GetRefName(inventory.petManageModal)}");
                
                sb.AppendLine($"unitLevelText: {GetRefName(inventory.unitLevelText)}");
                sb.AppendLine($"unitXPBar: {GetRefName(inventory.unitXPBar)}");
                sb.AppendLine($"unitXPText: {GetRefName(inventory.unitXPText)}");
                
                sb.AppendLine($"constellationModal: {GetRefName(inventory.constellationModal)}");
                sb.AppendLine("constellationStars:");
                if (inventory.constellationStars != null)
                {
                    for (int i = 0; i < inventory.constellationStars.Length; i++)
                    {
                        sb.AppendLine($"  [{i}]: {GetRefName(inventory.constellationStars[i])}");
                    }
                }
                else
                {
                    sb.AppendLine("  (null)");
                }
                sb.AppendLine($"constellationButton: {GetRefName(inventory.constellationButton)}");
                sb.AppendLine($"insigniaCountText: {GetRefName(inventory.insigniaCountText)}");

                sb.AppendLine("tabs:");
                if (inventory.tabs != null)
                {
                    for (int i = 0; i < inventory.tabs.Length; i++)
                    {
                        sb.AppendLine($"  [{i}]: {GetRefName(inventory.tabs[i])}");
                    }
                }
                else
                {
                    sb.AppendLine("  (null)");
                }

                sb.AppendLine("gridContainers:");
                if (inventory.gridContainers != null)
                {
                    for (int i = 0; i < inventory.gridContainers.Length; i++)
                    {
                        sb.AppendLine($"  [{i}]: {GetRefName(inventory.gridContainers[i])}");
                    }
                }
                else
                {
                    sb.AppendLine("  (null)");
                }

                sb.AppendLine("bottomScrollRoots:");
                if (inventory.bottomScrollRoots != null)
                {
                    for (int i = 0; i < inventory.bottomScrollRoots.Length; i++)
                    {
                        sb.AppendLine($"  [{i}]: {GetRefName(inventory.bottomScrollRoots[i])}");
                    }
                }
                else
                {
                    sb.AppendLine("  (null)");
                }

                sb.AppendLine("topPanels:");
                if (inventory.topPanels != null)
                {
                    for (int i = 0; i < inventory.topPanels.Length; i++)
                    {
                        sb.AppendLine($"  [{i}]: {GetRefName(inventory.topPanels[i])}");
                    }
                }
                else
                {
                    sb.AppendLine("  (null)");
                }

                sb.AppendLine("topPanelTexts:");
                if (inventory.topPanelTexts != null)
                {
                    for (int i = 0; i < inventory.topPanelTexts.Length; i++)
                    {
                        sb.AppendLine($"  [{i}]: {GetRefName(inventory.topPanelTexts[i])}");
                    }
                }
                else
                {
                    sb.AppendLine("  (null)");
                }

                sb.AppendLine($"tabsBar: {GetRefName(inventory.tabsBar)}");
                sb.AppendLine();
                sb.AppendLine("==========================================================================");
                sb.AppendLine();
            }

            // Find SkillTabUI
            var skillTab = Object.FindObjectOfType<SkillTabUI>();
            if (skillTab == null)
            {
                sb.AppendLine("WARNING: SkillTabUI component not found in the active scene!");
            }
            else
            {
                sb.AppendLine(">>> SKILL TAB UI COMPONENT REFERENCES <<<");
                sb.AppendLine($"GameObject Name: {skillTab.name}");
                sb.AppendLine($"Path: {GetHierarchyPath(skillTab.transform)}");
                sb.AppendLine();
                sb.AppendLine($"basicSkillButton: {GetRefName(skillTab.basicSkillButton)}");
                sb.AppendLine($"basicSkillText: {GetRefName(skillTab.basicSkillText)}");
                sb.AppendLine($"movementSkillButton: {GetRefName(skillTab.movementSkillButton)}");
                sb.AppendLine($"movementSkillText: {GetRefName(skillTab.movementSkillText)}");
                sb.AppendLine($"slot1SkillButton: {GetRefName(skillTab.slot1SkillButton)}");
                sb.AppendLine($"slot1SkillText: {GetRefName(skillTab.slot1SkillText)}");
                sb.AppendLine($"slot2SkillButton: {GetRefName(skillTab.slot2SkillButton)}");
                sb.AppendLine($"slot2SkillText: {GetRefName(skillTab.slot2SkillText)}");
                sb.AppendLine($"selectionModal: {GetRefName(skillTab.selectionModal)}");
                sb.AppendLine($"branchModal: {GetRefName(skillTab.branchModal)}");
                sb.AppendLine();
                sb.AppendLine("==========================================================================");
                sb.AppendLine();
            }

            // Hierarchy Dump - Canvas Level
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                sb.AppendLine(">>> CANVAS DETAILED HIERARCHY <<<");
                DumpHierarchy(canvas.gameObject, sb, "");
            }
            else
            {
                sb.AppendLine("WARNING: Main Canvas not found in scene!");
            }

            // Write report to file
            string outputPath = Path.Combine(Application.dataPath, "Celestial-Cross/InventoryUI_SceneReference.txt");
            File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);

            Debug.Log($"Inventory & Abilities Inspection completed successfully! File saved at: {outputPath}");
            AssetDatabase.Refresh();
        }

        private static string GetHierarchyPath(Transform t)
        {
            if (t == null) return "None";
            string path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }

        private static string GetRefName(Component c)
        {
            if (c == null) return "NULL";
            return $"{c.name} ({c.GetType().Name}) [Path: {GetHierarchyPath(c.transform)}]";
        }

        private static string GetRefName(GameObject go)
        {
            if (go == null) return "NULL";
            return $"{go.name} (GameObject) [Path: {GetHierarchyPath(go.transform)}]";
        }

        private static void DumpHierarchy(GameObject go, StringBuilder sb, string indent)
        {
            if (go == null) return;

            var rt = go.GetComponent<RectTransform>();
            string rectInfo = "";
            if (rt != null)
            {
                rectInfo = $" [Anch: min={rt.anchorMin}, max={rt.anchorMax} | Piv: {rt.pivot} | Pos: {rt.anchoredPosition} | Size: {rt.sizeDelta}]";
            }

            var components = go.GetComponents<Component>();
            var compNames = new List<string>();
            foreach (var c in components)
            {
                if (c != null)
                {
                    compNames.Add(c.GetType().Name);
                }
            }
            string compInfo = string.Join(", ", compNames);

            sb.AppendLine($"{indent}- {go.name} (Active: {go.activeInHierarchy}){rectInfo} [{compInfo}]");

            for (int i = 0; i < go.transform.childCount; i++)
            {
                DumpHierarchy(go.transform.GetChild(i).gameObject, sb, indent + "  ");
            }
        }
    }
}
